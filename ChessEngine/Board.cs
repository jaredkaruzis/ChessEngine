using System.Text;

namespace ChessEngine; 

public class Board {
    public Square[,] Squares { get; } = new Square[8, 8];
    public List<Piece> Pieces { get; } = new List<Piece>();
    public List<Move> MoveHistory { get; } = new List<Move>();
    public List<string> PositionHistory { get; } = new List<string>();

    public Dictionary<string, string> Tags { get; } = new Dictionary<string, string>();

    public int MoveCount { get; private set; }
    public Color CurrentTurn => MoveCount % 2 == 0 ? Color.White : Color.Black;
    public Color EnemyTurn => MoveCount % 2 == 1 ? Color.White : Color.Black;

    public int FiftyMoveCounter { get; private set; } = 0;
    public bool GameOver { get; private set; }
    public string? GameOverMessage { get; private set; }
    public Color Winner { get; private set; } = Color.NoColor;

    public Square EnpassantSquare { get; private set; }
    public Piece EnpassantPawn { get; private set; }

    public Board() {
        LoadFromFEN(DefaultBoard);
        Refresh();
    }

    public Board(string FEN) {
        LoadFromFEN(FEN);
        Refresh();
    }
    
    public Board(string PGN, bool pgn = true) {
        LoadFromFEN(DefaultBoard);
        Refresh();
        LoadFromPGN(PGN);
        Refresh();
    }

    // Indexer, allows reference to squares by coordinate 
    public Square this[int x, int y] {
        get => Squares[x, y];
    }

    /// <summary>
    /// Submits <paramref name="move"/>.
    /// Move will only be executed if it is valid.
    /// </summary>
    /// <param name="move"></param>
    /// <returns>
    /// If the move is executed, return true. Else, return false.
    /// </returns>
    public bool SubmitMove(Move move) {

        // If the squares aren't defined by reference, we gotta find the squares
        if (move.Origin == null || move.Destination == null) {

            // Algebraic Notation (using PGN / SAN, should handle UCI as well)
            if (!string.IsNullOrEmpty(move.AlgebraicNotation)) {
                move = ProcessAlgebraicNotationMove(move.AlgebraicNotation);
            }

            // When the two square are designated by coordinates seperately.
            else if (!string.IsNullOrEmpty(move.OriginSquare) && !string.IsNullOrEmpty(move.DestinationSquare)) {
                int originX = _notationDictionary[move.OriginSquare[0]];
                int originY = _notationDictionary[move.OriginSquare[1]];
                int destinationX = _notationDictionary[move.DestinationSquare[0]];
                int destinationY = _notationDictionary[move.DestinationSquare[1]];
                move.Origin = Squares[originX, originY];
                move.Destination = Squares[destinationX, destinationY];
            }
        }

        // If we have two squares, recheck that the squares represent a valid move
        if (move.Origin != null && move.Destination != null) {
            if (move.Origin.HasPiece && move.Origin.Piece.Color == CurrentTurn && move.Origin.Piece.Moves.Contains(move.Destination)) {
                ExecuteMove(move);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Accepts a submitted <paramref name="algebraicNotationMove"/>.
    /// Move will only be executed if it is valid.
    /// </summary>
    /// <returns>
    /// If the move is executed, return true. Else, return false.
    /// </returns>
    public bool SubmitMove(string algebraicNotationMove) {
        return SubmitMove(new Move(algebraicNotationMove));
    }

    /// <summary>
    /// Accepts a submitted move in the form of: 
    /// <paramref name="origin"/> square coordinate, 
    /// <paramref name="destination"/> square coordinate, with optional <paramref name="promotionPieceType"/>.
    /// Move will only be executed if it is valid.
    /// </summary>
    /// <returns>
    /// If the move is executed, return true. Else, return false.
    /// </returns>
    public bool SubmitMove(string origin, string destination, PieceType promotionPieceType = PieceType.Queen) {
        return SubmitMove(new Move(origin, destination, promotionPieceType));
    }

    /// <summary>
    /// Accepts a submitted move in the form of: 
    /// <paramref name="origin"/> square, 
    /// <paramref name="destination"/> square, with optional <paramref name="promotionPieceType"/>.
    /// Move will only be executed if it is valid.
    /// </summary>
    /// <returns>
    /// If the move is executed, return true. Else, return false.
    /// </returns>
    public bool SubmitMove(Square origin, Square destination, PieceType promotionPieceType = PieceType.Queen) {
        return SubmitMove(new Move(origin, destination, promotionPieceType));
    }

    /// <summary>
    /// Executes a submitted move. Only valid submitted moves will be executed. 
    /// Moves the piece in <paramref name="origin"/> to <paramref name="destination"/> and advances play to the next player's turn.
    /// This function handles updating various board states based on the move, including en passant, promotion, castling,
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    private void ExecuteMove(Move move) {
        Square origin = move.Origin;
        Square destination = move.Destination;

        // Remove captured piece from the board
        if (destination.HasPiece) {
            Pieces.Remove(destination.Piece);
        }

        // Increment 50 move count if necessary
        if (destination.HasPiece || origin.Piece.IsPawn) {
            FiftyMoveCounter = 0;
        }
        else FiftyMoveCounter++;

        // Check for special moves, we need to handle these properly
        var doublePawnMove = origin.Piece.IsPawn && Math.Abs(origin.Y - destination.Y) > 1;
        var castling = origin.Piece.IsKing && Math.Abs(origin.X - destination.X) > 1;

        // Handle Castling (move rook)
        if (castling) {
            var right = (destination.X > origin.X);
            var rookX = right ? 7 : 0;     // Rooks on the sides of the board
            var rookDestinationX = (right) ? origin.X + 1 : origin.X - 1;
            var rookSquare = Squares[rookX, origin.Y];    // Don't check for validity, Piece.King already did this
            var rook = rookSquare.Piece;
            rook.Square = Squares[rookDestinationX, origin.Y];
            rook.Square.Piece = rook;
            rookSquare.Piece = null;
        }

        origin.Piece.Move(destination);

        // Handle Enpassant (capture pawn or clear flags)
        if (EnpassantSquare != null) {
            if (destination.EnpassantFlag) {
                EnpassantPawn.Square.Piece = null;
                Pieces.Remove(EnpassantPawn);
            }
            EnpassantSquare.EnpassantFlag = false;
            EnpassantSquare = null;
            EnpassantPawn = null;
        }

        // Handle double pawn moves (store piece and flag square for enpassant)
        if (doublePawnMove) {
            var flagSquare = Squares[origin.X, destination.Y + (destination.Piece.IsWhite ? 1 : -1)];
            flagSquare.EnpassantFlag = true;
            EnpassantSquare = flagSquare;
            EnpassantPawn = destination.Piece;
        }

        // This pawn needs a promotion!
        if (destination.Piece.IsPawn && ((destination.Y == 0 && destination.Piece.IsWhite) ||
                                         (destination.Y == 7 && destination.Piece.IsBlack))) {
            Pieces.Remove(destination.Piece);
            Piece newPiece;
            switch (move.PromotionPieceType) {
                case (PieceType.Knight):
                    newPiece = new Knight(destination.Piece.Color, destination);
                    break;
                case (PieceType.Bishop):
                    newPiece = new Bishop(destination.Piece.Color, destination);
                    break;
                case (PieceType.Rook):
                    newPiece = new Rook(destination.Piece.Color, destination);
                    break;
                default:
                    newPiece = new Queen(destination.Piece.Color, destination);
                    break;
            }
            Pieces.Add(newPiece);
            destination.Piece = null;
            destination.Piece = newPiece;
        }

        MoveCount++;
        MoveHistory.Add(move);
        Refresh();

        if (GameOver) {
            // stuff
        }
    }

    private bool CheckGameOver() {

        // If there aren't any moves, its checkmate or stalemate
        if (Pieces.All(p => p.Moves.Count == 0)) {
            GameOver = true;
            if (GetHostileSquares(EnemyTurn).Any(s => s.HasPiece && s.Piece.IsKing)) {
                GameOverMessage = "Checkmate";
                Winner = EnemyTurn;
            }
            else {
                GameOverMessage = "Stalemate";
                Winner = Color.NoColor;
            }
        }

        // 50-move draw 
        if (FiftyMoveCounter >= 50) {
            GameOver = true;
            GameOverMessage = "50-move rule";
            Winner = Color.NoColor;
        }

        // Threefold-repetition draw
        var position = ExtractMinimalBoardRepresentationFromFEN(ExportFEN());
        if (PositionHistory.Count(x => x == position) >= 3) {
            GameOver = true;
            GameOverMessage = "Three-fold repetition";
            Winner = Color.NoColor;
        }
        PositionHistory.Add(position);

        // Insufficient material
        // TODO

        return GameOver;
    }

    /// <summary>
    /// Simulates a proposed move from <paramref name="origin"/> to <paramref name="destination"/>
    /// to determine if it puts the current player in check
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public bool TryMove(Square origin, Square destination) {

        Piece capturedPiece = null;
        Square enpassantSquare = null;

        if (destination.Piece != null) {
            capturedPiece = destination.Piece;
        }
        if (destination.EnpassantFlag) {  // Handle enpassant moves here
            capturedPiece = EnpassantPawn;
            enpassantSquare = capturedPiece.Square;
            enpassantSquare.Piece = null;
        }

        destination.Piece = origin.Piece;
        destination.Piece.Square = destination;

        bool preHasMoved = destination.Piece.HasMoved;  // Save hasMoved to reset to previous value

        destination.Piece.HasMoved = true;
        origin.Piece = null;

        MoveCount += 1;

        var attackedSquares = GetHostileSquares(CurrentTurn, exclude: capturedPiece);

        // here is the result, just have to clean up
        bool kingInCheck = attackedSquares.Any(x => x.HasPiece && x.Piece.IsKing && !x.Piece.IsColor(CurrentTurn));

        origin.Piece = destination.Piece;
        origin.Piece.Square = origin;
        origin.Piece.HasMoved = preHasMoved;

        if (enpassantSquare == null) {
            destination.Piece = capturedPiece;  // null if no piece
        }
        else {
            destination.Piece = null;
            enpassantSquare.Piece = capturedPiece;
        }

        MoveCount -= 1;

        return !kingInCheck;    // if the king was in check, the move failed
    }

    /// <summary>
    /// Returns all squares that pieces of Color <paramref name="color"/> can attack. Optionally
    /// will ignore moves made by piece <paramref name="exclude"/>
    /// </summary>
    /// <param name="color"></param>
    /// <param name="exclude"></param>
    /// <returns></returns>
    public List<Square> GetHostileSquares(Color color, Piece? exclude = null) {
        var squares = new List<Square>();
        foreach (var piece in Pieces.Where(x => x != exclude && x.IsColor(color))) {
            squares.AddRange(piece.GeneratePossibleMoves(this, stopRecurse: true));
        }
        return squares;
    }

    /// <summary>
    /// Returns a list of moves available for the current player
    /// </summary>
    /// <returns></returns>
    public List<Move> GetMoves() {
        var moves = new List<Move>();
        foreach (var piece in Pieces.Where(p => p.Color == CurrentTurn)) {
            foreach (var move in piece.Moves) {
                moves.Add(new Move(piece.Square, move));
            }
        }
        return moves;
    }

    /// <summary>
    /// After making a move, we need to reset some data structures and some other various housekeeping
    /// </summary>
    private void Refresh() {
        foreach (var piece in Pieces) {
            piece.Refresh(this);
        }
        CheckGameOver();
    }

    public static string DefaultBoard = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Convert Board State to FEN
    public string ExportFEN() {

        var sb = new StringBuilder();
        int emptySquareCount = 0;

        // represent squares and pieces
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                var square = Squares[j, i];
                if (square.IsEmpty) {
                    emptySquareCount++;
                    continue;
                }
                else {
                    if (emptySquareCount > 0) {
                        sb.Append(emptySquareCount);
                        emptySquareCount = 0;
                    }
                    switch (square.Piece.Type) {
                        case (PieceType.Pawn):
                            sb.Append(square.Piece.IsWhite ? "P" : "p");
                            break;
                        case (PieceType.Knight):
                            sb.Append(square.Piece.IsWhite ? "N" : "n");
                            break;
                        case (PieceType.Bishop):
                            sb.Append(square.Piece.IsWhite ? "B" : "b");
                            break;
                        case (PieceType.Rook):
                            sb.Append(square.Piece.IsWhite ? "R" : "r");
                            break;
                        case (PieceType.Queen):
                            sb.Append(square.Piece.IsWhite ? "Q" : "q");
                            break;
                        case (PieceType.King):
                            sb.Append(square.Piece.IsWhite ? "K" : "k");
                            break;

                    }
                }
            }
            if (emptySquareCount > 0) {
                sb.Append(emptySquareCount);
                emptySquareCount = 0;
            }
            if (i < 7) {
                sb.Append("/");
            }
            else {
                sb.Append(" ");
            }
        }

        // CURRENT TURN
        sb.Append(CurrentTurn == Color.White ? "w " : "b ");

        // CASTLING RIGHTS
        bool blackKingSide = false, blackQueenSide = false, whiteKingSide = false, whiteQueenSide = false;

        if (Squares[4, 0].HasPiece && Squares[7, 0].HasPiece) {
            var blackKingSideRook = Squares[7, 0].Piece;
            var blackKing = Squares[4, 0].Piece;
            if (!blackKingSideRook.HasMoved && !blackKing.HasMoved) {
                blackKingSide = true;
            }
        }
        if (Squares[4, 0].HasPiece && Squares[0, 0].HasPiece) {
            var blackQueenSideRook = Squares[0, 0].Piece;
            var blackKing = Squares[4, 0].Piece;
            if (!blackQueenSideRook.HasMoved && !blackKing.HasMoved) {
                blackQueenSide = true;
            }
        }
        if (Squares[4, 7].HasPiece && Squares[7, 7].HasPiece) {
            var whiteKingSideRook = Squares[7, 7].Piece;
            var whiteKing = Squares[4, 7].Piece;
            if (!whiteKingSideRook.HasMoved && !whiteKing.HasMoved) {
                whiteKingSide = true;
            }
        }
        if (Squares[4, 7].HasPiece && Squares[0, 7].HasPiece) {
            var whiteQueenSideRook = Squares[0, 7].Piece;
            var whiteKing = Squares[4, 7].Piece;
            if (!whiteQueenSideRook.HasMoved && !whiteKing.HasMoved) {
                whiteQueenSide = true;
            }
        }
        if (!blackKingSide && !blackQueenSide && !whiteKingSide && !whiteQueenSide) {
            sb.Append("- ");
        }
        else sb.Append($"{(whiteKingSide ? "K" : "")}{(whiteQueenSide ? "Q" : "")}{(blackKingSide ? "k" : "")}{(blackQueenSide ? "q" : "")} ");

        // ENPASSANT SQUARE
        if (EnpassantSquare == null) {
            sb.Append("- ");
        }
        else {
            sb.Append($"{EnpassantSquare.AlgebraicCoordinate} ");
        }

        // HALF MOVES TOWARDS 50 MOVE DRAW
        sb.Append($"{FiftyMoveCounter} ");

        // MOVE COUNT FULL MOVES
        sb.Append($"{(MoveCount / 2) + 1}");

        return sb.ToString();
    }

    // Convert Board State to PGN
    public string ExportPGN() {
        var sb = new StringBuilder();

        if (Tags.Count > 0) {
            foreach(var tag in Tags) {
                sb.AppendLine($"[{tag.Key} {tag.Value}]");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // Load Board State from FEN
    public void LoadFromFEN(string fen) {

        // create all square objects 
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                Squares[x, y] = new Square(x, y);
            }
        }
        var lines = fen.Split('/');

        // first 8 lines are the board
        for (int i = 0; i < 8; i++) {
            var charArray = lines[i].ToCharArray();
            var rowCounter = 0;
            foreach (var c in charArray) {
                if (char.IsLetter(c)) {
                    switch (c) {
                        case 'p':
                            Pieces.Add(new Pawn(Color.Black, Squares[rowCounter, i]));
                            Squares[rowCounter, i].Piece.HasMoved = !(Squares[rowCounter, i].Piece.Y == 1);
                            break;
                        case 'P':
                            Pieces.Add(new Pawn(Color.White, Squares[rowCounter, i]));
                            Squares[rowCounter, i].Piece.HasMoved = !(Squares[rowCounter, i].Piece.Y == 6);
                            break;
                        case 'b':
                            Pieces.Add(new Bishop(Color.Black, Squares[rowCounter, i]));
                            break;
                        case 'B':
                            Pieces.Add(new Bishop(Color.White, Squares[rowCounter, i]));
                            break;
                        case 'n':
                            Pieces.Add(new Knight(Color.Black, Squares[rowCounter, i]));
                            break;
                        case 'N':
                            Pieces.Add(new Knight(Color.White, Squares[rowCounter, i]));
                            break;
                        case 'r':
                            Pieces.Add(new Rook(Color.Black, Squares[rowCounter, i]));
                            break;
                        case 'R':
                            Pieces.Add(new Rook(Color.White, Squares[rowCounter, i]));
                            break;
                        case 'q':
                            Pieces.Add(new Queen(Color.Black, Squares[rowCounter, i]));
                            break;
                        case 'Q':
                            Pieces.Add(new Queen(Color.White, Squares[rowCounter, i]));
                            break;
                        case 'k':
                            Pieces.Add(new King(Color.Black, Squares[rowCounter, i]));
                            break;
                        case 'K':
                            Pieces.Add(new King(Color.White, Squares[rowCounter, i]));
                            break;

                    }
                    rowCounter++;
                    if (rowCounter > 7) {
                        break;
                    }
                    continue;
                }
                if (char.IsDigit(c)) {
                    var number = int.Parse(c.ToString()); //Fix
                    rowCounter += number;
                    if (rowCounter > 7) {
                        break;
                    }
                    continue;
                }
                Console.WriteLine($"Error Loading FEN at coordinates {rowCounter}, {i}");
            }
        }
        // Last line contains other board values: CurrentPlayer, CastlingRights, EnpassantSquares, HalfmovesToDraw, TotalFullMoves
        var lastLine = lines[7].Split(' ');
        var currentTurn = Color.NoColor;
        for (int i = 0; i < lastLine.Length; i++) {
            switch (i) {
                case (1):    // PLAYER TURN
                    if (lastLine[i].Contains("w")) {
                        currentTurn = Color.White;
                    }
                    else if (lastLine[i].Contains("b")) {
                        currentTurn = Color.Black;
                    }
                    break;
                case (2):   // CASTLING RIGHTS
                    if (lastLine[i].Contains("-")) {
                        foreach (var piece in Pieces) {
                            if (piece.IsKing) {
                                piece.HasMoved = true;
                            }
                        }
                    }
                    else {
                        // Black Queenside
                        if (lastLine[i].Contains("q")) {
                            Squares[0, 0].Piece.HasMoved = false;
                            Squares[4, 0].Piece.HasMoved = false;
                        }
                        else if (Squares[0, 0].HasPiece && Squares[0, 0].Piece.IsRook) {
                            Squares[0, 0].Piece.HasMoved = true;
                        }
                        // White Queenside
                        if (lastLine[i].Contains("Q")) {
                            Squares[0, 7].Piece.HasMoved = false;
                            Squares[4, 7].Piece.HasMoved = false;
                        }
                        else if (Squares[0, 7].HasPiece && Squares[0, 7].Piece.IsRook) {
                            Squares[0, 7].Piece.HasMoved = true;
                        }
                        // Black Kingside
                        if (lastLine[i].Contains("k")) {
                            Squares[7, 0].Piece.HasMoved = false;
                            Squares[4, 0].Piece.HasMoved = false;
                        }
                        else if (Squares[7, 0].HasPiece && Squares[7, 0].Piece.IsRook) {
                            Squares[7, 0].Piece.HasMoved = true;
                        }
                        // White Kingside
                        if (lastLine[i].Contains("K")) {
                            Squares[7, 7].Piece.HasMoved = false;
                            Squares[4, 7].Piece.HasMoved = false;
                        }
                        else if (Squares[7, 7].HasPiece && Squares[7, 7].Piece.IsRook) {
                            Squares[7, 7].Piece.HasMoved = true;
                        }
                    }
                    break;
                case (3):   // ENPASSANT SQUARE
                    var coords = lastLine[i].ToCharArray();
                    if (coords[0] == '-') {
                        break;
                    }
                    else {
                        var x = _notationDictionary[coords[0]];
                        var y = _notationDictionary[coords[1]];
                        EnpassantSquare = Squares[x, y];
                        EnpassantSquare.EnpassantFlag = true;
                        if (y == 2) {
                            EnpassantPawn = Squares[x, 3].Piece;
                        }
                        else if (y == 5) {
                            EnpassantPawn = Squares[x, 4].Piece;
                        }
                    }
                    break;
                case (4):   // TODO: HALFMOVE COUNT at this point dont bother
                    FiftyMoveCounter = int.Parse(lastLine[i]);
                    break;
                case (5):   // TURN COUNT IN FULL MOVES
                    var turnCount = int.Parse(lastLine[i]);
                    MoveCount = ((turnCount - 1) * 2) + (currentTurn == Color.White ? 0 : 1);
                    break;

            }
        }
    }

    // Load Board State from PGN
    private void LoadFromPGN(string pgn) {
        var index = pgn.Contains(']') ? pgn.LastIndexOf(']') : 0;

        var gameMoves = pgn.Substring(index).Trim();
        var tags = pgn.Substring(0, index).Trim();

        var tagTokens = tags.Split(']');    // this or new line, need to test
        foreach (var t in tagTokens) {
            var words = t.Split(' ');   
            var tagTitle = words[0];
            var tagDescription = String.Join(' ', words.Take(new Range(1, words.Length)));
            Tags.Add(tagTitle, tagDescription);
        }
        
        var moveTokens = gameMoves.Split(' ');
        var parsingComment = false;         // ignore comments for now, it will break if you use a semicolon like a mongoloid
        foreach (var t in moveTokens) {
            
            if (t.Contains('{')) {          
                parsingComment = true;  // Comment starting 
            }

            if (t.Contains('}')) {
                parsingComment = false; // Comment ending
                continue;
            }

            if (parsingComment) {       // skip comment
                continue;
            }

            if (t.Contains("1-0") || t.Contains("0-1") || t.Contains("1/2-1/2")) {
                continue;               // this is just the result we can ignore this, i think we can figure it out lol
            }

            if (!t.Contains('.')) {
                SubmitMove(t);
            }
        }
    }

    // Returns a subset from FEN which is used to compare board states for a threefold repetition
    private string ExtractMinimalBoardRepresentationFromFEN(string FEN) {
        return string.Join(" ", FEN.Split(' ').Take(4));
    }

    // Conversion to the standard chess coordinates. Our board: (0,0) = A8, (7,7) = H1
    private static string[] _xConversions = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };
    private static string[] _yConversions = new string[] { "8", "7", "6", "5", "4", "3", "2", "1" };
    public static string ConvertCoordinates(int x, int y) {
        return _xConversions[x] + _yConversions[y];
    }

    private static Dictionary<char, int> _notationDictionary = new Dictionary<char, int>() {
        { 'a', 0 },
        { 'b', 1 },
        { 'c', 2 },
        { 'd', 3 },
        { 'e', 4 },
        { 'f', 5 },
        { 'g', 6 },
        { 'h', 7 },
        { '8', 0 },
        { '7', 1 },
        { '6', 2 },
        { '5', 3 },
        { '4', 4 },
        { '3', 5 },
        { '2', 6 },
        { '1', 7 },
    };

    private Move ProcessAlgebraicNotationMove(string algebraicNotation) {
        var thisMove = new Move();

        // Handle castling moves
        algebraicNotation = algebraicNotation.Replace('O', '0');    // Sometimes they use Ohs instead of Zeros
        if (algebraicNotation.Contains("0-0")) {
            var castleCol = algebraicNotation.Contains("0-0-0") ? 2 : 6;    // Long or Short castle go to different squares
            var moves = GetMoves().Where(move => move.Origin.Piece.IsKing && move.Destination.X == castleCol);
            if (moves.Count() == 1) {
                return moves.First();
            }
            else throw new Exception();
        }

        // All the information that the algebraic move can contain, we use these to filter our results if that info is available.
        Square destinationSquare = thisMove.Destination;
        Square originSquare = thisMove.Origin;
        var destinationIndex = -1;
        var originPieceType = PieceType.Empty;
        var isCapture = algebraicNotation.Contains('x');
        char resolveAmbiguityRow = ' ';
        char resolveAmbiguityColumn = ' ';

        // If the last digit isn't a number, there is something going on.
        if (!char.IsDigit(algebraicNotation[algebraicNotation.Length - 1])) {

            // Walk backwards from the end until we hit a number, which will be the destination square.
            for (int i = 1; i < algebraicNotation.Length; i++) {
                if (char.IsDigit(algebraicNotation[algebraicNotation.Length - 1 - i])) {
                    var promotion = algebraicNotation.Substring(algebraicNotation.Length - i);
                    if (promotion.Contains("Q")) {
                        thisMove.PromotionPieceType = PieceType.Queen;
                    }
                    else if (promotion.Contains("B")) {
                        thisMove.PromotionPieceType = PieceType.Bishop;
                    }
                    else if (promotion.Contains("N")) {
                        thisMove.PromotionPieceType = PieceType.Knight;
                    }
                    else if (promotion.Contains("R")) {
                        thisMove.PromotionPieceType = PieceType.Rook;
                    }
                    var x = _notationDictionary[algebraicNotation[algebraicNotation.Length - 2 - i]];
                    var y = _notationDictionary[algebraicNotation[algebraicNotation.Length - 1 - i]];

                    destinationSquare = Squares[x, y];
                    destinationIndex = algebraicNotation.Length - 2 - i;
                    break;
                }
                else continue;
            }
        }
        // if the last digit is a number, its definitely the destination. No ambiguity.
        else {
            var destinationX = _notationDictionary[algebraicNotation[algebraicNotation.Length - 2]];
            var destinationY = _notationDictionary[algebraicNotation[algebraicNotation.Length - 1]];
            destinationSquare = Squares[destinationX, destinationY];
            destinationIndex = algebraicNotation.Length - 2;
        }

        // destinationIndex being 0 indicates the piece is a pawn
        if (destinationIndex == 0) {
            originPieceType = PieceType.Pawn;
        }
        else {
            // Start back at the start. First digit will be the type if not a pawn.
            var index = 0;

            switch (algebraicNotation[index]) {
                case ('N'):
                    originPieceType = PieceType.Knight;
                    index++;
                    break;
                case ('B'):
                    originPieceType = PieceType.Bishop;
                    index++;
                    break;
                case ('R'):
                    originPieceType = PieceType.Rook;
                    index++;
                    break;
                case ('Q'):
                    originPieceType = PieceType.Queen;
                    index++;
                    break;
                case ('K'):
                    originPieceType = PieceType.King;
                    index++;
                    break;
                default:
                    originPieceType = PieceType.Pawn;
                    break;
            }

            while (index != destinationIndex && algebraicNotation[index] != 'x') {
                if (char.IsLetter(algebraicNotation[index])) {
                    resolveAmbiguityColumn = algebraicNotation[index];
                    index++;
                }
                if (char.IsDigit(algebraicNotation[index])) {
                    resolveAmbiguityRow = algebraicNotation[index];
                    index++;
                }
            }
        }
        // One guaranteed piece of information we have is the destination square
        var possibleMoves = GetMoves().Where(move => move.Destination == destinationSquare);

        // Filter piece type
        if (originPieceType != PieceType.Empty) {
            possibleMoves = possibleMoves.Where(move => move.Origin.Piece.Type == originPieceType);
        }

        // Filter captures
        if (isCapture) {
            possibleMoves = possibleMoves.Where(move => move.Destination.HasPiece || move.Destination.EnpassantFlag).ToList();
        }
        else {
            possibleMoves = possibleMoves.Where(move => move.Destination.IsEmpty).ToList();
        }

        // Filter by origin square if possible
        if (resolveAmbiguityColumn != ' ') {
            possibleMoves = possibleMoves.Where(move => _xConversions[move.Origin.X][0] == resolveAmbiguityColumn).ToList();
        }
        if (resolveAmbiguityRow != ' ') {
            possibleMoves = possibleMoves.Where(move => _yConversions[move.Origin.Y][0] == resolveAmbiguityRow).ToList();
        }

        // Something went wrong.
        if (possibleMoves.Count() != 1) {
            if (possibleMoves.Count() > 1) { throw new Exception("Too much ambiguity in algebraic move."); }
            else throw new Exception("Invalid move");
        }
        else {
            var m = possibleMoves.First();
            m.PromotionPieceType = thisMove.PromotionPieceType;
            return possibleMoves.First();
        }
    }

    public bool TryGetSquare(int x, int y, out Square square) {
        square = CheckCoordinates(x, y) ? this[x, y] : null;
        return (square != null);
    }

    private bool CheckCoordinates(int x, int y) {
        return !(x > 7 || x < 0 || y > 7 || y < 0); // not beyond bounds
    }

    public override string ToString() {
        return ExportFEN();
    }
}
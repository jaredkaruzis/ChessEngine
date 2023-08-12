namespace ChessEngine; 

public enum Color {
    NoColor = -1,
    White = 0,
    Black = 1,
}

public enum PieceType {
    Empty = -1,
    Pawn = 0,
    Knight,
    Bishop,
    Rook,
    Queen,
    King,
}

public abstract class Piece {

    public Color Color;
    public Color EnemyColor => Color == Color.White ? Color.Black : Color.White;

    public bool IsWhite => Color == Color.White;
    public bool IsBlack => Color == Color.Black;

    public bool IsPawn => Type == PieceType.Pawn;
    public bool IsKing => Type == PieceType.King;
    public bool IsRook => Type == PieceType.Rook;

    public bool IsColor(Color c) => c == Color;

    public PieceType Type;

    public Square Square;

    public int X => Square.X;
    public int Y => Square.Y;

    public List<Square> Moves = new List<Square>();

    public abstract List<int[]> Movements { get; }

    public bool HasMoved;

    public Piece(Color color, Square square) {
        Color = color;
        Square = square;
        Square.Piece = this;
    }

    public void Refresh(Board board) {
        Moves = GeneratePossibleMoves(board);
    }

    public virtual void Move(Square s) {
        Square.Piece = null;
        Square = s;
        Square.Piece = this;
        HasMoved = true;
    }

    /// <summary>
    /// Default implementation of move generation. 
    /// Only works for Rook, Bishop, and Queen; other pieces require more finesse
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public virtual List<Square> GeneratePossibleMoves(Board board, bool stopRecurse = false) {
        var newMoves = new List<Square>();

        if (!stopRecurse && board.CurrentTurn != Color) {
            return newMoves;
        }

        foreach (var dir in Movements) {
            bool stopDirection = false; // flag that we can't move in this direction anymore
            for (int i = 1; i < 8; i++) {   
                var x = X + (dir[0] * i);
                var y = Y + (dir[1] * i);
                if (board.TryGetSquare(x, y, out Square destination)) {
                    if (destination.HasPiece) {
                        if (destination.Piece.IsColor(EnemyColor)) {    // If there is a piece here, it has to be enemy
                            stopDirection = true;                       // we still have other stuff to check, though
                        }
                        else break;       // This square is friendly, and this piece can't move through other pieces 
                    }
                    // If this is a recursive call, don't check board for move validity. We only care about Kings in check.
                    if (stopRecurse || board.TryMove(Square, destination)) {
                        newMoves.Add(destination);
                    }
                    if (stopDirection) {
                        break;
                    }
                } else break; 
            }
        }
        return newMoves;
    }
}
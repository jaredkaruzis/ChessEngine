namespace ChessEngine; 

public class Pawn : Piece {

    private List<int[]> _movements;
    private List<int[]> _attackMoveVectors;

    public override List<int[]> Movements => _movements;

    public Pawn(Color color, Square square) : base(color, square) {
        GenerateMovements();
        Type = PieceType.Pawn;
        Color = color;
        Square = square;
        Square.Piece = this;
    }

    public override List<Square> GeneratePossibleMoves(Board board, bool stopRecurse = false) {

        var newMoves = new List<Square>();

        if (!stopRecurse && board.CurrentTurn != Color) {
            return newMoves;
        }

        foreach (var dir in Movements) {
            // Don't add these for now! TODO: change stopRecurse to checkForKing or checkChecks
            if (stopRecurse) {
                break;
            }

            // Our move forward was off the board, stop trying!
            if (!board.TryGetSquare(X + dir[0], Y + dir[1], out Square destination)) {
                break;
            }

            // If there is a piece, we can't move through it, we aren't attacking
            if (destination.HasPiece) {
                break;
            }

            if (board.TryMove(Square, destination)) {
                newMoves.Add(destination);
            }

            // If we've moved, the second move forward is invalid
            if (HasMoved) {
                break;
            }
        }

        foreach(var dir in _attackMoveVectors) {

            if (!board.TryGetSquare(X + dir[0], Y + dir[1], out Square destination)) {
                continue;
            }

            if (destination.IsEmpty) {
                if (!destination.EnpassantFlag) {
                    continue;
                }
            }
            else if (!destination.Piece.IsColor(EnemyColor)) {   // Attack only
                continue;
            }

            if (stopRecurse) {
                newMoves.Add(destination);
            }
            else if (board.TryMove(Square, destination)) {
                newMoves.Add(destination);
            }
        }
        return newMoves;
    }

    // Helper function for pawns because they are the only pieces with different movesets
    private void GenerateMovements() {

        _movements = IsWhite ? new List<int[]>() {          // white pawns walk down...
                                    new int[2] { 0, -1 },
                                    new int[2] { 0, -2 },
                                } :                         
                                new List<int[]>() {         // black pawns walk up...
                                    new int[2] { 0, 1 },
                                    new int[2] { 0, 2 },
                                };

        _attackMoveVectors = IsWhite ? new List<int[]>() {
                                        new int[2] { 1, -1 },
                                        new int[2] { -1, -1 }
                                    } :
                                    new List<int[]>() {
                                        new int[2] { 1, 1 },
                                        new int[2] { -1, 1 },
                                    };
    }

    public override string ToString() {
        return $"{Color} Pawn";
    }
}

namespace ChessEngine; 

public class Knight : Piece {

    private List<int[]> _movements { get; } = new List<int[]>() {
        new int[2]{ 2, 1 },
        new int[2]{ -2, 1 },
        new int[2]{ 2, -1 },
        new int[2]{ -2, -1 },
        new int[2]{ 1, 2 },
        new int[2]{ -1, 2 },
        new int[2]{ 1, -2 },
        new int[2]{ -1, -2 },
    };

    public override List<int[]> Movements => _movements;

    public Knight(Color color, Square square) : base(color, square) {
        Type = PieceType.Knight;
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

            var x = X + dir[0];
            var y = Y + dir[1];

            if (!board.TryGetSquare(x, y, out Square destination)) {
                continue;
            }

            if (destination.HasPiece) {
                if (!destination.Piece.IsColor(EnemyColor)) {    // Can't capture allied piece
                    continue;
                }
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

    public override string ToString() {
        return $"{Color} Knight";
    }
}

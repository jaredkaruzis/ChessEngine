namespace ChessEngine; 

public class Queen : Piece {

    private List<int[]> _movements = new List<int[]>() {
        new int[2] { 1, 0 },
        new int[2] { 0, 1 },
        new int[2] { -1, 0 },
        new int[2] { 0, -1 },
        new int[2] { 1, 1 },
        new int[2] { -1, 1 },
        new int[2] { 1, -1 },
        new int[2] { -1, -1 },
    };

    public override List<int[]> Movements => _movements;

    public Queen(Color color, Square square) : base(color, square) {
        Type = PieceType.Queen;
        Color = color;
        Square = square;
        Square.Piece = this;
    }

    public override string ToString() {
        return $"{Color} Queen";
    }
}

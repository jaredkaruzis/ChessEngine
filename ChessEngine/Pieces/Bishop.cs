namespace ChessEngine; 

public class Bishop : Piece {

    public List<int[]> _movements = new List<int[]>() {
        new int[2] { 1, 1 },
        new int[2] { -1, 1 },
        new int[2] { 1, -1 },
        new int[2] { -1, -1 },
    };

    public override List<int[]> Movements => _movements;

    public Bishop(Color color, Square square) : base(color, square) {
        Type = PieceType.Bishop;
        Color = color;
        Square = square;
        Square.Piece = this;
    }

    public override string ToString() {
        return $"{Color} Bishop";
    }
}

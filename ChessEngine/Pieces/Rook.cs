namespace ChessEngine; 

public class Rook : Piece {

    public Rook(Color color, Square square) : base(color, square) {
        Type = PieceType.Rook;
        Color = color;
        Square = square;
        Square.Piece = this;
    }

    public List<int[]> _movements = new List<int[]>() {
        new int[2]{1,  0},
        new int[2]{0,  1},
        new int[2]{-1, 0},
        new int[2]{0, -1},
    };

    public override List<int[]> Movements => _movements;

    public override string ToString() {
        return $"{Color} Rook";
    }
}

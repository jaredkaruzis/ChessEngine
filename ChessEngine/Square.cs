namespace ChessEngine;

public class Square {

    public Square(int x, int y) {
        X = x;
        Y = y;
        Piece = null;
        AlgebraicCoordinate = Board.ConvertCoordinates(X, Y);
    }

    public int X;
    public int Y;

    public string AlgebraicCoordinate;

    public Piece? Piece;

    public bool IsEmpty => Piece == null;
    public bool HasPiece => Piece != null;

    public bool EnpassantFlag;

    public override string ToString() {
        return AlgebraicCoordinate;
    }
}

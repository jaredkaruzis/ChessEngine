namespace ChessEngine; 

public class Move {

    // Define move by reference
    public Square Origin = null;
    public Square Destination = null;

    // Define move by square coordinates, move piece on origin to destination in algebraic notation (e2e4, a1h8, etc)
    public string OriginSquare = null;
    public string DestinationSquare = null;

    // Define move by algebraic notation (e4, Qh8#, etc)
    public string AlgebraicNotation = null;

    // Define piece type for promotion if applicable
    public PieceType PromotionPieceType = PieceType.Empty;

    public Move(string originSquare, string destinationSquare, PieceType promotionPieceType = PieceType.Empty) {
        OriginSquare = originSquare;
        DestinationSquare = destinationSquare;
        PromotionPieceType = promotionPieceType;
    }

    public Move(Square originSquare, Square destinationSquare, PieceType promotionPieceType = PieceType.Empty) {
        Origin = originSquare;
        OriginSquare = Origin.AlgebraicCoordinate;
        Destination = destinationSquare;
        DestinationSquare = Destination.AlgebraicCoordinate;
        PromotionPieceType = promotionPieceType;
    }

    public Move(string algebraicNotation, PieceType PromotionPieceType = PieceType.Empty) {
        AlgebraicNotation = algebraicNotation;
    }

    public Move() { }
}

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

    public void CalculateAlgebraicMove(Board b) {
        if (Origin.Piece.IsKing && Math.Abs(Origin.X - Destination.X) > 1) {
            if (Destination.X == 2) {
                AlgebraicNotation = "0-0-0";
                return;
            }
            if (Destination.X == 6) {
                AlgebraicNotation = "0-0";
                return;
            }
        }
        var pieceString = Origin?.Piece?.Type switch {
            PieceType.Pawn => "",
            PieceType.Knight => "N",
            PieceType.Bishop => "B",
            PieceType.Rook => "R",
            PieceType.Queen => "Q",
            PieceType.King => "K",
            _ => "",
        };

        var columnAmbiguity = false;
        var rowAmbiguity = false;

        foreach (var piece in b.Pieces) {
            if (piece == Origin.Piece) continue;
            foreach (var move in piece.Moves) {
                if (move == Destination && piece.Type == Origin.Piece.Type) {
                    if (Origin.X == piece.X) {
                        columnAmbiguity = true;
                    }
                    if (Origin.Y == piece.Y) {
                        rowAmbiguity = true;
                    }
                }
            }
        }
        var resolveColumnAmbiguity = columnAmbiguity ? Origin.AlgebraicCoordinate[1].ToString() : "";
        var resolveRowAmbiguity = rowAmbiguity ? Origin.AlgebraicCoordinate[0].ToString() : "";
        var resolveAmbiguityString = $"{resolveColumnAmbiguity}{resolveRowAmbiguity}";

        var captureString = Destination.HasPiece ? "x" : "";

        var destinationString = Destination.AlgebraicCoordinate;

        //var testBoard = new Board(b.ExportFEN());
        //testBoard[Origin.X, Origin.Y].Piece.Move(testBoard[Destination.X, Destination.Y]);
        //var checks = testBoard[Destination.X, Destination.Y].Piece.GeneratePossibleMoves(testBoard).Where(s => s.Piece.Type == PieceType.King).Any();
        //var checkMate = testBoard.
        var checkString = "";   // TODO Add checks in here

        var promotionString = PromotionPieceType switch {
            PieceType.Knight => "=N",
            PieceType.Bishop => "=B",
            PieceType.Rook => "=R",
            PieceType.Queen => "=Q",
            _ => "",
        };

        AlgebraicNotation = $"{pieceString}{resolveAmbiguityString}{captureString}{destinationString}{promotionString}{checkString}";
    }

    public Move() { }
}

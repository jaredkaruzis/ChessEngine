namespace ChessEngine;

public static class AI {
    public enum LogicType {
        Random,
        Minmax,
    }

    private static Random _r = new Random();

    public static Square[] RandomMove(Board b) {
        var moves = new List<Square[]>();
        foreach (var piece in b.Pieces) {
            foreach (var destination in piece.Moves) {
                moves.Add(new Square[2] { piece.Square, destination });
            }
        }
        var grabBag = moves.Where(x => x[0].Piece.IsColor(b.CurrentTurn));
        var move = grabBag.ToList()[_r.Next(grabBag.Count())];
        return move;
    }

    // function minimax (node, depth, maximizingPlayer)
    //      if depth = 0 or terminal node (game over)
    //          return value
    //      if max Player
    //          value := −∞
    //          for each child of node do
    //              value := max(value, minimax(child, depth − 1, FALSE))
    //          return value
    //      if min Player
    //          value := +∞
    //          for each child of node do
    //              value := min(value, minimax(child, depth − 1, TRUE))
    //          return value
    public static float Minimax(Board b, int depth, float alpha, float beta, Color player) {
        var isMaxPlayer = b.CurrentTurn == player;

        if (b.GameOver) {
            if (b.Winner == player) return float.PositiveInfinity;
            if (b.Winner == Color.NoColor) return 0;
            else return float.NegativeInfinity;
        }
        if (depth == 0) return RateBoardState(b, player);

        if (isMaxPlayer) {
            var val = float.NegativeInfinity;

            foreach (var piece in b.Pieces.Where(x => x.Color == b.CurrentTurn)) {
                var possMoves = piece.Moves;
                foreach (var move in possMoves) {
                    var testBoard = DeepClone(b);
                    var originSquare = testBoard[piece.Square.X, piece.Square.Y];
                    var destinationSquare = testBoard[move.X, move.Y];
                    testBoard.SubmitMove(originSquare, destinationSquare, PieceType.Queen);
                    val = Math.Max(Minimax(testBoard, depth - 1, alpha, beta, player), val);
                    if (val >= beta) {
                        break;
                    }
                    alpha = Math.Max(alpha, val);
                }
            }
            return val;
        }
        else { // isMinPlayer
            var val = float.PositiveInfinity;

            foreach (var piece in b.Pieces.Where(x => x.Color == b.CurrentTurn)) {
                var possMoves = piece.Moves;
                foreach (var move in possMoves) {
                    var testBoard = DeepClone(b);
                    var originSquare = testBoard[piece.Square.X, piece.Square.Y];
                    var destinationSquare = testBoard[move.X, move.Y];
                    testBoard.SubmitMove(originSquare, destinationSquare, PieceType.Queen);
                    val = Math.Min(Minimax(testBoard, depth - 1, alpha, beta, player), val);
                    if (val <= alpha) {
                        break;
                    }
                    beta = Math.Min(beta, val);
                }
            }
            return val;
        }
    }

    public static Square[] MinmaxMove(Board b, int depth = 2) {
        var pieces = b.Pieces.Where(p => p.Color == b.CurrentTurn);
        var moves = new List<Move>();

        foreach (var p in pieces) {
            var destinations = p.Moves;
            foreach (var d in destinations) {
                moves.Add(new Move() { origin = p.Square, destination = d });
            }
        }

        Parallel.ForEach(moves, move => {
            var testBoard = DeepClone(b);
            var originSquare = testBoard[move.origin.X, move.origin.Y];
            var destinationSquare = testBoard[move.destination.X, move.destination.Y];
            testBoard.SubmitMove(originSquare, destinationSquare, PieceType.Queen);
            move.value = Minimax(testBoard, depth, float.NegativeInfinity, float.PositiveInfinity, b.CurrentTurn);
        });

        var bestMove = moves.OrderByDescending(m => m.value).First();

        return new Square[2] { bestMove.origin, bestMove.destination };
    }


    // TODO: Refine heuristic algorithm
    // Positive = WHITE, Negative = BLACK
    public static float RateBoardState(Board b, Color c) {
        var score = 0;

        foreach (var p in b.Pieces) {
            var side = p.Color == c ? 1 : -1;
            switch (p.Type) {
                case PieceType.Pawn:
                    score += 30 * side;
                    break;
                case PieceType.Bishop:
                    score += 60 * side;
                    break;
                case PieceType.Knight:
                    score += 60 * side;
                    break;
                case PieceType.Rook:
                    score += 100 * side;
                    break;
                case PieceType.Queen:
                    score += 900 * side;
                    break;
            }
            var squaresAttacked = p.Moves.Count;

            var moves = p.Moves;
            foreach (var move in moves) {
                if (move.HasPiece) {
                    switch (move.Piece.Type) {
                        case PieceType.Pawn:
                            score += 1 * side;
                            break;
                        case PieceType.Bishop:
                            score += 5 * side;
                            break;
                        case PieceType.Knight:
                            score += 5 * side;
                            break;
                        case PieceType.Rook:
                            score += 25 * side;
                            break;
                        case PieceType.Queen:
                            score += 45 * side;
                            break;
                        case PieceType.King:
                            score += 50 * side;
                            break;
                    }
                }
            }
            score += squaresAttacked * side;
        }
        return score;
    }

    public static Board DeepClone(Board b) {
        return new Board(b.ExportFEN());
    }

    private class Move {
        public Square destination;
        public Square origin;
        public float value;
    }
}

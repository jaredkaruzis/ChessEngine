using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

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
        if (depth == 0 || b.GameOver) return RateBoardState(b, player);

        if (isMaxPlayer) {
            var val = float.NegativeInfinity;
            foreach (var move in b.GetMoves()) {
                var testBoard = GenerateMoveBoard(b, new Move() { origin = move.Origin, destination = move.Destination });
                val = Math.Max(Minimax(testBoard, depth - 1, alpha, beta, player), val);
                alpha = Math.Max(alpha, val);
                if (val >= beta) {
                    break;
                }
            }
            return val;
        }
        else { // isMinPlayer
            var val = float.PositiveInfinity;
            foreach (var move in b.GetMoves()) {
                var testBoard = GenerateMoveBoard(b, new Move() { origin = move.Origin, destination = move.Destination });
                val = Math.Min(Minimax(testBoard, depth - 1, alpha, beta, player), val);
                beta = Math.Min(beta, val);
                if (val <= alpha) {
                    break;
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
            var testBoard = GenerateMoveBoard(b, move);
            move.value = Minimax(testBoard, depth, float.NegativeInfinity, float.PositiveInfinity, b.CurrentTurn);
        });

        var bestMove = moves.OrderByDescending(m => m.value).First();

        return new Square[2] { bestMove.origin, bestMove.destination };
    }

    private static Board GenerateMoveBoard(Board b, Move m) {
        var testBoard = DeepClone(b);
        var originSquare = testBoard[m.origin.X, m.origin.Y];
        var destinationSquare = testBoard[m.destination.X, m.destination.Y];
        testBoard.SubmitMove(originSquare, destinationSquare, PieceType.Queen);
        return testBoard;
    }

    public static float RateBoardState(Board b, Color c) {
        if (b.GameOver) {
            if (b.Winner == c) return float.PositiveInfinity;
            if (b.Winner == Color.NoColor) return 0;
            else return float.NegativeInfinity;
        }
        float score = 0;

        foreach (var p in b.Pieces) {
            var side = p.Color == c ? 1 : -1;
            var piece = PieceValues[p.Type];
            score += (piece * side * 100);

            var moves = p.GeneratePossibleMoves(b);

            if (p.IsPawn) {
                if (!p.HasMoved) score -= 5;
            }
            if (p.Type != PieceType.Queen && p.X > 2 && p.X < 5 && p.Y > 2 && p.Y < 5) score += 10;

            foreach (var move in moves) {
                if (move.X > 2 && move.X < 5 && move.Y > 2 && move.Y < 5) {
                    score += 10;
                }
                var targetPiece = move.HasPiece ? PieceValues[move.Piece.Type] : 1;
                score += (targetPiece * side * piece);
            }
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

    private static readonly Dictionary<PieceType, float> PieceValues = new Dictionary<PieceType, float> {
        { PieceType.Empty, 0 },
        { PieceType.Pawn, 1 },
        { PieceType.Bishop, 3 },
        { PieceType.Knight, 3 },
        { PieceType.Rook, 5 },
        { PieceType.Queen, 9 },
        { PieceType.King, 0 },
    };
}

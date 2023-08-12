namespace ChessEngine; 

public class King : Piece {

    public List<int[]> _movements = new List<int[]>() {
        new int[2] { 1, 1 },    // Bishop move vectors
        new int[2] { -1, 1 },
        new int[2] { 1, -1 },
        new int[2] { -1, -1 },
        new int[2] { 1, 0 },    // Rook move vectors
        new int[2] { 0, 1 },
        new int[2] { -1, 0 },
        new int[2] { 0, -1 },
        new int[2] {2, 0 },     // Castle moves
        new int[2] {-2, 0 },
    };

    public override List<int[]> Movements => _movements;

    public King(Color color, Square square) : base(color, square) {
        Type = PieceType.King;
        Color = color;
        Square = square;
        Square.Piece = this;
    }

    public override void Move(Square s) {
        if (!HasMoved) {
            Movements.RemoveAll(x => Math.Abs(x[0]) > 1);
            HasMoved = true;
        }
        base.Move(s);
    }

    public override List<Square> GeneratePossibleMoves(Board board, bool stopRecurse = false) {
        var newMoves = new List<Square>();

        if (!stopRecurse && board.CurrentTurn != Color) {
            return newMoves;
        }

        foreach (var dir in Movements) {

            var x = X + dir[0];
            var y = Y + dir[1];

            if (board.TryGetSquare(x, y, out Square destination)) {

                // If there is a piece present, it must not be allied
                if (destination.HasPiece) {
                    if (!destination.Piece.IsColor(EnemyColor)) {    
                        continue;
                    }
                }

                // Handle castling here (Check for validity of castling)
                if (Math.Abs(dir[0]) > 1) { //castling moves 2 spaces...

                    if (stopRecurse) {
                        continue;
                    }

                    if (Color == Color.White) {
                        if (Y != 7) continue;
                    }
                    else {
                        if (Y != 0) continue;
                    }

                    // If this King has moved, it can't castle
                    if (HasMoved) {
                        continue;
                    }

                    // Checks if the king is CURRENTLY in check
                    if (board.GetHostileSquares(EnemyColor).Contains(Square)) {
                        continue;
                    }

                    // Check that rook is in position and it hasn't moved
                    var rookSquare = (dir[0] > 1) ? board[7, y] : board[0, y];
                    if (!rookSquare.HasPiece || !rookSquare.Piece.IsRook || rookSquare.Piece.HasMoved) {
                        continue;
                    }

                    // Check each square in between king and rook for emptiness and lack of check (for each color and side)
                    if (rookSquare.X == 0 && rookSquare.Y == 7) {
                        if (board[1, 7].HasPiece || board[2, 7].HasPiece || board[3, 7].HasPiece) {
                            continue;
                        }
                        if (!board.TryMove(Square, board[1, 7]) || !board.TryMove(Square, board[2, 7]) || !board.TryMove(Square, board[3, 7])) {
                            continue;
                        }
                    }
                    if (rookSquare.X == 7 && rookSquare.Y == 7) {
                        if (board[5, 7].HasPiece || board[6, 7].HasPiece) {
                            continue;
                        }
                        if (!board.TryMove(Square, board[5, 7]) || !board.TryMove(Square, board[6, 7])) {
                            continue;
                        }
                    }
                    if (rookSquare.X == 0 && rookSquare.Y == 0) {
                        if (board[1, 0].HasPiece || board[2, 0].HasPiece || board[3, 0].HasPiece) {
                            continue;
                        }
                        if (!board.TryMove(Square, board[1, 0]) || !board.TryMove(Square, board[2, 0]) || !board.TryMove(Square, board[3, 0])) {
                            continue;
                        }
                    }
                    if (rookSquare.X == 7 && rookSquare.Y == 0) {
                        if (board[5, 0].HasPiece || board[6, 0].HasPiece) {
                            continue;
                        }
                        if (!board.TryMove(Square, board[5, 0]) || !board.TryMove(Square, board[6, 0])) {
                            continue;
                        }
                    }
                    newMoves.Add(destination);
                    continue;
                }
                // END CASTLING

                if (stopRecurse || board.TryMove(Square, destination)) {
                    newMoves.Add(destination);
                }
            }
        }
        return newMoves;
    }

    public override string ToString() {
        return $"{Color} King";
    }
}

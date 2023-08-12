using Xunit;

namespace ChessEngine;

public class ChessTests {

    private string pgn1 = "1. e4 e5 2. Nf3 Nf6 3. Nc3 Nd5 4. Nxd5 h6 5. Nf4 g6 6. Nd3 Ba3 7. Nfxe5 Bxb2 8. Bxb2 O-O 9. Qe2 Qg5 10. O-O-O Qxg2 11. f3 g5 12. f4 g4 13. f5 g3 14. f6 Qxh2 15. Nxf7 g2 16. Nxh6+ Kh8 17. f7+ Kh7 18. Rxh2 g1=Q 19. Nf5+ Kg6 20. Nh6 Rh8 21. Nf5 Rxh2 22. f8=Q Kh7 23. Qg7+ Qxg7 24. Nxg7 Rh6 25. Bg2 a6 26. Qg4 b6 27. Rh1 c6 28. Ne5 d6 29. Nf7 Bb7 30. Rxh6+ Kg8 31. Ne6+ Kxf7 32. Qg7+ Ke8 33. Rh8#";
    private string pgn2 = "[Event \"F/S Return Match\"]\r\n[Site \"Belgrade, Serbia JUG\"]\r\n[Date \"1992.11.04\"]\r\n[Round \"29\"]\r\n[White \"Fischer, Robert J.\"]\r\n[Black \"Spassky, Boris V.\"]\r\n[Result \"1/2-1/2\"]\r\n\r\n1. e4 e5 2. Nf3 Nc6 3. Bb5 a6 {This opening is called the Ruy Lopez.} 4. Ba4 Nf6 5. O-O Be7 6. Re1 b5 7. Bb3 d6 8. c3 O-O 9. h3 Nb8 10. d4 Nbd7 11. c4 c6 12. cxb5 axb5 13. Nc3 Bb7 14. Bg5 b4 15. Nb1 h6 16. Bh4 c5 17. dxe5 Nxe4 18. Bxe7 Qxe7 19. exd6 Qf6 20. Nbd2 Nxd6 21. Nc4 Nxc4 22. Bxc4 Nb6 23. Ne5 Rae8 24. Bxf7+ Rxf7 25. Nxf7 Rxe1+ 26. Qxe1 Kxf7 27. Qe3 Qg5 28. Qxg5 hxg5 29. b3 Ke6 30. a3 Kd6 31. axb4 cxb4 32. Ra5 Nd5 33. f3 Bc8 34. Kf2 Bf5 35. Ra7 g6 36. Ra6+ Kc5 37. Ke1 Nf4 38. g3 Nxh3 39. Kd2 Kb5 40. Rd6 Kc5 41. Ra6 Nf2 42. g4 Bd3 43. Re6 1/2-1/2";
    private string pgn3 = "1. e4 e5 2. d3 d6 3. Be3 Be6 4. Qf3 Qf6 5. Nd2 Nd7 6. O-O-O O-O-O 7. h4 g5 8. h5 g4 9. h6 g3 10. Nh3 Bg7 11. hxg7 gxf2 12. gxh8=Q Ne7 13. Re1 Qg7 14. Be2 fxe1=N 15. Qf2 Nxd3+ 16. Kd1 Qf8 17. Qhf6 Nxb2+ 18. Kc1 Nd3+ 19. Kd1 Ne1 20. Bxa7 Nd3 21. Nc4 Nc1 22. Nb6+ cxb6 23. Qe3 Nb3 24. Qc3+ Ndc5 25. Bxb6 Kb8 26. Qxc5 Nd4 27. Qc7+ Ka8 28. Ba6 Nb5 29. Qxb7#";

    public ChessTests() { }

    [Fact]
    public void TestDefaultLoad() {
        var b = new Board(Board.DefaultBoard);
        Assert.Equal("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", b.ExportFEN());
    }

    [Fact]
    public void TestCastling1() {
        var Kq = new Board("r3k2r/pppqbppp/2np1n1B/4p3/4P3/2NP1N1b/PPPQBPPP/R3K2R w Kq - 11 10");
        Assert.Equal("r3k2r/pppqbppp/2np1n1B/4p3/4P3/2NP1N1b/PPPQBPPP/R3K2R w Kq - 11 10", Kq.ExportFEN());
    }

    [Fact]
    public void TestCastling2() {
        var Qk = new Board("r3k2r/ppp2ppp/B1nq1n1B/3pp3/3PP3/b1NQ1N1b/PPP2PPP/R3K2R w Qk - 14 10");
        Assert.Equal("r3k2r/ppp2ppp/B1nq1n1B/3pp3/3PP3/b1NQ1N1b/PPP2PPP/R3K2R w Qk - 14 10", Qk.ExportFEN());
    }

    [Fact]
    public void TestCastling3() {
        var noCastling = new Board("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w - - 4 4");
        Assert.Equal("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w - - 4 4", noCastling.ExportFEN());
    }

    [Fact]
    public void TestCastling4() {
        var noCastling = new Board("r3k2r/pppb1ppp/2nq1n2/2bpp3/3PPB2/2NQ1N2/PPP1BPPP/R3K2R w - - 20 14");
        Assert.Equal("r3k2r/pppb1ppp/2nq1n2/2bpp3/3PPB2/2NQ1N2/PPP1BPPP/R3K2R w - - 20 14", noCastling.ExportFEN());
    }

    [Fact]
    public void TestEnpassant1() {
        var whiteTakesF6 = new Board("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3");
        Assert.Equal("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3", whiteTakesF6.ExportFEN());
    }

    [Fact]
    public void TestEnpassant2() {
        var blackTakesC3 = new Board("rnbqkbnr/ppp1p1pp/5P2/8/2Pp4/8/PP1P1PPP/RNBQKBNR b KQkq c3 0 4");
        Assert.Equal("rnbqkbnr/ppp1p1pp/5P2/8/2Pp4/8/PP1P1PPP/RNBQKBNR b KQkq c3 0 4", blackTakesC3.ExportFEN());
    }

    public void LoadPgn1() {

    }

    public void LoadPgnWithTags() {

    }

    public void LoadPgnCastlingPromotion() {

    }
}
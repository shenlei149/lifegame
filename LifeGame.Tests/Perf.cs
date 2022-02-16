using Xunit;

namespace LifeGame.Tests
{
    // dotnet test --logger "console;verbosity=detailed"
    public class Perf
    {
        [Fact(Skip = "")]
        public void TestBoolArrayLife()
        {
            var board = new LifeBoard();
            var life = new BoolArrayLife();
            TestWithDraw(life, board);
        }

        [Fact]
        public void TestBoolArrayLifeStep_50K()
        {
            var board = new LifeBoard();
            var life = new BoolArrayLife();
            TestStep(life, board, 50_000);
        }

        [Fact(Skip = "")]
        public void TestScholesAlg()
        {
            var board = new LifeBoard();
            var life = new JohnScholes();
            TestWithDraw(life, board);
        }

        [Fact]
        public void TestScholesAlgStep_200K()
        {
            var board = new LifeBoard();
            var life = new JohnScholes();
            TestStep(life, board, 200_000);
        }

        [Fact(Skip = "")]
        public void TestAbrashAlg()
        {
            var board = new LifeBoard();
            var life = new MichaelAbrash();
            TestWithDraw(life, board);
        }

        [Fact]
        public void TestAbrashAlgStep_30M()
        {
            var board = new LifeBoard();
            var life = new MichaelAbrash();
            TestStep(life, board, 30_000_000);
        }

        [Fact(Skip = "")]
        public void TestStaffordAlg()
        {
            var board = new LifeBoard();
            var life = new Stafford();
            TestWithDraw(life, board);
        }

        [Fact]
        public void TestStaffordAlgStep_10M()
        {
            var board = new LifeBoard();
            var life = new Stafford();
            TestStep(life, board, 10_000_000);
        }

        private static void TestWithDraw(ILife life, LifeBoard board)
        {
            life.AddBlinker(new LifePoint(3, 3));
            life.AddAcorn(new LifePoint(15, 15));
            life.AddR(new LifePoint(30, 30));
            for (int i = 0; i < 100_000; i++)
            {
                life.Draw(board);
                life.Step();
            }
        }

        private static void TestStep(ILife life, LifeBoard board, int count)
        {
            life.AddBlinker(new LifePoint(3, 3));
            life.AddAcorn(new LifePoint(15, 15));
            life.AddR(new LifePoint(30, 30));
            for (int i = 0; i < count; i++)
            {
                life.Step();
            }
        }
    }
}

using System.Threading;

namespace LifeGame // Note: actual namespace depends on the project name.
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var board = new LifeBoard();
            var life = new MichaelAbrash();
            life.AddBlinker(new LifePoint(2, 2));
            life.AddAcorn(new LifePoint(15, 15));
            // life.AddR(new LifePoint(5, 5));
            while (true)
            {
                life.Draw(board);
                ConsoleCanvas.Draw(board);
                life.Step();

                Thread.Sleep(1000);
            }
        }
    }
}

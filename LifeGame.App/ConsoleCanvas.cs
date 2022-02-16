using System;

namespace LifeGame
{
    public static class ConsoleCanvas
    {
        private static string BLOCK = "\U00002588\U00002588";
        private static string SPACE = "  ";

        public static void Draw(LifeBoard board)
        {
            Console.Clear();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board[x, y])
                    {
                        Console.Write(BLOCK);
                    }
                    else
                    {
                        Console.Write(SPACE);
                    }
                }

                Console.WriteLine();
            }
        }
    }
}

namespace LifeGame
{
    public class BoolArrayLife : ILife
    {
        private LifeBoard board;

        public BoolArrayLife()
        {
            Clear();
        }

        public void Clear()
        {
            board = new LifeBoard();
        }

        public bool this[int x, int y] { get => board[x, y]; set => board[x, y] = value; }
        public bool this[LifePoint p] { get => board[p]; set => board[p] = value; }

        private int LivingNeighbors(int x, int y)
        {
            // avoid calling IsValidPoint too many times
            if (board.IsInnerPoint(x, y))
            {
                return (board[x - 1, y - 1, true] ? 1 : 0)
                     + (board[x - 1, y, true] ? 1 : 0)
                     + (board[x - 1, y + 1, true] ? 1 : 0)
                     + (board[x, y - 1, true] ? 1 : 0)
                     + (board[x, y + 1, true] ? 1 : 0)
                     + (board[x + 1, y - 1, true] ? 1 : 0)
                     + (board[x + 1, y, true] ? 1 : 0)
                     + (board[x + 1, y + 1, true] ? 1 : 0);
            }

            return (board[x - 1, y - 1] ? 1 : 0)
                 + (board[x - 1, y] ? 1 : 0)
                 + (board[x - 1, y + 1] ? 1 : 0)
                 + (board[x, y - 1] ? 1 : 0)
                 + (board[x, y + 1] ? 1 : 0)
                 + (board[x + 1, y - 1] ? 1 : 0)
                 + (board[x + 1, y] ? 1 : 0)
                 + (board[x + 1, y + 1] ? 1 : 0);
        }

        public void Step()
        {
            var newBoard = new LifeBoard();
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int count = LivingNeighbors(x, y);
                    newBoard[x, y] = count == 3 || (board[x, y] && count == 2);
                }
            }

            board.Update(newBoard);
        }

        public void Draw(LifeBoard board)
        {
            board.Update(this.board);
        }
    }
}

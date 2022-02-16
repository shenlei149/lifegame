using System;

namespace LifeGame
{
    public static class Size
    {
        public static int WIDTH { get; set; } = 36;
        public static int HEIGHT { get; set; } = 36;
    }

    public struct LifePoint
    {
        public int X { get; }
        public int Y { get; }
        public LifePoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class LifeBoard
    {
        public int Height { get; } = Size.HEIGHT;
        public int Width { get; } = Size.WIDTH;
        private bool[,] cells;

        private bool IsValidPoint(int x, int y)
        {
            return 0 <= x && x < Width && 0 <= y && y < Height;
        }

        public bool IsInnerPoint(int x, int y)
        {
            return 1 <= x && x < Width - 1 && 1 <= y && y < Height - 1;
        }

        public bool this[int x, int y, bool skipValidation = false]
        {
            get
            {
                if (skipValidation || IsValidPoint(x, y))
                { return cells[x, y]; }
                return false;
            }
            set
            {
                if (skipValidation || IsValidPoint(x, y))
                { cells[x, y] = value; }
            }
        }

        public bool this[LifePoint p]
        {
            get => this[p.X, p.Y];
            set => this[p.X, p.Y] = value;
        }

        public LifeBoard()
        {
            Clear();
        }

        public void Clear()
        {
            cells = new bool[Width, Height];
        }

        public void Update(LifeBoard board)
        {
            cells = board.cells;
        }
    }

    public interface ILife
    {
        void Clear();
        bool this[int x, int y] { get; set; }
        bool this[LifePoint point] { get; set; }
        void Draw(LifeBoard board);
        void Step();
    }
}

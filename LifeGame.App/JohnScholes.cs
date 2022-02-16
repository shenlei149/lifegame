using System;

namespace LifeGame
{
    class ByteBlock
    {
        private int Width { get; }
        private int Height { get; }

        // one dimension - easy to Move
        private readonly byte[] bytes;

        public ByteBlock(int width, int height, byte[] bytes = null)
        {
            this.Width = width;
            this.Height = height;
            this.bytes = bytes == null ? new byte[width * height] : bytes;
        }

        public byte this[int x, int y]
        {
            get => bytes[y * Width + x];
            set => bytes[y * Width + x] = value;
        }

        // 1 2 3        2 3 0
        // 4 5 6   -->  5 6 0
        // 7 8 9        8 9 0
        public ByteBlock MoveLeft()
        {
            byte[] newBytes = new byte[bytes.Length];
            Array.Copy(bytes, 1, newBytes, 0, bytes.Length - 1);
            for (int i = Width - 1; i < newBytes.Length; i += Width)
                newBytes[i] = 0;
            return new ByteBlock(Width, Height, newBytes);
        }

        // 1 2 3        0 1 2
        // 4 5 6   -->  0 4 5
        // 7 8 9        0 7 8
        public ByteBlock MoveRight()
        {
            byte[] newBytes = new byte[bytes.Length];
            Array.Copy(bytes, 0, newBytes, 1, bytes.Length - 1);
            for (int i = Width; i < newBytes.Length; i += Width)
                newBytes[i] = 0;
            return new ByteBlock(Width, Height, newBytes);
        }

        // 1 2 3        4 5 6
        // 4 5 6   -->  7 8 9
        // 7 8 9        0 0 0
        public ByteBlock MoveUp()
        {
            byte[] newBytes = new byte[bytes.Length];
            Array.Copy(bytes, Width, newBytes, 0, bytes.Length - Width);
            return new ByteBlock(Width, Height, newBytes);
        }

        // 1 2 3        0 0 0
        // 4 5 6   -->  1 2 3
        // 7 8 9        4 5 6
        public ByteBlock MoveDown()
        {
            byte[] newBytes = new byte[bytes.Length];
            Array.Copy(bytes, 0, newBytes, Width, bytes.Length - Width);
            return new ByteBlock(Width, Height, newBytes);
        }


        // 1 2 3               0 0 0
        // 4 5 6  where 4 -->  1 0 0
        // 7 8 9               0 0 0
        public ByteBlock Where(byte b)
        {
            byte[] newBytes = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i += 1)
                newBytes[i] = bytes[i] == b ? (byte)1 : (byte)0;
            return new ByteBlock(Width, Height, newBytes);
        }

        // 1 2 3   1 0 1      2 2 4
        // 4 5 6 + 1 1 1 -->  5 6 7
        // 7 8 9   0 1 0      7 9 9
        public ByteBlock Sum(params ByteBlock[] bs)
        {
            if (bs == null)
                throw new ArgumentNullException();

            // Omitted: Verify that every block in bs is the  dimensions as this.

            byte[] newBytes = (byte[])bytes.Clone();
            foreach (var b in bs)
                for (int i = 0; i < newBytes.Length; i += 1)
                    newBytes[i] += b.bytes[i];

            return new ByteBlock(Width, Height, newBytes);
        }

        public static ByteBlock operator |(ByteBlock a, ByteBlock b)
        {
            if (a == null || b == null || a.Width != b.Width || a.Height != b.Height)
                throw new ArgumentException();

            byte[] newBytes = new byte[a.bytes.Length];
            for (int i = 0; i < newBytes.Length; i += 1)
                newBytes[i] = (byte)(a.bytes[i] | b.bytes[i]);

            return new ByteBlock(a.Width, a.Height, newBytes);
        }

        public static ByteBlock operator &(ByteBlock a, ByteBlock b)
        {
            if (a == null || b == null || a.Width != b.Width || a.Height != b.Height)
                throw new ArgumentException();

            byte[] newBytes = new byte[a.bytes.Length];
            for (int i = 0; i < newBytes.Length; i += 1)
                newBytes[i] = (byte)(a.bytes[i] & b.bytes[i]);

            return new ByteBlock(a.Width, a.Height, newBytes);
        }
    }

    public class JohnScholes : ILife
    {
        private static readonly int height = Size.HEIGHT;
        private static readonly int width = Size.WIDTH;
        private ByteBlock cells;

        public JohnScholes()
        {
            Clear();
        }

        public void Clear()
        {
            cells = new ByteBlock(width, height);
        }

        public bool this[LifePoint p]
        {
            get => this[p.X, p.Y];
            set => this[p.X, p.Y] = value;
        }

        public bool this[int x, int y]
        {
            get
            {
                return cells[x, y] != 0;
            }
            set
            {
                cells[x, y] = value ? (byte)1 : (byte)0;
            }
        }

        public void Step()
        {
            var w = cells.MoveLeft();
            var e = cells.MoveRight();
            var n = cells.MoveUp();
            var s = cells.MoveDown();
            var nw = w.MoveUp();
            var ne = e.MoveUp();
            var sw = w.MoveDown();
            var se = e.MoveDown();
            var sum = cells.Sum(w, e, n, s, nw, ne, sw, se);
            cells = sum.Where(3) | (sum.Where(4) & cells);
        }

        public void Draw(LifeBoard board)
        {
            board.Clear();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (this[x, y])
                    {
                        board[x, y] = this[x, y];
                    }
                }
            }
        }
    }
}

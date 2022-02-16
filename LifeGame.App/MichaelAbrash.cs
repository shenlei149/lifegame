using System;
using System.Collections.Generic;

namespace LifeGame
{
    struct Cell
    {
        private readonly byte cell;
        public Cell(byte cell)
        {
            this.cell = cell;
        }

        // Bit 5 is the upcoming state of the cell.
        // Bit 4 is the current state of the cell.
        // bits 0 through 3 are the number of living neighbors.
        private const int nextMask = 1 << 5;
        private const int stateMask = 1 << 4;
        private const int countMask = 0xf;

        public bool Next => (cell & nextMask) != 0;
        public Cell NextAlive() => new Cell((byte)(cell | nextMask));
        public Cell NextDead() => new Cell((byte)(cell & ~nextMask));

        public bool State => (cell & stateMask) != 0;
        public int Count => cell & countMask;

        // Dead cell with all dead neighbors.
        public bool AllDead => cell == 0;

        public Cell MakeAlive() => new Cell((byte)(cell | stateMask));
        public Cell MakeDead() => new Cell((byte)(cell & ~stateMask));

        // We don't have to mask out the state bit to do an increment or decrement!
        public Cell Increment()
        {
            return new Cell((byte)(cell + 1));
        }

        public Cell Decrement()
        {
            return new Cell((byte)(cell - 1));
        }
    }

    public class MichaelAbrash : ILife
    {
        private static readonly int height = Size.HEIGHT + 2;
        private static readonly int width = Size.WIDTH + 2;
        private List<(int, int)> changes;
        private Cell[,] cells;

        public MichaelAbrash()
        {
            Clear();
        }

        public void Clear()
        {
            cells = new Cell[width, height];
            changes = new List<(int, int)>();
        }

        // We ensure that the four borders of the array of cells are always dead.
        private bool IsValidPoint(int x, int y) =>
            0 < x && x < width - 1 && 0 < y && y < height - 1;

        // Make a dead cell come alive.
        private void BecomeAlive(int x, int y)
        {
            if (cells[x, y].State)
            {
                return;
            }

            cells[x - 1, y - 1] = cells[x - 1, y - 1].Increment();
            cells[x - 1, y] = cells[x - 1, y].Increment();
            cells[x - 1, y + 1] = cells[x - 1, y + 1].Increment();
            cells[x, y - 1] = cells[x, y - 1].Increment();
            cells[x, y] = cells[x, y].MakeAlive();
            cells[x, y + 1] = cells[x, y + 1].Increment();
            cells[x + 1, y - 1] = cells[x + 1, y - 1].Increment();
            cells[x + 1, y] = cells[x + 1, y].Increment();
            cells[x + 1, y + 1] = cells[x + 1, y + 1].Increment();

            changes.Add((x, y));
        }

        // Make a live cell die.
        private void BecomeDead(int x, int y)
        {
            if (!cells[x, y].State)
            {
                return;
            }

            cells[x - 1, y - 1] = cells[x - 1, y - 1].Decrement();
            cells[x - 1, y] = cells[x - 1, y].Decrement();
            cells[x - 1, y + 1] = cells[x - 1, y + 1].Decrement();
            cells[x, y - 1] = cells[x, y - 1].Decrement();
            cells[x, y] = cells[x, y].MakeDead();
            cells[x, y + 1] = cells[x, y + 1].Decrement();
            cells[x + 1, y - 1] = cells[x + 1, y - 1].Decrement();
            cells[x + 1, y] = cells[x + 1, y].Decrement();
            cells[x + 1, y + 1] = cells[x + 1, y + 1].Decrement();

            changes.Add((x, y));
        }

        public bool this[int x, int y]
        {
            get
            {
                x++;
                y++;
                if (IsValidPoint(x, y))
                    return cells[x, y].State;
                return false;
            }
            set
            {
                x++;
                y++;
                if (!IsValidPoint(x, y))
                    return;
                Cell c = cells[x, y];
                // No change? Bail out.
                if (c.State == value)
                    return;
                if (value)
                    BecomeAlive(x, y);
                else
                    BecomeDead(x, y);
            }
        }

        public bool this[LifePoint p]
        {
            get => this[p.X, p.Y];
            set => this[p.X, p.Y] = value;
        }

        public void Step()
        {
            Cell[,] clone = (Cell[,])cells.Clone();

            // var previousChanges = changes;
            // // Start a new change list.
            // changes = new List<(int, int)>();
            var currentChanges = new List<(int, int)>();

            foreach ((int cx, int cy) in changes)
            {
                int minx = Math.Max(cx - 1, 1);
                int maxx = Math.Min(cx + 2, width - 1);
                int miny = Math.Max(cy - 1, 1);
                int maxy = Math.Min(cy + 2, height - 1);

                for (int y = miny; y < maxy; y++)
                {
                    for (int x = minx; x < maxx; x++)
                    {
                        Cell cell = clone[x, y];
                        // if (cell.AllDead)
                        //     continue;

                        int count = cell.Count;
                        bool state = cell.State;
                        bool newState = count == 3 | count == 2 & state;
                        // if (cell.State)
                        // {
                        //     if (count != 2 && count != 3)
                        //         BecomeDead(x, y);
                        // }
                        // else if (count == 3)
                        //     BecomeAlive(x, y);
                        if (state & !newState)
                        {
                            currentChanges.Add((x, y));
                            cells[x, y] = cell.NextDead();
                        }
                        else if (!state & newState)
                        {
                            currentChanges.Add((x, y));
                            cells[x, y] = cell.NextAlive();
                        }
                    }
                }
            }

            changes.Clear();

            foreach ((int x, int y) in currentChanges)
            {
                if (cells[x, y].Next)
                {
                    BecomeAlive(x, y);
                }
                else
                {
                    BecomeDead(x, y);
                }
            }
        }

        public void Draw(LifeBoard board)
        {
            board.Clear();

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (cells[x, y].State)
                    {
                        board[x - 1, y - 1] = cells[x, y].State;
                    }
                }
            }
        }
    }
}

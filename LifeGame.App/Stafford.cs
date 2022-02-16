using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace LifeGame
{
    static class TripletLookup
    {
        public static Triplet[] lookup;
        public static bool[] changed;

        static TripletLookup()
        {
            // Some of these are impossible, but who cares?
            lookup = new Triplet[1 << 12];
            changed = new bool[1 << 12];

            for (int left = 0; left < 2; left += 1)
                for (int middle = 0; middle < 2; middle += 1)
                    for (int right = 0; right < 2; right += 1)
                        for (int lc = 0; lc < 8; lc += 1)
                            for (int mc = 0; mc < 7; mc += 1)
                                for (int rc = 0; rc < 8; rc += 1)
                                {
                                    Triplet t = new Triplet()
                                        .SetLeftCurrent(left == 1)
                                        .SetMiddleCurrent(middle == 1)
                                        .SetRightCurrent(right == 1)
                                        .SetLeftCountRaw(lc)
                                        .SetMiddleCountRaw(mc)
                                        .SetRightCountRaw(rc)
                                        .SetLeftNext((lc + middle == 3) | (left == 1) & (lc + middle == 2))
                                        .SetMiddleNext((left + mc + right == 3) | (middle == 1) & (left + mc + right == 2))
                                        .SetRightNext((middle + rc == 3) | (right == 1) & (middle + rc == 2));
                                    lookup[t.LookupKey1] = t;
                                    changed[t.LookupKey1] = t.Changed;
                                }

        }
    }

    struct Triplet
    {
        // We represent three adjacent cells using 15 bits of a short.
        // Call the cells Left, Middle and Right.
        private short triplet;

        public Triplet(short triplet)
        {
            this.triplet = triplet;
        }

        public Triplet(int triplet)
        {
            this.triplet = (short)triplet;
        }

        // The original implementation used bit 15 to store whether the cell
        // triplet was on the edge of the finite grid so that it could decide
        // quickly whether to do wrap-around semantics or not. We're not 
        // implementing wrap-around semantics in this series so bit 15 is unused.

        // Bits 12, 13 and 14 are the state of the cells on the next tick.
        private const int lnext = 14;
        private const int mnext = 13;
        private const int rnext = 12;

        // Bits 9, 10 and 11 are the state of the cells on the current tick.
        private const int lcur = 11;
        private const int mcur = 10;
        private const int rcur = 9;

        // Bits 6, 7 and 8 are the count of living neighbors of the left
        // cell; three bits is not enough to count up to eight living
        // neighbours, but we already know the state of the neighbor
        // to the right; it is the middle cell.
        private const int lcount = 6;

        // Similarly for bits 3, 4 and 5 for the middle cell.
        private const int mcount = 3;

        // And similarly for bits 0, 1 and 2 for the right cell.
        private const int rcount = 0;

        private const int lnextMask = 1 << lnext;
        private const int mnextMask = 1 << mnext;
        private const int rnextMask = 1 << rnext;
        private const int lcurMask = 1 << lcur;
        private const int mcurMask = 1 << mcur;
        private const int rcurMask = 1 << rcur;
        private const int nextMask = lnextMask | mnextMask | rnextMask;
        private const int currentMask = lcurMask | mcurMask | rcurMask;
        private const int lcountMask = 7 << lcount;
        private const int mcountMask = 7 << mcount;
        private const int rcountMask = 7 << rcount;

        // Getters and setters for state

        // Note that I want to treat state as both a bool and an int,
        // so I've got getters for both cases.

        public bool LeftNext => (lnextMask & triplet) != 0;
        public bool MiddleNext => (mnextMask & triplet) != 0;
        public bool RightNext => (rnextMask & triplet) != 0;

        public int LeftNextRaw => (triplet & lnextMask) >> lnext;
        public int MiddleNextRaw => (triplet & mnextMask) >> mnext;
        public int RightNextRaw => (triplet & rnextMask) >> rnext;

        public Triplet SetLeftNext(bool b) => new Triplet(b ? (lnextMask | triplet) : (~lnextMask & triplet));
        public Triplet SetMiddleNext(bool b) => new Triplet(b ? (mnextMask | triplet) : (~mnextMask & triplet));
        public Triplet SetRightNext(bool b) => new Triplet(b ? (rnextMask | triplet) : (~rnextMask & triplet));

        public bool LeftCurrent => (lcurMask & triplet) != 0;
        public bool MiddleCurrent => (mcurMask & triplet) != 0;
        public bool RightCurrent => (rcurMask & triplet) != 0;

        public int LeftCurrentRaw => (triplet & lcurMask) >> lcur;
        public int MiddleCurrentRaw => (triplet & mcurMask) >> mcur;
        public int RightCurrentRaw => (triplet & rcurMask) >> rcur;

        public Triplet SetLeftCurrent(bool b) => new Triplet(b ? (lcurMask | triplet) : (~lcurMask & triplet));
        public Triplet SetMiddleCurrent(bool b) => new Triplet(b ? (mcurMask | triplet) : (~mcurMask & triplet));
        public Triplet SetRightCurrent(bool b) => new Triplet(b ? (rcurMask | triplet) : (~rcurMask & triplet));

        // It's convenient to fetch the current and next states as integers.
        public int NextState => (nextMask & triplet) >> rnext;
        public int CurrentState => (currentMask & triplet) >> rcur;
        public bool Changed => CurrentState != NextState;

        // Getters and setters for the neighbour counts

        // I've got getters for both the "raw" 3-bit integer stored in the 
        // triplet and the actual neighbour count it represents.

        public int LeftCountRaw => (lcountMask & triplet) >> lcount;
        public int MiddleCountRaw => (mcountMask & triplet) >> mcount;
        public int RightCountRaw => (rcountMask & triplet) >> rcount;

        public int LeftCount => MiddleCurrentRaw + LeftCountRaw;
        public int MiddleCount => LeftCurrentRaw + RightCurrentRaw + MiddleCountRaw;
        public int RightCount => MiddleCurrentRaw + RightCountRaw;

        public Triplet SetLeftCountRaw(int c)
        {
            Debug.Assert(0 <= c && c <= 7);
            return new Triplet((c << lcount) | ~lcountMask & triplet);
        }

        public Triplet SetMiddleCountRaw(int c)
        {
            Debug.Assert(0 <= c && c <= 6);
            return new Triplet((c << mcount) | ~mcountMask & triplet);
        }

        public Triplet SetRightCountRaw(int c)
        {
            Debug.Assert(0 <= c && c <= 7);
            return new Triplet((c << rcount) | ~rcountMask & triplet);
        }


        // It is slow and tedious to change counts via the "set raw count"
        // mechanism; instead, let's make fifteen helper methods using the
        // naming convention:
        //
        // U = unchanged
        // P = increment
        // M = decrement

        private const int lcountone = 1 << lcount;
        private const int mcountone = 1 << mcount;
        private const int rcountone = 1 << rcount;

        public Triplet UUP() => new Triplet(rcountone + triplet);
        public Triplet UUM() => new Triplet(-rcountone + triplet);
        public Triplet UPU() => new Triplet(mcountone + triplet);
        public Triplet UPP() => new Triplet(mcountone + rcountone + triplet);
        public Triplet UMU() => new Triplet(-mcountone + triplet);
        public Triplet UMM() => new Triplet(-mcountone - rcountone + triplet);
        public Triplet PUU() => new Triplet(lcountone + triplet);
        public Triplet PUM() => new Triplet(lcountone - rcountone + triplet);
        public Triplet PPU() => new Triplet(lcountone + mcountone + triplet);
        public Triplet PPP() => new Triplet(lcountone + mcountone + rcountone + triplet);
        public Triplet MUU() => new Triplet(-lcountone + triplet);
        public Triplet MUP() => new Triplet(-lcountone + rcountone + triplet);
        public Triplet MMU() => new Triplet(-lcountone - mcountone + triplet);
        public Triplet MMM() => new Triplet(-lcountone - mcountone - rcountone + triplet);

        // Key for first pass lookup is the bottom 12 bits:
        // the raw counts and current state.
        public int LookupKey1 => triplet & 0x0fff;
    }

    public class Stafford : ILife
    {
        private static readonly int height = Size.HEIGHT + 2;
        private static readonly int width = ((Size.WIDTH + 2) - 1) / 3 + 1;

        // Coordinates of triplets, not cells.
        private List<(int, int)> changes;
        private Triplet[,] triplets;

        public Stafford()
        {
            Clear();
        }

        public void Clear()
        {
            triplets = new Triplet[width, height];
            changes = new List<(int, int)>();
        }

        private bool IsValidPoint(int x, int y) =>
            0 < x && x < (width - 1) * 3 && 0 < y && y < (height - 1) * 3;

        private bool BecomeAlive(int x, int y)
        {
            int tx = x / 3;
            Triplet t = triplets[tx, y];
            switch (x % 3)
            {
                case 0:
                    if (t.LeftCurrent)
                        return false;

                    // Left is about to be born
                    triplets[tx - 1, y - 1] = triplets[tx - 1, y - 1].UUP();
                    triplets[tx, y - 1] = triplets[tx, y - 1].PPU();
                    triplets[tx - 1, y] = triplets[tx - 1, y].UUP();
                    triplets[tx, y] = t.SetLeftCurrent(true);
                    triplets[tx - 1, y + 1] = triplets[tx - 1, y + 1].UUP();
                    triplets[tx, y + 1] = triplets[tx, y + 1].PPU();
                    break;
                case 1:
                    if (t.MiddleCurrent)
                        return false;

                    // Middle is about to be born
                    triplets[tx, y - 1] = triplets[tx, y - 1].PPP();
                    triplets[tx, y] = t.SetMiddleCurrent(true);
                    triplets[tx, y + 1] = triplets[tx, y + 1].PPP();
                    break;
                case 2:
                    if (t.RightCurrent)
                        return false;

                    // Right is about to be born
                    triplets[tx, y - 1] = triplets[tx, y - 1].UPP();
                    triplets[tx + 1, y - 1] = triplets[tx + 1, y - 1].PUU();
                    triplets[tx, y] = t.SetRightCurrent(true);
                    triplets[tx + 1, y] = triplets[tx + 1, y].PUU();
                    triplets[tx, y + 1] = triplets[tx, y + 1].UPP();
                    triplets[tx + 1, y + 1] = triplets[tx + 1, y + 1].PUU();
                    break;
            }

            return true;
        }

        private bool BecomeDead(int x, int y)
        {
            int tx = x / 3;
            Triplet t = triplets[tx, y];

            switch (x % 3)
            {
                case 0:
                    if (!t.LeftCurrent)
                        return false;

                    triplets[tx - 1, y - 1] = triplets[tx - 1, y - 1].UUM();
                    triplets[tx, y - 1] = triplets[tx, y - 1].MMU();
                    triplets[tx - 1, y] = triplets[tx - 1, y].UUM();
                    triplets[tx, y] = t.SetLeftCurrent(false);
                    triplets[tx - 1, y + 1] = triplets[tx - 1, y + 1].UUM();
                    triplets[tx, y + 1] = triplets[tx, y + 1].MMU();
                    break;
                case 1:
                    if (!t.MiddleCurrent)
                        return false;

                    triplets[tx, y - 1] = triplets[tx, y - 1].MMM();
                    triplets[tx, y] = t.SetMiddleCurrent(false);
                    triplets[tx, y + 1] = triplets[tx, y + 1].MMM();
                    break;
                case 2:
                    if (!t.RightCurrent)
                        return false;

                    triplets[tx, y - 1] = triplets[tx, y - 1].UMM();
                    triplets[tx + 1, y - 1] = triplets[tx + 1, y - 1].MUU();
                    triplets[tx, y] = t.SetRightCurrent(false);
                    triplets[tx + 1, y] = triplets[tx + 1, y].MUU();
                    triplets[tx, y + 1] = triplets[tx, y + 1].UMM();
                    triplets[tx + 1, y + 1] = triplets[tx + 1, y + 1].MUU();
                    break;
            }

            return true;
        }

        public bool this[int x, int y]
        {
            get
            {
                x++;
                y++;
                if (IsValidPoint(x, y))
                {
                    Triplet t = triplets[x / 3, y];
                    switch (x % 3)
                    {
                        case 0: return t.LeftCurrent;
                        case 1: return t.MiddleCurrent;
                        case 2: return t.RightCurrent;
                    }
                }
                return false;
            }
            set
            {
                x++;
                y++;
                if (IsValidPoint(x, y))
                {
                    if (value)
                    {
                        if (BecomeAlive((int)x, (int)y))
                            changes.Add(((int)x / 3, (int)y));
                    }
                    else
                    {
                        if (BecomeDead((int)x, (int)y))
                            changes.Add(((int)x / 3, (int)y));
                    }
                }
            }
        }

        public bool this[LifePoint v]
        {
            get => this[v.X, v.Y];
            set => this[v.X, v.Y] = value;
        }

        public void Step()
        {
            // First pass: for the previous changes and all their neighbours, record their
            // new states. If the new state changed, make a note of that.

            // This list might have duplicates.
            var currentChanges = new List<(int, int)>();

            foreach ((int cx, int cy) in changes)
            {
                int minx = Math.Max(cx - 1, 1);
                int maxx = Math.Min(cx + 2, width - 1);
                int miny = Math.Max(cy - 1, 1);
                int maxy = Math.Min(cy + 2, height - 1);
                for (int y = miny; y < maxy; y += 1)
                {
                    for (int x = minx; x < maxx; x += 1)
                    {
                        // Triplet c = triplets[x, y];
                        // Triplet t = c;
                        // int lc = t.LeftCount;
                        // int mc = t.MiddleCount;
                        // int rc = t.RightCount;
                        // t = t.SetLeftNext(lc == 3 | t.LeftCurrent & lc == 2);
                        // t = t.SetMiddleNext(mc == 3 | t.MiddleCurrent & mc == 2);
                        // t = t.SetRightNext(rc == 3 | t.RightCurrent & rc == 2);
                        // if (t.Changed)

                        int key1 = triplets[x, y].LookupKey1;
                        if (TripletLookup.changed[key1])
                        {
                            currentChanges.Add((x, y));
                            triplets[x, y] = TripletLookup.lookup[key1];
                        }
                    }
                }
            }

            changes.Clear();

            // Second pass: all the triplets that were marked as having changed need
            // to have their neighbour counts updated.

            foreach ((int x, int y) in currentChanges)
            {
                Triplet t = triplets[x, y];

                // If we've already done this one before, no need to do it again.
                if (!t.Changed)
                    continue;

                bool changed = false;
                if (t.LeftNext)
                    changed |= BecomeAlive(x * 3, y);
                else
                    changed |= BecomeDead(x * 3, y);

                if (t.MiddleNext)
                    changed |= BecomeAlive(x * 3 + 1, y);
                else
                    changed |= BecomeDead(x * 3 + 1, y);

                if (t.RightNext)
                    changed |= BecomeAlive(x * 3 + 2, y);
                else
                    changed |= BecomeDead(x * 3 + 2, y);
                Debug.Assert(changed);
                changes.Add((x, y));
            }
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

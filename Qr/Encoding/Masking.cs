using System;

namespace QrSharp.Encoding
{
    public static class Masking
    {
        private static readonly Func<int, int, bool>[] MASKS = new Func<int, int, bool>[]
        {
            (r,c)=> ((r+c)%2)==0,
            (r,c)=> (r%2)==0,
            (r,c)=> (c%3)==0,
            (r,c)=> ((r+c)%3)==0,
            (r,c)=> ((r/2 + c/3)%2)==0,
            (r,c)=> (((r*c)%2 + (r*c)%3)==0),
            (r,c)=> ((((r*c)%2 + (r*c)%3)%2)==0),
            (r,c)=> ((((r+c)%2 + (r*c)%3)%2)==0)
        };

        public static int[,] Apply(int[,] placed, bool[,] func, int maskId)
        {
            int n = placed.GetLength(0);
            var outm = (int[,])placed.Clone();
            var f = MASKS[maskId];
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (!func[r, c]) if (f(r, c)) outm[r, c] ^= 1;
            return outm;
        }

        public static int ScorePenalty(int[,] m) => N1(m) + N2(m) + N3(m) + N4(m);

        // N1: sequências >= 5 iguais
        private static int N1(int[,] m)
        {
            int n = m.GetLength(0), s = 0;
            for (int r = 0; r < n; r++)
            {
                int run = 1;
                for (int c = 1; c < n; c++)
                {
                    if (m[r, c] == m[r, c - 1]) run++;
                    else { if (run >= 5) s += 3 + (run - 5); run = 1; }
                }
                if (run >= 5) s += 3 + (run - 5);
            }
            for (int c = 0; c < n; c++)
            {
                int run = 1;
                for (int r = 1; r < n; r++)
                {
                    if (m[r, c] == m[r - 1, c]) run++;
                    else { if (run >= 5) s += 3 + (run - 5); run = 1; }
                }
                if (run >= 5) s += 3 + (run - 5);
            }
            return s;
        }

        // N2: blocos 2×2 iguais
        private static int N2(int[,] m)
        {
            int n = m.GetLength(0), s = 0;
            for (int r = 0; r < n - 1; r++)
                for (int c = 0; c < n - 1; c++)
                {
                    int v = m[r, c];
                    if (m[r + 1, c] == v && m[r, c + 1] == v && m[r + 1, c + 1] == v) s += 3;
                }
            return s;
        }

        // N3: padrão 1:1:3:1:1 com margens claras
        private static int N3(int[,] m)
        {
            int n = m.GetLength(0), s = 0;
            int[] pat = { 1, 0, 1, 1, 1, 0, 1 };
            for (int r = 0; r < n; r++)
                for (int c = 0; c <= n - 7; c++)
                {
                    bool ok = true;
                    for (int k = 0; k < 7; k++) if (m[r, c + k] != pat[k]) { ok = false; break; }
                    if (ok)
                    {
                        bool leftClear = true, rightClear = true;
                        for (int k = Math.Max(0, c - 4); k < c; k++) if (m[r, k] == 1) leftClear = false;
                        for (int k = c + 7; k < Math.Min(n, c + 11); k++) if (m[r, k] == 1) rightClear = false;
                        if (leftClear && rightClear) s += 40;
                    }
                }
            for (int c = 0; c < n; c++)
                for (int r = 0; r <= n - 7; r++)
                {
                    bool ok = true;
                    for (int k = 0; k < 7; k++) if (m[r + k, c] != pat[k]) { ok = false; break; }
                    if (ok)
                    {
                        bool upClear = true, downClear = true;
                        for (int k = Math.Max(0, r - 4); k < r; k++) if (m[k, c] == 1) upClear = false;
                        for (int k = r + 7; k < Math.Min(n, r + 11); k++) if (m[k, c] == 1) downClear = false;
                        if (upClear && downClear) s += 40;
                    }
                }
            return s;
        }

        // N4: densidade de pretos
        private static int N4(int[,] m)
        {
            int n = m.GetLength(0), dark = 0;
            for (int r = 0; r < n; r++) for (int c = 0; c < n; c++) dark += m[r, c];
            int total = n * n;
            int k2 = Math.Abs((100 * dark / total) - 50) / 5;
            return k2 * 10;
        }
    }
}

using QrSharp.Core;

namespace QrSharp.Matrix
{
    public static class TemplateBuilder
    {
        public static (int[,] mat, bool[,] isFunc) Build(int version)
        {
            int n = Tables.VersionSize(version);
            var mat = new int[n, n];
            var func = new bool[n, n];

            Fill(mat, 2); // 2 = placeholder

            PutFinder(mat, func, 0, 0);
            PutFinder(mat, func, 0, n - 7);
            PutFinder(mat, func, n - 7, 0);

            PutTiming(mat, func);
            if (version >= 2) PutAlignments(mat, func, version);

            ReserveFormatAreas(mat, func);
            if (version >= 7) ReserveVersionAreas(mat, func);
            PutDarkModule(mat, func);
            return (mat, func);
        }

        private static void Fill(int[,] m, int v)
        {
            int n = m.GetLength(0);
            for (int r = 0; r < n; r++) for (int c = 0; c < n; c++) m[r, c] = v;
        }

        private static void PutFinder(int[,] mat, bool[,] func, int r0, int c0)
        {
            int n = mat.GetLength(0);
            for (int dr = -1; dr <= 7; dr++)
                for (int dc = -1; dc <= 7; dc++)
                {
                    int r = r0 + dr, c = c0 + dc;
                    if (r < 0 || r >= n || c < 0 || c >= n) continue;

                    if (dr == -1 || dr == 7 || dc == -1 || dc == 7)
                    { mat[r, c] = 0; func[r, c] = true; }
                    else
                    {
                        bool outer = (dr == 0 || dr == 6 || dc == 0 || dc == 6);
                        bool inner = (dr >= 2 && dr <= 4 && dc >= 2 && dc <= 4);
                        mat[r, c] = (outer || inner) ? 1 : 0;
                        func[r, c] = true;
                    }
                }
        }

        private static void PutTiming(int[,] mat, bool[,] func)
        {
            int n = mat.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                if (!func[6, i]) { mat[6, i] = (i % 2 == 0) ? 1 : 0; func[6, i] = true; }
                if (!func[i, 6]) { mat[i, 6] = (i % 2 == 0) ? 1 : 0; func[i, 6] = true; }
            }
        }

        private static void PutAlignments(int[,] mat, bool[,] func, int ver)
        {
            int n = mat.GetLength(0);
            var centers = Tables.ALIGNMENT[ver];
            foreach (int rc in centers)
                foreach (int cc in centers)
                {
                    if ((rc <= 8 && cc <= 8) || (rc <= 8 && cc >= n - 9) || (rc >= n - 9 && cc <= 8)) continue;

                    for (int dr = -2; dr <= 2; dr++)
                        for (int dc = -2; dc <= 2; dc++)
                        {
                            int r = rc + dr, c = cc + dc;
                            int v = (System.Math.Max(System.Math.Abs(dr), System.Math.Abs(dc)) == 2 || (dr == 0 && dc == 0)) ? 1 : 0;
                            mat[r, c] = v; func[r, c] = true;
                        }
                }
        }

        private static void ReserveFormatAreas(int[,] mat, bool[,] func)
        {
            int n = mat.GetLength(0);
            for (int i = 0; i < 9; i++)
            {
                if (i == 6) continue;
                mat[8, i] = 0; func[8, i] = true;
                mat[i, 8] = 0; func[i, 8] = true;
            }
            for (int i = 0; i < 8; i++)
            {
                mat[n - 1 - i, 8] = 0; func[n - 1 - i, 8] = true;
                mat[8, n - 1 - i] = 0; func[8, n - 1 - i] = true;
            }
        }

        private static void ReserveVersionAreas(int[,] mat, bool[,] func)
        {
            int n = mat.GetLength(0);
            for (int row = n - 11; row <= n - 9; row++)
                for (int col = 0; col <= 5; col++) { mat[row, col] = 0; func[row, col] = true; }
            for (int row = 0; row <= 5; row++)
                for (int col = n - 11; col <= n - 9; col++) { mat[row, col] = 0; func[row, col] = true; }
        }

        private static void PutDarkModule(int[,] mat, bool[,] func)
        {
            int n = mat.GetLength(0);
            mat[n - 8, 8] = 1; func[n - 8, 8] = true;
        }
    }
}

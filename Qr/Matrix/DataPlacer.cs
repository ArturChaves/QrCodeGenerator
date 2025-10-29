using System.Collections.Generic;

namespace QrSharp.Matrix
{
    public static class DataPlacer
    {
        public static int[,] Place(int[,] baseMat, bool[,] func, List<int> bits)
        {
            int n = baseMat.GetLength(0);
            var mat = (int[,])baseMat.Clone();
            int idx = 0; int c = n - 1; bool upward = true;

            while (c > 0)
            {
                if (c == 6) c--; // pular coluna timing

                if (upward)
                {
                    for (int r = n - 1; r >= 0; r--)
                        for (int dc = 0; dc >= -1; dc--)
                        {
                            int cc = c + dc; if (func[r, cc]) continue;
                            if (mat[r, cc] == 2) mat[r, cc] = (idx < bits.Count) ? bits[idx++] : 0;
                        }
                }
                else
                {
                    for (int r = 0; r < n; r++)
                        for (int dc = 0; dc >= -1; dc--)
                        {
                            int cc = c + dc; if (func[r, cc]) continue;
                            if (mat[r, cc] == 2) mat[r, cc] = (idx < bits.Count) ? bits[idx++] : 0;
                        }
                }

                upward = !upward; c -= 2;
            }

            return mat;
        }
    }
}

using System;

namespace QrSharp.Core
{
    /// <summary>GF(256) com polinômio primitivo 0x11D (QR Code).</summary>
    public sealed class GaloisField
    {
        private readonly int[] _exp = new int[512];
        private readonly int[] _log = new int[256];

        public GaloisField(int primitive = 0x11D)
        {
            int x = 1;
            for (int i = 0; i < 255; i++)
            {
                _exp[i] = x;
                _log[x] = i;
                x <<= 1;
                if ((x & 0x100) != 0) x ^= primitive;
            }
            for (int i = 255; i < 512; i++) _exp[i] = _exp[i - 255];
        }

        public int Mul(int a, int b)
        {
            if (a == 0 || b == 0) return 0;
            return _exp[_log[a] + _log[b]];
        }

        public int ExpAt(int i) => _exp[i]; // α^i

        internal int[] PolyMul(int[] p, int[] q)
        {
            var r = new int[p.Length + q.Length - 1];
            for (int i = 0; i < p.Length; i++)
            {
                int a = p[i]; if (a == 0) continue;
                int la = _log[a];
                for (int j = 0; j < q.Length; j++)
                {
                    int b = q[j]; if (b == 0) continue;
                    r[i + j] ^= _exp[la + _log[b]];
                }
            }
            return r;
        }

        internal void PolyDivmod(int[] dividend, int[] divisor, out int[] quotient, out int[] remainder)
        {
            var outv = (int[])dividend.Clone();
            for (int i = 0; i <= dividend.Length - divisor.Length; i++)
            {
                int coef = outv[i];
                if (coef != 0)
                {
                    int f = (_log[coef] - _log[divisor[0]] + 255) % 255;
                    for (int j = 0; j < divisor.Length; j++)
                    {
                        int dj = divisor[j];
                        if (dj != 0) outv[i + j] ^= _exp[(_log[dj] + f) % 255];
                    }
                }
            }
            int sep = dividend.Length - divisor.Length;
            quotient = new int[sep + 1];
            Array.Copy(outv, 0, quotient, 0, sep + 1);
            remainder = new int[dividend.Length - (sep + 1)];
            Array.Copy(outv, sep + 1, remainder, 0, remainder.Length);
        }
    }
}

namespace QrSharp.Matrix
{
    public static class FormatVersionWriter
    {
        public static void WriteFormat(int[,] m, int maskId, int eclBits)
        {
            int n = m.GetLength(0);
            int fmt = BCH_Format(eclBits, maskId);

            var bits = new int[15];
            for (int i = 0; i < 15; i++) bits[i] = (fmt >> (14 - i)) & 1; // MSB->LSB

            (int r, int c)[] pos1 = new (int, int)[]
            {
                (8,0),(8,1),(8,2),(8,3),(8,4),(8,5),(8,7),(8,8),
                (7,8),(5,8),(4,8),(3,8),(2,8),(1,8),(0,8)
            };
            for (int i = 0; i < 15; i++) m[pos1[i].r, pos1[i].c] = bits[i];

            (int r, int c)[] pos2 = new (int, int)[]
            {
                (n-1,8),(n-2,8),(n-3,8),(n-4,8),(n-5,8),(n-6,8),(n-7,8),
                (8,n-8),(8,n-7),(8,n-6),(8,n-5),(8,n-4),(8,n-3),(8,n-2),(8,n-1)
            };
            for (int i = 0; i < 15; i++) m[pos2[i].r, pos2[i].c] = bits[i];
        }

        public static void WriteVersion(int[,] m, int ver)
        {
            if (ver < 7) return;
            int n = m.GetLength(0);
            int vv = BCH_Version(ver);

            // bits em ordem LSB->MSB
            int[] lsb = new int[18];
            for (int i = 0; i < 18; i++) lsb[i] = (vv >> i) & 1;

            // bottom-left (3×6) col-first
            for (int col = 0; col < 6; col++)
                for (int row = 0; row < 3; row++)
                    m[(n - 11) + row, col] = lsb[col * 3 + row];

            // top-right (6×3) row-first
            for (int row = 0; row < 6; row++)
                for (int col = 0; col < 3; col++)
                    m[row, (n - 11) + col] = lsb[row * 3 + col];
        }

        private static int BCH_Format(int eclBits, int maskId)
        {
            int data = (eclBits << 3) | maskId; // 5 bits
            int g = 0x537;                      // x^10 + x^8 + x^5 + x^4 + x^2 + x + 1
            int v = data << 10;
            for (int i = 14; i >= 10; i--)
                if (((v >> i) & 1) != 0) v ^= g << (i - 10);
            int fmt = ((data << 10) | (v & 0x3FF)) ^ 0x5412;
            return fmt & 0x7FFF;
        }

        private static int BCH_Version(int ver)
        {
            int v = ver & 0x3F;   // 6 bits
            int g = 0x1F25;       // x^12 + x^11 + x^10 + x^9 + x^8 + x^5 + x^2 + 1
            int val = v << 12;
            for (int i = 17; i >= 12; i--)
                if (((val >> i) & 1) != 0) val ^= g << (i - 12);
            return ((v << 12) | (val & 0xFFF)) & 0x3FFFF; // 18 bits
        }
    }
}

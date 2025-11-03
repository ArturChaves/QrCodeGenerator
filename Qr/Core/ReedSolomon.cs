using System;

namespace QrCodeGenerator.Qr.Core
{
    public static class ReedSolomon
    {
        public static int[] GeneratorPoly(GaloisField gf, int ecLen)
        {
            int[] g = new[] { 1 };
            for (int i = 0; i < ecLen; i++)
                g = gf.PolyMul(g, new[] { 1, gf.ExpAt(i) }); 
            return g;
        }

        public static int[] EncodeBlock(GaloisField gf, int[] dataCW, int ecLen)
        {
            var g = GeneratorPoly(gf, ecLen);
            var dividend = new int[dataCW.Length + ecLen];
            Array.Copy(dataCW, dividend, dataCW.Length);
            gf.PolyDivmod(dividend, g, out _, out var remainder);
            return remainder;
        }
    }
}

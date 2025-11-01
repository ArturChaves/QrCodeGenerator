using System.Collections.Generic;
using System.Linq;
using QrCodeGenerator.Qr.Core;
using QrSharp.Core;

namespace QrSharp.Encoding
{
    public static class Interleaver
    {
        public static List<int> BuildStream(int version, IEccTableProvider eccTable, IReadOnlyList<int> dataCodewords)
        {
            var spec = eccTable.GetSpec(version);
            var gf = new GaloisField();
            var blocks = new List<(int[] data, int[] ecc)>();

            int k = 0;
            // Grupo 1
            for (int i = 0; i < spec.Group1.count; i++)
            {
                var blk = SliceCopy(dataCodewords, k, spec.Group1.dataBytes);
                k += spec.Group1.dataBytes;
                var ecc = ReedSolomon.EncodeBlock(gf, blk, spec.EcPerBlock);
                blocks.Add((blk, ecc));
            }
            // Grupo 2
            for (int i = 0; i < spec.Group2.count; i++)
            {
                var blk = SliceCopy(dataCodewords, k, spec.Group2.dataBytes);
                k += spec.Group2.dataBytes;
                var ecc = ReedSolomon.EncodeBlock(gf, blk, spec.EcPerBlock);
                blocks.Add((blk, ecc));
            }

            int maxDataLen = blocks.Max(b => b.data.Length);
            var outCW = new List<int>(spec.TotalCodewords);

            // interleave data
            for (int i = 0; i < maxDataLen; i++)
                foreach (var (data, _) in blocks)
                    if (i < data.Length) outCW.Add(data[i]);

            // interleave ECC
            for (int i = 0; i < spec.EcPerBlock; i++)
                foreach (var (_, ecc) in blocks) outCW.Add(ecc[i]);

            return outCW;
        }

        private static int[] SliceCopy(IReadOnlyList<int> src, int start, int len)
        {
            var dst = new int[len];
            for (int i = 0; i < len; i++) dst[i] = src[start + i];
            return dst;
        }
    }
}

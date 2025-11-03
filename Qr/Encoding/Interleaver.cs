using System;
using System.Collections.Generic;
using System.Linq;
using QrCodeGenerator.Qr.Core;
using QrSharp.Core;

namespace QrSharp.Encoding
{
    public static class Interleaver
    {
        /// <summary>
        /// Monta o stream final de codewords (dados intercalados + ECC intercalados).
        /// Agora recebe também o nível de ECC e usa o provider com assinatura nova.
        /// </summary>
        public static List<int> BuildStream(
            int version,
            EccLevel eccLevel,
            IEccTableProvider eccTable,
            IReadOnlyList<int> dataCodewords)
        {
            if (version < 1 || version > 40)
                throw new ArgumentOutOfRangeException(nameof(version));

            if (dataCodewords is null)
                throw new ArgumentNullException(nameof(dataCodewords));

            var spec = eccTable.GetSpec(version, eccLevel); // <<<<< assinatura nova
            var expectedDataBytes =
                spec.Group1.count * spec.Group1.dataBytes +
                spec.Group2.count * spec.Group2.dataBytes;

            if (dataCodewords.Count != expectedDataBytes)
                throw new ArgumentException(
                    $"Quantidade de data codewords ({dataCodewords.Count}) " +
                    $"não bate com a especificação para v{version} {eccLevel}: " +
                    $"{expectedDataBytes}.");

            var gf = new GaloisField();
            var blocks = new List<(int[] data, int[] ecc)>(spec.Group1.count + spec.Group2.count);

            int k = 0;

            // ----- Grupo 1 (blocos "curtos") -----
            for (int i = 0; i < spec.Group1.count; i++)
            {
                var blk = SliceCopy(dataCodewords, k, spec.Group1.dataBytes);
                k += spec.Group1.dataBytes;

                var ecc = ReedSolomon.EncodeBlock(gf, blk, spec.EcPerBlock);
                blocks.Add((blk, ecc));
            }

            // ----- Grupo 2 (blocos "longos") -----
            for (int i = 0; i < spec.Group2.count; i++)
            {
                var blk = SliceCopy(dataCodewords, k, spec.Group2.dataBytes);
                k += spec.Group2.dataBytes;

                var ecc = ReedSolomon.EncodeBlock(gf, blk, spec.EcPerBlock);
                blocks.Add((blk, ecc));
            }

            // Interleaving dos dados
            int maxDataLen = blocks.Count == 0 ? 0 : blocks.Max(b => b.data.Length);
            var outCW = new List<int>(spec.TotalCodewords);

            for (int i = 0; i < maxDataLen; i++)
            {
                foreach (var (data, _) in blocks)
                {
                    if (i < data.Length)
                        outCW.Add(data[i]);
                }
            }

            // Interleaving dos ECC (todos os blocos têm o mesmo EcPerBlock)
            for (int i = 0; i < spec.EcPerBlock; i++)
            {
                foreach (var (_, ecc) in blocks)
                    outCW.Add(ecc[i]);
            }

            // Sanidade opcional: o tamanho final deve bater TotalCodewords
            if (outCW.Count != spec.TotalCodewords)
            {
                throw new InvalidOperationException(
                    $"Stream final ({outCW.Count}) difere do TotalCodewords " +
                    $"({spec.TotalCodewords}) para v{version} {eccLevel}.");
            }

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

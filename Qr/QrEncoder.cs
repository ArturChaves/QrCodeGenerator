using QrCodeGenerator.Qr.Core;
using QrSharp.Core;
using QrSharp.Encoding;
using QrSharp.Matrix;
using System.Text;

namespace QrCodeGenerator.Qr
{
    public static class QrEncoder
    {
        /// <summary>
        /// Gera o bitmap do QR (true=preto) para o nível ECC informado, modo BYTE (ISO-8859-1).
        /// Usa o provider automático (estilo Nayuki) e o Interleaver com nova assinatura.
        /// </summary>
        public static bool[,] EncodeAuto(string text, EccLevel eccLevel)
        {
            var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(text ?? string.Empty);

            // Provider automático (GetSpec(version, level))
            IEccTableProvider ecc = new EccTableAuto();

            int chosen = PickVersion (bytes, eccLevel, ecc);
            if (chosen < 0) throw new ArgumentException($"Mensagem grande demais para v40-{eccLevel}.");

            // 1) Encode (modo BYTE) + pad até a capacidade de DADOS da versão/nível
            var bb = DataEncoderByte.Encode(bytes, chosen);
            var spec = ecc.GetSpec(chosen, eccLevel);

            int dataCap = spec.Group1.count * spec.Group1.dataBytes
                        + spec.Group2.count * spec.Group2.dataBytes;

            bb.PadToCodewords(dataCap);
            var dataCw = bb.ToCodewords();

            // 2) Interleave + ECC (assinatura nova)
            var stream = Interleaver.BuildStream(chosen, eccLevel, ecc, dataCw);

            // 3) Converte codewords para bits (MSB-first)
            var bits = new List<int>(stream.Count * 8);
            foreach (int cw in stream)
                for (int i = 7; i >= 0; i--) bits.Add((cw >> i) & 1);

            // 4) Template + placement
            var (baseMat, isFunc) = TemplateBuilder.Build(chosen);
            var placed = DataPlacer.Place(baseMat, isFunc, bits);

            // 5) Testa máscaras e escreve formato/versão
            int eclBits = EclToFormatBits(eccLevel);
            int bestScore = int.MaxValue; int[,] best = null!; int bestMask = 0;

            for (int mask = 0; mask < 8; mask++)
            {
                var m = Masking.Apply(placed, isFunc, mask);
                FormatVersionWriter.WriteFormat(m, mask, eclBits);
                if (chosen >= 7) FormatVersionWriter.WriteVersion(m, chosen);

                int score = Masking.ScorePenalty(m);
                if (score < bestScore) { bestScore = score; bestMask = mask; best = m; }
            }

            // 6) int -> bool
            int n = Tables.VersionSize(chosen);
            var bm = new bool[n, n];
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    bm[r, c] = best[r, c] == 1;

            return bm;
        }

        /// <summary>
        /// Escolhe a menor versão que comporta a mensagem para o nível ECC informado.
        /// </summary>
        private static int PickVersion(ReadOnlySpan<byte> data, EccLevel level, IEccTableProvider ecc)
        {

            var baseBuffer = new BitBuffer();
            baseBuffer.Append(0b0100, 4);              

            baseBuffer.Append(data.Length, 16);

            for (int i = 0; i < data.Length; i++)
                baseBuffer.Append(data[i], 8);

            for (int ver = 1; ver <= 40; ver++)
            {
                var spec = ecc.GetSpec(ver, level);
                int cap = spec.Group1.count * spec.Group1.dataBytes
                        + spec.Group2.count * spec.Group2.dataBytes;

                int countBits = Tables.CountBitsForVersion(ver);

                var test = baseBuffer.Clone();

                if (countBits == 8)
                {
                    test.ReplaceBits(start: 4, length: 16, newValue: data.Length, bitCount: 8);
                }
                else
                {
                    test.ReplaceBits(start: 4, length: 16, newValue: data.Length, bitCount: 16);
                }
                test.PadToCodewords(cap);

                if (test.ToCodewords().Count == cap)
                    return ver;
            }

            return -1;
        }


        /// <summary>
        /// Mapeia o nível ECC para os 2 bits do campo "format information".
        /// Convenção oficial: L=01, M=00, Q=11, H=10.
        /// </summary>
        private static int EclToFormatBits(EccLevel level) => level switch
        {
            EccLevel.L => 0b01,
            EccLevel.M => 0b00,
            EccLevel.Q => 0b11,
            EccLevel.H => 0b10,
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }
}

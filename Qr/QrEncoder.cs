using System;
using System.Collections.Generic;
using System.Text;
using QrSharp.Core;
using QrSharp.Encoding;
using QrSharp.Matrix;

namespace QrCodeGenerator.Qr
{
    public static class QrEncoder
    {
        /// <summary>
        /// Gera o bitmap do QR (true=preto) no nível ECC M, modo BYTE (ISO-8859-1).
        /// </summary>
        public static bool[,] EncodeAutoM(string text, Action<string>? log = null)
        {
            var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(text ?? "");
            var ecc = new EccTableM();

            int chosen = PickVersion(bytes, ecc, log);
            if (chosen < 0) throw new ArgumentException("Mensagem grande demais para v40-M.");

            // 1) Encode + pad
            var bb = DataEncoderByte.Encode(bytes, chosen);
            var spec = ecc.GetSpec(chosen);
            int dataCap = spec.Group1.count * spec.Group1.dataBytes + spec.Group2.count * spec.Group2.dataBytes;
            bb.PadToCodewords(dataCap);
            var dataCw = bb.ToCodewords();

            // 2) Interleave + ECC
            var stream = Interleaver.BuildStream(chosen, ecc, dataCw);

            // 3) Bits MSB-first
            var bits = new List<int>(stream.Count * 8);
            foreach (int cw in stream) for (int i = 7; i >= 0; i--) bits.Add((cw >> i) & 1);

            // 4) Template + placement
            var (baseMat, isFunc) = TemplateBuilder.Build(chosen);
            var placed = DataPlacer.Place(baseMat, isFunc, bits);

            // 5) Máscaras e escrita formato/versão
            int bestScore = int.MaxValue; int[,] best = null!; int bestMask = 0;
            for (int mask = 0; mask < 8; mask++)
            {
                var m = Masking.Apply(placed, isFunc, mask);
                FormatVersionWriter.WriteFormat(m, mask, eclBits: 0b00); // M
                if (chosen >= 7) FormatVersionWriter.WriteVersion(m, chosen);
                int score = Masking.ScorePenalty(m);
                if (score < bestScore) { bestScore = score; bestMask = mask; best = m; }
            }
            log?.Invoke($"version={chosen}, mask={bestMask}, penalty={bestScore}");

            // 6) int->bool
            int n = Tables.VersionSize(chosen);
            var bm = new bool[n, n];
            for (int r = 0; r < n; r++) for (int c = 0; c < n; c++) bm[r, c] = best[r, c] == 1;
            return bm;
        }

        private static int PickVersion(ReadOnlySpan<byte> data, IEccTableProvider ecc, Action<string>? log)
        {
            for (int ver = 1; ver <= 40; ver++)
            {
                var spec = ecc.GetSpec(ver);
                int cap = spec.Group1.count * spec.Group1.dataBytes + spec.Group2.count * spec.Group2.dataBytes;
                int countBits = Tables.CountBitsForVersion(ver);

                var bb = new BitBuffer();
                bb.Append(0b0100, 4);             // BYTE
                bb.Append(data.Length, countBits);
                foreach (var b in data) bb.Append(b, 8);

                var test = bb.Clone(); test.PadToCodewords(cap);
                if (test.ToCodewords().Count == cap)
                {
                    log?.Invoke($"fits in v{ver}");
                    return ver;
                }
            }
            return -1;
        }
    }
}

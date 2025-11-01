using System.Text;
using QrCodeGenerator.Qr.Core;
using QrSharp.Core;

namespace QrSharp.Encoding
{
    public static class DataEncoderByte
    {
        /// <summary>Codifica dados no modo BYTE (ISO-8859-1 por padrão).</summary>
        public static BitBuffer Encode(ReadOnlySpan<byte> data, int version)
        {
            int countBits = Tables.CountBitsForVersion(version);
            var bb = new BitBuffer();
            bb.Append(0b0100, 4);                 // modo BYTE
            bb.Append(data.Length, countBits);    // char count
            foreach (var b in data) bb.Append(b, 8);
            return bb;
        }
    }
}

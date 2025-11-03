using QrCodeGenerator.Qr.Core;

namespace QrSharp.Encoding
{
    public static class DataEncoderByte
    {
        public static BitBuffer Encode(ReadOnlySpan<byte> data, int version)
        {
            int countBits = Tables.CountBitsForVersion(version);
            var bb = new BitBuffer();
            bb.Append(0b0100, 4);
            bb.Append(data.Length, countBits);
            foreach (var b in data) bb.Append(b, 8);
            return bb;
        }
    }
}

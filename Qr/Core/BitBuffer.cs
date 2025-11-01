using System.Collections.Generic;

namespace QrCodeGenerator.Qr.Core
{
    public sealed class BitBuffer
    {
        private readonly List<int> _bits = new(1024);
        public IReadOnlyList<int> Bits => _bits;

        public void Append(int val, int length)
        {
            for (int i = length - 1; i >= 0; i--)
                _bits.Add((val >> i) & 1);
        }

        public void PadToCodewords(int totalBytes)
        {
            int capBits = totalBytes * 8;

            int remain = capBits - _bits.Count;
            int t = remain > 4 ? 4 : (remain < 0 ? 0 : remain);
            Append(0, t);                       // terminator (até 4 bits)
            while (_bits.Count % 8 != 0) _bits.Add(0); // byte align

            bool toggle = true;
            while (_bits.Count < capBits)
            {
                int b = toggle ? 0xEC : 0x11;   // pad bytes alternando
                toggle = !toggle;
                for (int i = 7; i >= 0; i--) _bits.Add((b >> i) & 1);
            }
        }

        public List<int> ToCodewords()
        {
            var outCW = new List<int>(_bits.Count / 8);
            for (int i = 0; i < _bits.Count; i += 8)
            {
                int x = 0;
                for (int j = 0; j < 8; j++) x = (x << 1) | _bits[i + j];
                outCW.Add(x & 0xFF);
            }
            return outCW;
        }

        public BitBuffer Clone()
        {
            var bb = new BitBuffer();
            bb._bits.AddRange(_bits);
            return bb;
        }
    }
}

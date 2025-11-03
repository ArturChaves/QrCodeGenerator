// Tables.cs
namespace QrSharp.Core
{
    public enum EccLevel { L = 0, M = 1, Q = 2, H = 3 }

    public readonly record struct EccBlockSpec(
        int TotalCodewords,
        int EcPerBlock,
        (int count, int dataBytes) Group1,
        (int count, int dataBytes) Group2);

    public interface IEccTableProvider
    {
        EccBlockSpec GetSpec(int version, EccLevel level); // 1..40
    }

    public static class Tables
    {
        // ----------------------------
        // Alignment pattern centers
        // ----------------------------
        public static readonly Dictionary<int, int[]> ALIGNMENT = new()
        {
            {1, Array.Empty<int>()},
            {2, new[]{6,18}},{3,new[]{6,22}},{4,new[]{6,26}},{5,new[]{6,30}},{6,new[]{6,34}},
            {7,new[]{6,22,38}},{8,new[]{6,24,42}},{9,new[]{6,26,46}},{10,new[]{6,28,50}},
            {11,new[]{6,30,54}},{12,new[]{6,32,58}},{13,new[]{6,34,62}},
            {14,new[]{6,26,46,66}},{15,new[]{6,26,48,70}},{16,new[]{6,26,50,74}},
            {17,new[]{6,30,54,78}},{18,new[]{6,30,56,82}},{19,new[]{6,30,58,86}},
            {20,new[]{6,34,62,90}},
            {21,new[]{6,28,50,72,94}},{22,new[]{6,26,50,74,98}},{23,new[]{6,30,54,78,102}},
            {24,new[]{6,28,54,80,106}},{25,new[]{6,32,58,84,110}},
            {26,new[]{6,30,58,86,114}},{27,new[]{6,34,62,90,118}},
            {28,new[]{6,26,50,74,98,122}},{29,new[]{6,30,54,78,102,126}},
            {30,new[]{6,26,52,78,104,130}},
            {31,new[]{6,30,56,82,108,134}},{32,new[]{6,34,60,86,112,138}},
            {33,new[]{6,30,58,86,114,142}},{34,new[]{6,34,62,90,118,146}},
            {35,new[]{6,30,54,78,102,126,150}},{36,new[]{6,24,50,76,102,128,154}},
            {37,new[]{6,28,54,80,106,132,158}},{38,new[]{6,32,58,84,110,136,162}},
            {39,new[]{6,26,54,82,110,138,166}},{40,new[]{6,30,58,86,114,142,170}},
        };

        public static int VersionSize(int ver) => 17 + 4 * ver;
        public static int CountBitsForVersion(int ver) => (ver <= 9 ? 8 : 16);

        // --------------------------------------------------------------------
        // A partir do Project Nayuki (MIT). Índices: [ECC(L/M/Q/H)][version].
        // ECC_CODEWORDS_PER_BLOCK[ecc, ver] = nº de codewords ECC por BLOCO.
        // NUM_ERROR_CORRECTION_BLOCKS[ecc, ver] = nº de BLOCOS (podem ter
        // comprimentos de dados diferentes: alguns "curtos", alguns "longos").
        // Fontes: arrays publicados no código do projeto e na doc Doxygen.
        // --------------------------------------------------------------------
        private static readonly sbyte[,] ECC_CODEWORDS_PER_BLOCK = new sbyte[4, 41]
        {
            // L (index 0)
            { -1,  7,10,15,20,26,18,20,24,30,18,20,24,26,30,22,24,28,30,28,28,28,28,30,30,26,28,30,30,30,30,30,30,30,30,30,30,30,30,30,30 },
            // M (index 1)
            { -1, 10,16,26,18,24,16,18,22,22,26,30,22,22,24,24,28,28,26,26,26,26,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28 },
            // Q (index 2)
            { -1, 13,22,18,26,18,24,18,22,20,24,28,26,24,20,30,24,28,28,26,30,28,30,30,30,30,28,30,30,30,30,30,30,30,30,30,30,30,30,30,30 },
            // H (index 3)
            { -1, 17,28,22,16,22,28,26,26,24,28,24,28,22,24,24,30,28,28,26,28,30,24,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30 }
        };

        private static readonly sbyte[,] NUM_ERROR_CORRECTION_BLOCKS = new sbyte[4, 41]
        {
            // L
            { -1,  1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8, 8, 9, 9,10,12,12,12,13,14,15,16,17,18,19,19,20,21,22,24,25 },
            // M
            { -1,  1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9,10,10,11,13,14,16,17,17,18,20,21,23,25,26,28,29,31,33,35,37,38,40,43,45,47,49 },
            // Q
            { -1,  1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8,10,12,16,12,17,16,18,21,20,23,23,25,27,29,34,34,35,38,40,43,45,48,51,53,56,59,62,65,68 },
            // H
            { -1,  1, 1, 2, 4, 4, 4, 5, 6, 8, 8,11,11,16,16,18,16,19,21,25,25,25,34,30,32,35,37,40,42,45,48,51,54,57,60,63,66,70,74,77,81 }
        };

        // ----------------------------
        // Cálculos de capacidade
        // ----------------------------

        // Nº de módulos "brutos" destinados a dados (inclui remainders),
        // fórmula equivalente à usada pelo Nayuki.
        public static int GetNumRawDataModules(int ver)
        {
            if (ver < 1 || ver > 40) throw new ArgumentOutOfRangeException(nameof(ver));
            // (16*ver + 128)*ver + 64  ==  (4*ver + 17)^2
            int result = (16 * ver + 128) * ver + 64;

            if (ver >= 2)
            {
                int numAlign = ver / 7 + 2;                  // quantidade de alinhamentos numa direção
                result -= (25 * numAlign - 10) * numAlign - 55;
                if (ver >= 7) result -= 36;                  // 2 áreas de "version information"
            }
            return result;
        }

        // Nº de codewords de DADOS (descarta remainder bits)
        public static int GetNumDataCodewords(int ver, EccLevel level)
        {
            int totalCodewords = GetNumRawDataModules(ver) / 8;
            int eccPerBlock = ECC_CODEWORDS_PER_BLOCK[(int)level, ver];
            int numBlocks = NUM_ERROR_CORRECTION_BLOCKS[(int)level, ver];
            return totalCodewords - (eccPerBlock * numBlocks);
        }

        // Helpers para obter ECC por bloco e nº de blocos
        public static int GetEccPerBlock(int ver, EccLevel level) =>
            ECC_CODEWORDS_PER_BLOCK[(int)level, ver];

        public static int GetNumBlocks(int ver, EccLevel level) =>
            NUM_ERROR_CORRECTION_BLOCKS[(int)level, ver];
    }

    // ------------------------------------------------------------
    // Provider que calcula Group1/Group2 dinamicamente (estilo Nayuki)
    // ------------------------------------------------------------
    public sealed class EccTableAuto : IEccTableProvider
    {
        /// <summary>
        /// Retorna EccBlockSpec para a versão/nivel informados, com divisão correta
        /// entre blocos "curtos" e "longos" em função do resto da divisão inteira.
        /// </summary>
        public EccBlockSpec GetSpec(int version, EccLevel level)
        {
            if (version < 1 || version > 40) throw new ArgumentOutOfRangeException(nameof(version));

            int totalCodewords = Tables.GetNumRawDataModules(version) / 8;
            int eccPerBlock = Tables.GetEccPerBlock(version, level);
            int numBlocks = Tables.GetNumBlocks(version, level);

            int totalEcc = eccPerBlock * numBlocks;
            int totalData = totalCodewords - totalEcc;

            // Distribuição dos dados entre os blocos:
            // alguns blocos têm 'shortLen', e (totalData % numBlocks) blocos têm 'longLen = shortLen+1'
            int longBlocks = totalData % numBlocks;
            int shortBlocks = numBlocks - longBlocks;

            int longLen = totalData / numBlocks + (longBlocks > 0 ? 1 : 0);
            int shortLen = longLen - (longBlocks > 0 ? 1 : 0);

            // Se não existir um dos grupos, mantemos (0,0)
            var g1 = shortBlocks > 0 ? (shortBlocks, shortLen) : (0, 0);
            var g2 = longBlocks > 0 ? (longBlocks, longLen) : (0, 0);

            return new EccBlockSpec(
                TotalCodewords: totalCodewords,
                EcPerBlock: eccPerBlock,
                Group1: g1,
                Group2: g2
            );
        }
    }
}

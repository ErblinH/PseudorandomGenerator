namespace PseudorandomGenerator.Application;

public class GaloisField
{
    private static readonly byte[] expTable = new byte[256];
    private static readonly byte[] logTable = new byte[256];
    private const byte irreduciblePolynomial = 0x1B; // x^8 + x^4 + x^3 + x + 1

    static GaloisField()
    {
        InitializeTables();
    }

    private static void InitializeTables()
    {
        byte value = 1;
        for (int i = 0; i < 256; i++)
        {
            expTable[i] = value;
            logTable[value] = (byte)i;
            value = Multiply(value, 0x02);
        }
    }

    private static byte Multiply(byte a, byte b)
    {
        byte result = 0;
        while (b != 0)
        {
            if ((b & 1) != 0)
                result ^= a;
            a = (byte)((a << 1) ^ ((a & 0x80) != 0 ? irreduciblePolynomial : (byte)0));
            b >>= 1;
        }
        return result;
    }

    public static byte Add(byte a, byte b) => (byte)(a ^ b);

    public static byte Subtract(byte a, byte b) => (byte)(a ^ b);

    public static byte Inverse(byte a) => expTable[255 - logTable[a]];

    public static byte GFMultiply(byte a, byte b) => (a == 0 || b == 0) ? (byte)0 : expTable[(logTable[a] + logTable[b]) % 255];

}

namespace PseudorandomGenerator.Application.Detyra4;

public class MDSCalculator
{
    private const int IRREDUCIBLE_POLYNOMIAL = 0x11b;

    public static bool CheckMDSMatrix(int[,] data)
    {
        var matrix_0 = TruncateMDSMatrix(3, 0, 0, data);
        var matrix_1 = TruncateMDSMatrix(3, 1, 0, data);
        var matrix_2 = TruncateMDSMatrix(3, 2, 0, data);
        var matrix_3 = TruncateMDSMatrix(3, 3, 0, data);

        //Determinants of the third order

        var row1 = CalculateDeterminant(matrix_0);
        var row2 = (-1) * CalculateDeterminant(matrix_1);
        var row3 = CalculateDeterminant(matrix_2);
        var row4 = (-1) * CalculateDeterminant(matrix_3);

        var sum_row = row1 + row2 + row3 + row4;

        return true;
    }

    public static int[,] TruncateMDSMatrix(int n, int row, int column, int[,] matrix)
    {
        // Initialize an nxn MDS matrix
        int[,] reducedMatrix = new int[n, n];

        int rowOffset = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i == row)
            {
                rowOffset = 1;
                continue;
            }

            int colOffset = 0;
            for (int j = 0; j < 4; j++)
            {
                if (j == column)
                {
                    colOffset = 1;
                    continue;
                }

                // Fill the new 3x3 matrix, skipping the removed row and column
                reducedMatrix[i - rowOffset, j - colOffset] = matrix[i, j];
            }
        }

        return reducedMatrix;
    }

    public static int CalculateDeterminant(int[,] matrix)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
        {
            throw new ArgumentException("The input matrix must be 3x3.");
        }

        // Elements of the matrix
        int a = matrix[0, 0];
        int b = matrix[0, 1];
        int c = matrix[0, 2];
        int d = matrix[1, 0];
        int e = matrix[1, 1];
        int f = matrix[1, 2];
        int g = matrix[2, 0];
        int h = matrix[2, 1];
        int i = matrix[2, 2];

        // Using the determinant formula for a 3x3 matrix
        int determinant = a * (e * i - f * h)
                          - b * (d * i - f * g)
                          + c * (d * h - e * g);

        return determinant;
    }

    public static int[,] GenerateMDSMatrix(int n)
    {
        // Initialize an nxn MDS matrix
        int[,] mdsMatrix = new int[n, n];

        // Example: Use Vandermonde matrix construction for MDS property in GF(2)
        // This is a simple and common method for constructing an MDS matrix.
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                // A Vandermonde matrix is defined as:
                // A[i, j] = i^j (in the field GF(2), we compute mod 2)
                mdsMatrix[i, j] = (int)Math.Pow(i + 1, j) % 2;
            }
        }

        return mdsMatrix;
    }

    public static void PrintMatrix(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public static byte GFMultiply(byte a, byte b)
    {
        byte result = 0;

        for (int i = 0; i < 8; i++)
        {
            if ((b & 1) != 0) // If the lowest bit of b is set
            {
                result ^= a; // Add a to the result (in GF(2^8), addition is XOR)
            }

            bool carry = (a & 0x80) != 0; // Check if a has a leading 1 (degree 8)
            a <<= 1; // Multiply a by x

            if (carry)
            {
                a ^= unchecked((byte)IRREDUCIBLE_POLYNOMIAL); // Reduce modulo the irreducible polynomial
            }

            b >>= 1; // Move to the next bit of b
        }

        return result;
    }

    // Determinant of a 3x3 matrix in GF(2^8)
    public static byte Determinant3x3(byte[,] matrix)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
        {
            throw new ArgumentException("Matrix must be 3x3.");
        }

        byte a = matrix[0, 0];
        byte b = matrix[0, 1];
        byte c = matrix[0, 2];
        byte d = matrix[1, 0];
        byte e = matrix[1, 1];
        byte f = matrix[1, 2];
        byte g = matrix[2, 0];
        byte h = matrix[2, 1];
        byte i = matrix[2, 2];

        // Calculating the determinant using the standard formula:
        byte term1 = GFMultiply(a, GFAdd(GFMultiply(e, i), GFMultiply(f, h)));
        byte term2 = GFMultiply(b, GFAdd(GFMultiply(d, i), GFMultiply(f, g)));
        byte term3 = GFMultiply(c, GFAdd(GFMultiply(d, h), GFMultiply(e, g)));

        return GFAdd(GFAdd(term1, term2), term3);
    }

    public static byte GFAdd(byte a, byte b)
    {
        return (byte)(a ^ b);
    }

    // Print matrix utility
    public static void PrintMatrix(byte[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{matrix[i, j]:X2} ");
            }
            Console.WriteLine();
        }
    }
}
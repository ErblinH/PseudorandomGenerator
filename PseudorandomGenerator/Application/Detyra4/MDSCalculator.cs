namespace PseudorandomGenerator.Application.Detyra4;

public class MDSCalculator
{
    private const int IRREDUCIBLE_POLYNOMIAL = 0x11b;

    public static bool CheckMDSMatrix(byte[,] data)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (data[i, j] == 0)
                {
                    return false;
                }
            }
        }

        var matrix_0 = TruncateMDSMatrix(3, 0, 0, data);
        var matrix_1 = TruncateMDSMatrix(3, 1, 0, data);
        var matrix_2 = TruncateMDSMatrix(3, 2, 0, data);
        var matrix_3 = TruncateMDSMatrix(3, 3, 0, data);

        var row1 = 1 * Determinant3x3(matrix_0);
        var row2 = (-1) * Determinant3x3(matrix_1);
        var row3 = 1 * Determinant3x3(matrix_2);
        var row4 = (-1) * Determinant3x3(matrix_3);

        var sum_row = row1 + row2 + row3 + row4;

        if (sum_row == 0) return false;

        if (!CheckMinorDeterminant(matrix_0) || !CheckMinorDeterminant(matrix_1) || !CheckMinorDeterminant(matrix_2) || !CheckMinorDeterminant(matrix_3))
            return false;

        return true;
    }

    public static bool CheckMinorDeterminant(byte[,] matrix)
    {
        var matrix_0 = TruncateMDSMatrix(2, 0, 0, matrix);
        var matrix_1 = TruncateMDSMatrix(2, 1, 0, matrix);
        var matrix_2 = TruncateMDSMatrix(2, 2, 0, matrix);

        if (!IsDeterminantNonZero(matrix_0) || !IsDeterminantNonZero(matrix_1) || !IsDeterminantNonZero(matrix_2))
            return false;

        return true;
    }

    public static bool IsDeterminantNonZero(byte[,] matrix)
    {
        byte a = matrix[0, 0];
        byte b = matrix[0, 1];
        byte c = matrix[1, 0];
        byte d = matrix[1, 1];

        int determinant = a * d - b * c;

        if (determinant == 0)
        {
            return false;
        }

        return true;
    }

    public static byte[,] TruncateMDSMatrix(int n, int row, int column, byte[,] matrix)
    {
        // Initialize a 3x3 matrix since we are removing one row and one column from a 4x4 matrix
        byte[,] reducedMatrix = new byte[n, n];

        int length = matrix.GetLength(0);

        int rowOffset = 0;
        for (int i = 0; i < length; i++)
        {
            if (i == row)
            {
                rowOffset = 1;
                continue;
            }

            int colOffset = 0;
            for (int j = 0; j < length; j++)
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

    public static byte FlipOneBit(byte value, int bitPosition)
    {
        return (byte)(value ^ (1 << bitPosition));
    }

    public static byte[] FindRightColumnValues(byte[] leftColumn)
    {
        byte[] rightColumn = new byte[leftColumn.Length];

        for (int i = 0; i < leftColumn.Length; i++)
        {
            byte leftValue = leftColumn[i];

            rightColumn[i] = FlipOneBit(leftValue, 0);
        }

        return rightColumn;
    }

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

    public static byte[,] MultiplyMatricesGF(byte[,] A, byte[,] B)
    {
        int n = A.GetLength(0); // Assuming both matrices are nxn
        byte[,] result = new byte[n, n];

        // Multiply matrices: result[i,j] = sum(A[i,k] * B[k,j]) for all k
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                byte sum = 0;

                for (int k = 0; k < n; k++)
                {
                    byte product = GFMultiply(A[i, k], B[k, j]);
                    sum = GFAdd(sum, product); // Add in GF(2^8)
                }

                result[i, j] = sum;
            }
        }

        return result;
    }

    public static byte[] GetColumn(byte[,] matrix, int columnIndex)
    {
        byte[] column = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            column[i] = matrix[i, columnIndex];
        }

        return column;
    }

    public static byte[,] FindMatrix(byte[,] matrix)
    {
        var column_0 = GetColumn(matrix, 0);
        var column_1 = GetColumn(matrix, 1);
        var column_2 = GetColumn(matrix, 2);
        var column_3 = GetColumn(matrix, 3);

        var right_column_0 = FindRightColumnValues(column_0);
        var right_column_1 = FindRightColumnValues(column_1);
        var right_column_2 = FindRightColumnValues(column_2);
        var right_column_3 = FindRightColumnValues(column_3);

        byte[,] new_matrix = new byte[4, 4];

        // Combine the columns into the matrix
        for (int i = 0; i < 4; i++)
        {
            new_matrix[i, 0] = right_column_0[i]; // First column
            new_matrix[i, 1] = right_column_1[i]; // Second column
            new_matrix[i, 2] = right_column_2[i]; // Third column
            new_matrix[i, 3] = right_column_3[i]; // Fourth column
        }

        return new_matrix;
    }

    public static int CalculateHammingDistance(byte[] col1, byte[] col2)
    {
        int hammingDistance = 0;

        // Compare the elements of the two columns
        for (int i = 0; i < col1.Length; i++)
        {
            if (col1[i] != col2[i])
            {
                hammingDistance++; // Increment the counter if the elements differ
            }
        }

        return hammingDistance;
    }

    public static bool CheckDifusionEffect(byte[,] a, byte[,] b)
    {
        var a_column_0 = GetColumn(a, 0);
        var b_column_0 = GetColumn(b, 0);

        var dist_0 = CalculateHammingDistance(a_column_0, b_column_0);

        if (dist_0 < 16) return false;

        var a_column_1 = GetColumn(a, 1);
        var b_column_1 = GetColumn(b, 1);

        var dist_1 = CalculateHammingDistance(a_column_1, b_column_1);

        if (dist_1 < 16) return false;

        var a_column_2 = GetColumn(a, 2);
        var b_column_2 = GetColumn(b, 2);

        var dist_2 = CalculateHammingDistance(a_column_2, b_column_2);

        if (dist_2 < 16) return false;

        var a_column_3 = GetColumn(a, 3);
        var b_column_3 = GetColumn(b, 3);

        var dist_3 = CalculateHammingDistance(a_column_3, b_column_3);

        if (dist_3 < 16) return false;

        return true;
    }
}
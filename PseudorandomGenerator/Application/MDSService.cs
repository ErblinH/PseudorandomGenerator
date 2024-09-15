namespace PseudorandomGenerator.Application;

public class MDSService
{
    #region GenerateMDS

    private readonly byte[,] matrix;
    private readonly int size;

    public MDSService(int size)
    {
        this.size = size;
        matrix = new byte[size, size];
        GenerateMDSMatrix();
    }

    private void GenerateMDSMatrix()
    {
        // Example using a Cauchy matrix for MDS (4x4 for simplicity)
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                matrix[i, j] = GaloisField.Inverse((byte)(i ^ j));
            }
        }
    }

    public byte[,] PrintMatrix()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Console.Write($"{matrix[i, j]:X2} ");
            }
            Console.WriteLine();
        }

        return matrix;
    }

    public byte Determinant(byte[,] matrix)
    {
        if (matrix.GetLength(0) != 4 || matrix.GetLength(1) != 4)
            throw new ArgumentException("Matrix must be 4x4.");

        // Calculate the determinant using Laplace expansion for 4x4 matrix
        byte det = 0;
        for (int i = 0; i < 4; i++)
        {
            det = GaloisField.Add(det, GaloisField.GFMultiply(matrix[0, i], Cofactor(matrix, 0, i)));
        }
        return det;
    }

    private static byte Cofactor(byte[,] matrix, int row, int col)
    {
        byte[,] subMatrix = new byte[3, 3];
        for (int i = 0, subi = 0; i < 4; i++)
        {
            if (i == row) continue;
            for (int j = 0, subj = 0; j < 4; j++)
            {
                if (j == col) continue;
                subMatrix[subi, subj++] = matrix[i, j];
            }
            subi++;
        }
        return GaloisField.GFMultiply((row + col) % 2 == 0 ? (byte)1 : (byte)0xFF, Determinant3x3(subMatrix));
    }

    private static byte Determinant3x3(byte[,] matrix)
    {
        // Calculate the determinant of a 3x3 matrix
        return GaloisField.Add(
            GaloisField.Add(
                GaloisField.GFMultiply(matrix[0, 0], GaloisField.Subtract(GaloisField.GFMultiply(matrix[1, 1], matrix[2, 2]), GaloisField.GFMultiply(matrix[1, 2], matrix[2, 1]))),
                GaloisField.GFMultiply(matrix[0, 1], GaloisField.Subtract(GaloisField.GFMultiply(matrix[1, 2], matrix[2, 0]), GaloisField.GFMultiply(matrix[1, 0], matrix[2, 2])))),
            GaloisField.GFMultiply(matrix[0, 2], GaloisField.Subtract(GaloisField.GFMultiply(matrix[1, 0], matrix[2, 1]), GaloisField.GFMultiply(matrix[1, 1], matrix[2, 0])))
        );
    }
    #endregion
}
namespace PseudorandomGenerator.Application;

public class DecryptSimplifiedAES
{
    private string[,] ciphertext = {
        {"3", "7" },
        {"2", "2"}
    };

    private string[,] key = {
        { "3", "B" },
        { "F", "6"}
    };

    private string[,] SBOX = {
        { "6", "B", "0", "4" },
        { "7", "E", "2", "F" },
        { "9", "8", "A", "C" },
        { "3", "1", "5", "D" }
    };

    private string[,] inverse_SBOX = {
        { "2", "D", "6", "C" },
        { "3", "E", "0", "4" },
        { "9", "8", "A", "1" },
        { "B", "F", "5", "7" }
    };

    private string[,] mds = {
        { "1", "1" },
        { "1", "2"}
    };

    private string[,] inverse_mds = {
        { "2", "F" },
        { "D", "A" }
    };

    public void Decrypt()
    {
        //First round decryption
        Console.WriteLine("Starting first round decryption");

        // Inverse AddRoundKey
        var roundKey = XORMatrixOperation(ciphertext, key);
        PrintMatrix(roundKey, "Inverse AddRoundKey result");

        // Inverse ShiftRows
        int lastRow = roundKey.GetLength(0) - 1;
        (roundKey[lastRow, 0], roundKey[lastRow, 1]) = (roundKey[lastRow, 1], roundKey[lastRow, 0]);
        PrintMatrix(roundKey, "Inverse ShiftRows result");

        // Inverse SBOX Substitution
        var resultMatrixSBOX = InverseSBOXSubstitution(roundKey);
        PrintMatrix(resultMatrixSBOX, "Inverse SBOX Substitution result");

        //Second round
        Console.WriteLine("Starting second round");
        var secondRound = EncryptRound(resultMatrixSBOX, 2);

        //Third round
        Console.WriteLine("Starting third round");
        var thirdRound = EncryptRound(secondRound, 1);

        PrintMatrix(thirdRound, "Final encrypted result matrix");
    }

    private string[,] EncryptRound(string[,] matrix, int count)
    {
        //Generate new key
        key = GenerateKey(key, count);
        PrintMatrix(key, $"Key matrix of round:{count}");

        //AddRoundKey
        var roundKey = XORMatrixOperation(matrix, key);
        PrintMatrix(roundKey, "Round Key result matrix");

        //MixColumns
        string[,] resultMatrixMixColumns = MultiplyMatricesGFInverse(mds, roundKey);
        PrintMatrix(resultMatrixMixColumns, "Mix Columns result matrix");

        //ShiftRows
        int lastRow = resultMatrixMixColumns.GetLength(0) - 1;
        (resultMatrixMixColumns[lastRow, 0], resultMatrixMixColumns[lastRow, 1]) = (resultMatrixMixColumns[lastRow, 1], resultMatrixMixColumns[lastRow, 0]);
        PrintMatrix(resultMatrixMixColumns, "Shift Rows result matrix");

        //SBOX
        var resultMatrixSBOX = InverseSBOXSubstitution(resultMatrixMixColumns);
        PrintMatrix(resultMatrixSBOX, "SBOX Substitution result matrix");

        return roundKey;
    }

    private string[,] InverseSBOXSubstitution(string[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        string[,] resultMatrixSBOX = new string[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int value = Convert.ToInt32(matrix[i, j], 16);

                int row = (value & 0xC) >> 2;
                int col = value & 0x3;

                resultMatrixSBOX[i, j] = inverse_SBOX[row, col];
            }
        }

        return resultMatrixSBOX;
    }

    private string[,] MultiplyMatricesGFInverse(string[,] matrixA, string[,] matrixB)
    {
        // This will use the inverse of MixColumns matrix (inverse_mds)
        string[,] resultMatrix = new string[2, 2];

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int result = 0;
                for (int k = 0; k < 2; k++)
                {
                    int valueA = Convert.ToInt32(matrixA[i, k], 16);
                    int valueB = Convert.ToInt32(matrixB[k, j], 16);
                    result ^= GFMul(valueA, valueB);
                }
                resultMatrix[i, j] = result.ToString("X");
            }
        }

        return resultMatrix;
    }

    private static int GFMul(int a, int b)
    {
        int p = 0;
        int carry;
        int irreducible = 0x13; // x^4 + x + 1

        for (int i = 0; i < 4; i++)
        {
            if ((b & 1) != 0)
            {
                p ^= a;
            }

            carry = a & 0x8;
            a <<= 1;

            if (carry != 0)
            {
                a ^= irreducible;
            }

            b >>= 1;
        }

        return p & 0xF; // Mask to get only the lower 4 bits
    }

    private string XOR(string first, string second)
    {
        int value1 = Convert.ToInt32(first, 16);
        int value2 = Convert.ToInt32(second, 16);

        return (value1 ^ value2).ToString("X");
    }

    private void PrintMatrix(string[,] matrix, string message)
    {
        Console.WriteLine(message);

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

    private string[,] XORMatrixOperation(string[,] first, string[,] second)
    {
        int rows = first.GetLength(0);
        int cols = first.GetLength(1);

        string[,] resultMatrix = new string[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int value1 = Convert.ToInt32(first[i, j], 16);
                int value2 = Convert.ToInt32(second[i, j], 16);

                int result = value1 ^ value2;

                resultMatrix[i, j] = result.ToString("X");
            }
        }

        return resultMatrix;
    }

    private string[,] GenerateKey(string[,] key, int count)
    {
        var k0 = key[0, 0];
        var k1 = key[1, 0];

        var k2 = key[0, 1];
        var k3 = key[1, 1];

        var gvalue = GFunction(k3, count);

        var k0_1 = XOR(k0, gvalue);

        var k1_1 = XOR(k0_1, k1);

        var k2_1 = XOR(k1_1, k2);

        var k3_1 = XOR(k2_1, k3);

        string[,] key_updated = {
            { k0_1, k2_1 },
            { k1_1, k3_1}
        };

        return key_updated;
    }

    private string GFunction(string hexElement, int count)
    {
        List<int> binaryList = new List<int>();

        int value = Convert.ToInt32(hexElement, 16);

        string binaryString = Convert.ToString(value, 2).PadLeft(4, '0');

        foreach (char bit in binaryString)
        {
            binaryList.Add(bit - '0');
        }

        List<int> rotatedList = new List<int>();

        for (int i = 1; i < binaryList.Count; i++)
        {
            rotatedList.Add(binaryList[i]);
        }

        rotatedList.Add(binaryList[0]);

        int row = (rotatedList[0] << 1) | rotatedList[1];

        int col = (rotatedList[2] << 1) | rotatedList[3];

        var sboxMpped = SBOX[row, col];

        int value1 = Convert.ToInt32(sboxMpped, 16);
        int value2 = Convert.ToInt32(count.ToString(), 16);

        int result = value1 ^ value2;

        return result.ToString("X");
    }

}
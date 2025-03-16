using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot.SS.AI.EncryptionLib.Helpers
{
    public class DSESHelper
    {
        private static readonly int[] InitialPermutation = {
        58, 50, 42, 34, 26, 18, 10, 2,
        60, 52, 44, 36, 28, 20, 12, 4,
        62, 54, 46, 38, 30, 22, 14, 6,
        64, 56, 48, 40, 32, 24, 16, 8,
        57, 49, 41, 33, 25, 17, 9, 1,
        59, 51, 43, 35, 27, 19, 11, 3,
        61, 53, 45, 37, 29, 21, 13, 5,
        63, 55, 47, 39, 31, 23, 15, 7
    };

        private static readonly int[] FinalPermutation = {
        40, 8, 48, 16, 56, 24, 64, 32,
        39, 7, 47, 15, 55, 23, 63, 31,
        38, 6, 46, 14, 54, 22, 62, 30,
        37, 5, 45, 13, 53, 21, 61, 29,
        36, 4, 44, 12, 52, 20, 60, 28,
        35, 3, 43, 11, 51, 19, 59, 27,
        34, 2, 42, 10, 50, 18, 58, 26,
        33, 1, 41, 9, 49, 17, 57, 25
    };

        public static byte[] Encrypt(string plaintext, string key)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            return DESCore(inputBytes, keyBytes, true);
        }

        public static byte[] Decrypt(byte[] ciphertext, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            return DESCore(ciphertext, keyBytes, false);
        }

        private static byte[] DESCore(byte[] data, byte[] key, bool encrypt)
        {
            byte[] permutedData = Permute(data, InitialPermutation);

            byte[] left = new byte[4];
            byte[] right = new byte[4];

            Array.Copy(permutedData, 0, left, 0, 4);
            Array.Copy(permutedData, 4, right, 0, 4);

            for (int i = 0; i < 16; i++)
            {
                byte[] newRight = Feistel(right, key);

                for (int j = 0; j < 4; j++)
                    newRight[j] ^= left[j];

                left = right;
                right = newRight;
            }

            byte[] combined = new byte[8];
            Array.Copy(right, 0, combined, 0, 4);
            Array.Copy(left, 0, combined, 4, 4);

            return Permute(combined, FinalPermutation);
        }

        private static byte[] Permute(byte[] data, int[] permutationTable)
        {
            byte[] permuted = new byte[data.Length];
            for (int i = 0; i < permutationTable.Length; i++)
            {
                int bit = (permutationTable[i] - 1) % (data.Length * 8);
                int byteIndex = bit / 8;
                int bitIndex = bit % 8;

                if ((data[byteIndex] & (1 << (7 - bitIndex))) != 0)
                    permuted[i / 8] |= (byte)(1 << (7 - (i % 8)));
            }
            return permuted;
        }

        private static byte[] Feistel(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }

            return result;
        }
    }
}

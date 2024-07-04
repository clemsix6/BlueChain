using System.Numerics;
using System.Security.Cryptography;


namespace BlueChainClient.Cryptography;


public static class Base58Check
{
    private const int CheckSumSizeInBytes = 4;
    private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";


    private static byte[] CalculateCheckSum(byte[] data)
    {
        var hash1 = SHA256.HashData(data);
        var hash2 = SHA256.HashData(hash1);
        return hash2.Take(CheckSumSizeInBytes).ToArray();
    }


    public static string Encode(byte[] data)
    {
        var checkSum = CalculateCheckSum(data);
        var dataWithCheckSum = data.Concat(checkSum).ToArray();
        var intData = new BigInteger(dataWithCheckSum.Reverse().Concat(new byte[] { 0 }).ToArray());

        var result = "";
        while (intData > 0) {
            var remainder = (int)(intData % 58);
            intData /= 58;
            result = Digits[remainder] + result;
        }

        for (var i = 0; i < dataWithCheckSum.Length && dataWithCheckSum[i] == 0; i++)
            result = '1' + result;

        return result;
    }


    public static byte[] Decode(string base58)
    {
        BigInteger intData = 0;
        for (var i = 0; i < base58.Length; i++) {
            var digit = Digits.IndexOf(base58[i]);
            if (digit < 0) {
                throw new FormatException($"Invalid Base58 character `{base58[i]}` at position {i}");
            }

            intData = intData * 58 + digit;
        }

        var bytesWithCheckSum = intData.ToByteArray().Reverse().SkipWhile(b => b == 0).ToArray();

        if (base58.StartsWith("1") && bytesWithCheckSum.Length < base58.Length)
            bytesWithCheckSum = new byte[base58.Length - bytesWithCheckSum.Length].Concat(bytesWithCheckSum).ToArray();

        var data = bytesWithCheckSum.Take(bytesWithCheckSum.Length - CheckSumSizeInBytes).ToArray();
        var providedCheckSum = bytesWithCheckSum.Skip(bytesWithCheckSum.Length - CheckSumSizeInBytes).ToArray();

        var actualCheckSum = CalculateCheckSum(data);
        if (!providedCheckSum.SequenceEqual(actualCheckSum))
            throw new FormatException("Invalid checksum");

        return data;
    }
}
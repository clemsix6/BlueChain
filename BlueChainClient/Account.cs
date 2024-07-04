using System.Security.Cryptography;
using System.Text;
using BlueChainClient.Cryptography;
using NSec.Cryptography;


namespace BlueChainClient;


public class Account
{
    private readonly Key ed25519Key;
    private readonly RSA rsa;


    private Account()
    {
        var creationParameters = new KeyCreationParameters {
            ExportPolicy = KeyExportPolicies.AllowPlaintextExport
        };

        this.ed25519Key = Key.Create(SignatureAlgorithm.Ed25519, creationParameters);
        this.rsa = RSA.Create(4096);
    }


    private Account(string eccPrivate, string rsaPrivate)
    {
        var importParameters = new KeyCreationParameters
        {
            ExportPolicy = KeyExportPolicies.AllowPlaintextExport
        };

        try
        {
            this.ed25519Key = Key.Import(
                SignatureAlgorithm.Ed25519,
                Base58Check.Decode(eccPrivate),
                KeyBlobFormat.RawPrivateKey,
                importParameters
            );
        }
        catch (FormatException ex)
        {
            throw new FormatException("The ECC private key is not in the correct format.", ex);
        }

        this.rsa = RSA.Create();
        try
        {
            this.rsa.ImportRSAPrivateKey(Base58Check.Decode(rsaPrivate), out _);
        }
        catch (CryptographicException ex)
        {
            throw new FormatException("The RSA private key is not in the correct format.", ex);
        }
    }


    public static Account Create()
    {
        const string keyPath = "account.key";

        if (!File.Exists(keyPath))
        {
            var account = new Account();
            var eccPrivate = account.GetEccPrivateKey();
            var rsaPrivate = account.GetRsaPrivateKey();

            File.WriteAllText(keyPath, $"{eccPrivate}\n{rsaPrivate}");
            return account;
        }

        var lines = File.ReadAllLines(keyPath);
        if (lines.Length != 2)
        {
            throw new FormatException("The key file does not contain the correct number of lines.");
        }

        return new Account(lines[0], lines[1]);
    }


    public (byte[] encryptedData, byte[] encryptedKey) Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();

        var encryptedData = memoryStream.ToArray();
        var aesKeyWithIv = aes.Key.Concat(aes.IV).ToArray();
        var encryptedKey = rsa.Encrypt(aesKeyWithIv, RSAEncryptionPadding.OaepSHA256);

        return (encryptedData, encryptedKey);
    }


    public byte[] Decrypt(byte[] encryptedData, byte[] encryptedKey)
    {
        var aesKeyWithIv = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
        var aesKey = aesKeyWithIv.Take(32).ToArray();
        var aesIv = aesKeyWithIv.Skip(32).ToArray();

        using var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream(encryptedData);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        var decryptedData = new byte[encryptedData.Length];
        var bytesRead = cryptoStream.Read(decryptedData, 0, decryptedData.Length);

        return decryptedData.Take(bytesRead).ToArray();
    }


    public string Sign(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var signatureBytes = SignatureAlgorithm.Ed25519.Sign(ed25519Key, messageBytes);
        return Base58Check.Encode(signatureBytes);
    }


    public bool Verify(string message, string signature)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var signatureBytes = Base58Check.Decode(signature);
        return SignatureAlgorithm.Ed25519.Verify(ed25519Key.PublicKey, messageBytes, signatureBytes);
    }


    public string GetEccPublicKey()
    {
        return Base58Check.Encode(ed25519Key.PublicKey.Export(KeyBlobFormat.RawPublicKey));
    }


    private string GetEccPrivateKey()
    {
        return Base58Check.Encode(ed25519Key.Export(KeyBlobFormat.RawPrivateKey));
    }


    public string GetRsaPublicKey()
    {
        return Base58Check.Encode(rsa.ExportRSAPublicKey());
    }


    private string GetRsaPrivateKey()
    {
        return Base58Check.Encode(rsa.ExportRSAPrivateKey());
    }
}
using System.Security.Cryptography;
using System.Text;
using BlueChainClient.Cryptography;
using BlueChainShared;
using BlueChainShared.Requests;
using BlueProtocol.Network.Sockets.Clients;


namespace BlueChainClient;


public class Client
{
    private readonly AsyncClient remoteNode;
    private readonly Account account;


    public Client(Account account)
    {
        this.remoteNode = new AsyncClient();
        this.account = account;
    }


    public void Connect(string host, int port)
    {
        var clientAuthRequest = new ClientAuthenticationRequest {
            PublicKey = this.account.GetEccPublicKey()
        };

        this.remoteNode.Connect(host, port);
        this.remoteNode.Start();
        this.remoteNode.Send(clientAuthRequest);

        var result = clientAuthRequest.WaitResult();
        if (!result.Authorized) {
            throw new UnauthorizedAccessException(result.Message);
        }
    }


    private string GetFileId(string filename)
    {
        var publicKey = this.account.GetEccPublicKey();
        var rawId = $"{publicKey}:{filename}";
        var fileHash = SHA256.HashData(Encoding.UTF8.GetBytes(rawId));
        return Base58Check.Encode(fileHash);
    }


    private void SendFileBlock(string fileId, int blockCount, int blockId, byte[] data)
    {
        var (encryptedData, encryptedKey) = this.account.Encrypt(data);
        var keyString = Base58Check.Encode(encryptedKey);
        var dataString = Convert.ToBase64String(encryptedData);

        var fileBlock = new FileBlock {
            FileId = fileId,
            BlockCount = blockCount,
            BlockId = blockId,
            Key = keyString,
            Data = dataString
        };
        var uploadEvent = new UploadEvent {
            FileBlock = fileBlock
        };
        this.remoteNode.Send(uploadEvent);
    }


    public string SendFile(string filename, Action<int, int>? progressCallback = null)
    {
        var streamReader = new StreamReader(filename);

        var fileId = GetFileId(filename);
        var fileBlockCount = (int)Math.Ceiling(streamReader.BaseStream.Length / 1024.0 / 1024.0);
        var fileBlockId = 0;
        var buffer = new byte[1024 * 1024];
        int readSize;
        while ((readSize = streamReader.BaseStream.Read(buffer, 0, buffer.Length)) > 0) {
            var block = new byte[readSize];
            Array.Copy(buffer, block, readSize);
            SendFileBlock(fileId, fileBlockCount, fileBlockId++, block);
            progressCallback?.Invoke(fileBlockId, fileBlockCount);
        }

        return fileId;
    }


    private int DownloadBlock(string fileId, int blockId, FileStream fileStream)
    {
        var downloadRequest = new DownloadRequest {
            FileId = fileId,
            BlockId = blockId
        };

        this.remoteNode.Send(downloadRequest);
        var response = downloadRequest.WaitResult();
        if (response.FileBlock == null)
            throw new FileNotFoundException("File block not found.");

        var encryptedKey = Base58Check.Decode(response.FileBlock.Key);
        var encryptedData = Convert.FromBase64String(response.FileBlock.Data);
        var decryptedData = this.account.Decrypt(encryptedData, encryptedKey);
        fileStream.Write(decryptedData, 0, decryptedData.Length);

        return response.FileBlock.BlockCount;
    }


    public void DownloadFile(string fileId, string filename, Action<int, int>? progressCallback = null)
    {
        var fileStream = new FileStream(filename, FileMode.Create | FileMode.Append);
        var blockCount = 1;

        for (var i = 0; i < blockCount; i++) {
            blockCount = DownloadBlock(fileId, i, fileStream);
            progressCallback?.Invoke(i, blockCount);
        }
    }
}
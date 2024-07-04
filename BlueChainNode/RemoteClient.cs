using BlueChainShared.Requests;
using BlueProtocol.Controllers;
using BlueProtocol.Network.Sockets.Clients;
using Newtonsoft.Json;


namespace BlueChainNode;


public class RemoteClient : Controller
{
    public AsyncClient Client { get; }
    public string PublicKey { get; }


    public RemoteClient(AsyncClient client, string publicKey)
    {
        this.Client = client;
        this.PublicKey = publicKey;

        this.Client.RequestHandler.RegisterController(this);
    }


    [Route]
    public void OnFileBlockReceived(UploadEvent uploadEvent)
    {
        const string storagePath = "storage";

        if (!Directory.Exists(storagePath))
            Directory.CreateDirectory(storagePath);

        var fileName = uploadEvent.FileBlock.FileId + "_" + uploadEvent.FileBlock.BlockId;
        var serial = JsonConvert.SerializeObject(uploadEvent);
        File.WriteAllText(Path.Combine(storagePath, fileName), serial);

        Console.WriteLine($"[!] File block received: {fileName}");
    }


    [Route]
    public DownloadResponse OnDownloadRequest(DownloadRequest downloadRequest)
    {
        const string storagePath = "storage";

        var fileName = downloadRequest.FileId + "_" + downloadRequest.BlockId;
        var filePath = Path.Combine(storagePath, fileName);

        Console.WriteLine($"[!] Download request: {fileName}");
        if (!File.Exists(filePath))
            return new DownloadResponse { FileBlock = null };
        Console.WriteLine($"[!] File block found: {fileName}");

        var serial = File.ReadAllText(filePath);
        var uploadEvent = JsonConvert.DeserializeObject<UploadEvent>(serial);
        Console.WriteLine($"[!] File block sent: {fileName}");

        return new DownloadResponse { FileBlock = uploadEvent?.FileBlock ?? null };
    }
}
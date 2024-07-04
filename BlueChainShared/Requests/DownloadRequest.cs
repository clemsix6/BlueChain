using BlueProtocol.Network.Communication.Requests;


namespace BlueChainShared.Requests;


public class DownloadRequest : Request<DownloadResponse>
{
    public required string FileId { get; init; }
    public required int BlockId { get; init; }
}


public class DownloadResponse : Response
{
    public required FileBlock? FileBlock { get; init; }
}
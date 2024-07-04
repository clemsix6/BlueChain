namespace BlueChainShared;


public class FileBlock
{
    public required string FileId { get; init; }
    public required int BlockCount { get; init; }
    public required int BlockId { get; init; }
    public required string Key { get; init; }
    public required string Data { get; init; }
}
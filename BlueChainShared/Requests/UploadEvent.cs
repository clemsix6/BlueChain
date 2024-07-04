using BlueProtocol.Network.Communication.Events;


namespace BlueChainShared.Requests;


public class UploadEvent : Event
{
    public required FileBlock FileBlock { get; init; }
}
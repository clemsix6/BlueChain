using BlueProtocol.Network.Communication.Requests;


namespace BlueChainShared.Requests;


public class ClientAuthenticationRequest : Request<ClientAuthenticationResponse>
{
    public required string PublicKey { get; init; }
}


public class ClientAuthenticationResponse : Response
{
    public required bool Authorized { get; init; }
    public new string Message { get; init; } = string.Empty;
}
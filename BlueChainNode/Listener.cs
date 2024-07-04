using System.Net;
using BlueChainShared.Requests;
using BlueProtocol.Controllers;
using BlueProtocol.Network.Communication.System;
using BlueProtocol.Network.Sockets.Clients;
using BlueProtocol.Network.Sockets.Servers;


namespace BlueChainNode;


public class Listener : Controller
{
    private readonly BlueServer<AsyncClient> server;

    private readonly List<AsyncClient> pairs = [];
    private readonly List<RemoteClient> clients = [];


    public Listener()
    {
        this.server = new BlueServer<AsyncClient>();
    }


    public Listener(int port)
    {
        this.server = new BlueServer<AsyncClient>(port);
    }


    public IPEndPoint LocalEndPoint => this.server.LocalEndPoint;


    public void Start()
    {
        this.server.RequestHandler.RegisterController(this);
        this.server.Start();
        this.server.OnClientConnectedEvent += this.OnClientConnected;
    }


    private void OnClientConnected(AsyncClient client)
    {
        lock (this.pairs) {
            if (this.pairs.Count >= 10) {
                client.Close(CloseReason.Custom("Too many connections"));
                return;
            }

            if (this.pairs.Any(x => x.RemoteEndPoint.Equals(client.RemoteEndPoint))) {
                client.Close(CloseReason.Custom("Already connected"));
                return;
            }

            client.OnDisconnectedEvent += (_, _) => {
                Console.WriteLine($"[-] {client.RemoteEndPoint}");

                lock (this.pairs) {
                    this.pairs.Remove(client);
                }
            };

            Console.WriteLine($"[+] {client.RemoteEndPoint}");
            this.pairs.Add(client);
        }
    }


    [Route]
    public ClientAuthenticationResponse OnClientAuthentication(AsyncClient client, ClientAuthenticationRequest request)
    {
        lock (this.clients) {
            if (this.clients.Any(x => x.PublicKey == request.PublicKey)) {
                Task.Run(() => {
                    Thread.Sleep(2000);
                    client.Close(CloseReason.Custom("Already authenticated"));
                });

                return new ClientAuthenticationResponse { Authorized = false, Message = "Already authenticated" };
            }

            client.OnDisconnectedEvent += (_, _) => {
                lock (this.clients) {
                    this.clients.RemoveAll(x => x.PublicKey == request.PublicKey);
                }
            };

            Console.WriteLine($"[!] {client.RemoteEndPoint} -> {request.PublicKey}");
            this.clients.Add(new RemoteClient(client, request.PublicKey));
            return new ClientAuthenticationResponse { Authorized = true };
        }
    }
}
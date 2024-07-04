using System.Security.Cryptography;
using BlueChainClient;
using BlueChainClient.Cryptography;
using BlueChainClientCLI;
using Spectre.Console;
using Spectre.Console.Cli;


internal static class Program
{
    private static readonly Account Account = Account.Create();
    private static readonly Client Client = new(Account);


    private static void Main()
    {
        PrintTitle();
        PrintPublicKey();
        RunApp();
    }


    private static CommandApp ConfigureApp()
    {
        var app = new CommandApp();
        app.Configure(config => {
            config.AddCommand<ConnectCommand>("connect")
                .WithDescription("Connect to a server")
                .WithExample(["connect", "--port", "1234"])
                .WithData(Client);
            config.AddCommand<UploadFileCommand>("upload")
                .WithDescription("Upload a file to the node")
                .WithExample(["upload", "--filename", "file.txt"])
                .WithData(Client);
            config.AddCommand<DownloadFileCommand>("download")
                .WithDescription("Download a file from the node")
                .WithExample(["download", "--fileid", "4ka...vFu", "--filename", "file.txt"])
                .WithData(Client);
        });

        return app;
    }


    private static void PrintTitle()
    {
        const string title = "CLIENT";

        Console.WriteLine();
        AnsiConsole.Write(
            new FigletText(title)
                .Centered()
                .Color(Color.Blue)
        );
        Console.WriteLine();
    }


    private static void PrintPublicKey()
    {
        var eccPublicKey = Account.GetEccPublicKey();
        var rsaPublicKey = Account.GetRsaPublicKey();

        var panel = new Panel($"[bold]" +
                              $"ECC: {eccPublicKey}\n" +
                              $"RSA: {rsaPublicKey}" +
                              $"[/]") {
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Blue),
            Header = new PanelHeader("[bold] Public Key [/]"),
        };
        AnsiConsole.Write(panel);
        Console.WriteLine("\n");
    }


    private static void RunApp()
    {
        var app = ConfigureApp();

        while (true) {
            Console.Write("\n> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            var splitInput = input.Split(' ');
            app.Run(splitInput);
        }
    }
}
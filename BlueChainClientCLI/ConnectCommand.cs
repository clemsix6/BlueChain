using System.ComponentModel;
using BlueChainClient;
using Spectre.Console;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;


namespace BlueChainClientCLI;


public class ConnectCommand : Command<ConnectCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--port <PORT>")]
        [Description("Port number to connect to")]
        public int Port { get; set; } = -42;
    }


    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (settings.Port == -42)
            return ValidationResult.Error("Port is required");
        if (settings.Port < 0 || settings.Port > 65535)
            return ValidationResult.Error("Port must be between 0 and 65535");

        return ValidationResult.Success();
    }


    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine($"[bold]Connecting to server on port {settings.Port}...[/]");

        try {
            if (context.Data is not Client client)
                throw new InvalidCastException("Client instance not found in context data");
            client.Connect("127.0.0.1", settings.Port);
        } catch (UnauthorizedAccessException e) {
            AnsiConsole.MarkupLine($"[red]Error: {e.Message}[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[bold]Successfully connected to server on port {settings.Port}[/]");

        return 0;
    }
}
using System.ComponentModel;
using BlueChainClient;
using Spectre.Console;
using Spectre.Console.Cli;


namespace BlueChainClientCLI;


public class UploadFileCommand : Command<UploadFileCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--filename <FILE_NAME>")]
        [Description("Name / path of the file to send")]
        public string FileName { get; set; } = string.Empty;
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        var fileName = settings.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
            return ValidationResult.Error("File name is required");
        if (!File.Exists(fileName))
            return ValidationResult.Error("File not found");

        return ValidationResult.Success();
    }


    public override int Execute(CommandContext context, Settings settings)
    {
        var fileName = settings.FileName;
        if (context.Data is not Client client)
            throw new InvalidCastException("Client instance not found in context data");

        AnsiConsole.MarkupLine($"[bold]Uploading file {fileName}...[/]");

        var fileId = client.SendFile(fileName, (blockId, blockCount) => {
            AnsiConsole.MarkupLine($"[bold]Block {blockId}/{blockCount}...[/]");
        });

        AnsiConsole.MarkupLine($"[bold]File uploaded successfully with ID {fileId}[/]");

        return 0;
    }
}
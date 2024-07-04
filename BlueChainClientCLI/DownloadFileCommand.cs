using System.ComponentModel;
using System.Security.Cryptography;
using BlueChainClient;
using Spectre.Console;
using Spectre.Console.Cli;


namespace BlueChainClientCLI;


public class DownloadFileCommand : Command<DownloadFileCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--filename <FILE_NAME>")]
        [Description("Name / path of the file to send")]
        public string FileName { get; set; } = string.Empty;

        [CommandOption("--fileid <FILE_ID>")]
        [Description("ID of the file to download")]
        public string FileId { get; set; } = string.Empty;
    }


    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        var fileName = settings.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
            return ValidationResult.Error("File name is required");

        var fileId = settings.FileId;
        if (string.IsNullOrWhiteSpace(fileId))
            return ValidationResult.Error("File ID is required");

        return ValidationResult.Success();
    }


    public override int Execute(CommandContext context, Settings settings)
    {
        var fileName = settings.FileName;
        if (context.Data is not Client client)
            throw new InvalidCastException("Client instance not found in context data");

        AnsiConsole.MarkupLine($"[bold]Downloading file {fileName}...[/]");

        try {
            client.DownloadFile(settings.FileId, fileName,
                (blockId, blockCount) => { AnsiConsole.MarkupLine($"[bold]Block {blockId}/{blockCount}...[/]"); });
        } catch (CryptographicException) {
            AnsiConsole.MarkupLine("[bold red]You cannot decrypt this file[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[bold]File downloaded successfully to {fileName}[/]");

        return 0;
    }
}
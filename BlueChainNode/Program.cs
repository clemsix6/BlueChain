using BlueChainNode;
using Spectre.Console;


internal static class Program
{
    private static readonly Listener Listener = new(55678);


    private static void Main()
    {
        PrintTitle();
        Listener.Start();
        PrintPort();

        Console.ReadLine();
    }


    private static void PrintTitle()
    {
        const string title = "NODE";

        Console.WriteLine();
        AnsiConsole.Write(
            new FigletText(title)
                .Centered()
                .Color(Color.Yellow)
        );
        Console.WriteLine();
    }


    private static void PrintPort()
    {
        var port = Listener.LocalEndPoint.Port;

        var panel = new Panel($"[bold]   {port}   [/]") {
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Yellow),
            Header = new PanelHeader("[bold] Port [/]"),
        };
        AnsiConsole.Write(panel);
        Console.WriteLine("\n");
    }
}

public enum PageTypes
{
    None = 0,
    Menu,
    CreateFile
}

class Program
{
    public static PageTypes CurrentPage = PageTypes.None;

    // Key: command, Value: Command handler.
    private static Dictionary<string, CommandHandler>? _menuCommands;

    static void Main(string[] args)
    {
        Console.Title = "File Refactoring App";
        string input;
        bool exit = false;
        InitCommands();

        Console.WriteLine("A nice tool for renaming, editing & creating files easily.\n");
        PrintHelp();
        CurrentPage = PageTypes.Menu;

        do
        {
            input = Console.ReadLine();
            HandleInput(input);
            Thread.Sleep(500);
        } 
        while (!exit);
    }

    private static void HandleInput(string commandInput)
    {
        var commandMap = GetPageCommands();
        if (commandMap.ContainsKey(commandInput)) {
            commandMap[commandInput](commandInput);
        } else
        {
            WriteLineHighlight($"Command: '{commandInput}' did not exist in the command map for page: {CurrentPage}.");
        }
    }

    public delegate void CommandHandler(string input);

    private static void PrintHelp()
    {
        WriteLineHighlight("> Stop app: 'quit'");
        WriteLineHighlight("> Restart app: 'restart'");
        WriteLineHighlight("> Clear the console: 'clear'");
        WriteLineHighlight("> Help: 'help'");
        WriteLineHighlight("> Create a file: 'cfile'");
        WriteLineHighlight("> Template Info: 'help-template'");
        Console.WriteLine("\n");
    }

    private static void WriteLineHighlight(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void InitCommands()
    {
        _menuCommands = new Dictionary<string, CommandHandler>()
        {
            { "cfile", CreateFile }
        };
    }

    private static Dictionary<string, CommandHandler> GetPageCommands()
    {
        var ret = new Dictionary<string, CommandHandler>();
        switch(CurrentPage) {
            case PageTypes.Menu:
                ret = _menuCommands;
                break;

            default:
                WriteLineHighlight($"Could not find commands for page: {CurrentPage}.");
                break;
        }

        return ret;
    }

    // Commands
    private static void CreateFile(string input)
    {
        Console.WriteLine("Welcome to the menu for creating a file!");
    }
}
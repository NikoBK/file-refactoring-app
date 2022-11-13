
public enum PageTypes
{
    None = 0,
    Menu,
    CreateFile,
    RemoveNCharsFromNFiles
}

class Program
{
    private static bool _exit = false;
    public static PageTypes CurrentPage = PageTypes.None;
    private static string _subCmdCommandKey = "subCmd";

    // Most pages have steps for processes.
    private static int _currentPageStep = 1;

    // SubCmd means whether we are writing a value using a command within a command or not.
    private static bool _writingSubCmd = false;

    // Key: command, Value: Command handler.
    private static Dictionary<string, CommandHandler>? _menuCommands;
    private static Dictionary<string, CommandHandler>? _commonCommands; // Can be used on any page.
    private static Dictionary<string, CommandHandler>? _createFileCommands;
    private static Dictionary<string, CommandHandler>? _multiRefactorFiles_RemoveCharsCommands;

    // Properties
    public static string RootPath { get; private set; } = string.Empty;
    public static string Template { get; private set; } = string.Empty;
    public static string CreateFileExtension { get; private set; } = string.Empty;

    static void Main(string[] args)
    {
        Console.Title = "File Refactoring App";
        string input;
        InitCommands();

        Console.WriteLine("A nice tool for renaming, editing & creating files easily.\n");
        PrintHelp();
        CurrentPage = PageTypes.Menu;

        do
        {
            // The cursor is "Enter command: " in front of your typing.
            bool showCursor = _writingSubCmd ? false : true;
            if (showCursor) { 
                Console.Write("Enter command: "); 
            }

            input = Console.ReadLine();
            HandleInput(input);
            Thread.Sleep(250);
        } 
        while (!_exit);
    }

    private static void HandleInput(string commandInput)
    {
        // If we are handling a common command, just handle it right away.
        if (_commonCommands.ContainsKey(commandInput)) {
            _commonCommands[commandInput](commandInput);
            return;
        }

        // If the command input is not a common command.
        var commandMap = GetPageCommands();
        string key = _writingSubCmd ? "subCmd" : commandInput;
        if (commandMap.ContainsKey(key)) {
            commandMap[key](commandInput);
        } else
        {
            WriteLineHighlight($"Command: '{commandInput}' did not exist in the command map for page: {CurrentPage}.");
        }
    }

    public delegate void CommandHandler(string input);

    private static void WriteLineHighlight(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void InitCommands()
    {
        _commonCommands = new Dictionary<string, CommandHandler>()
        {
            { "quit", Quit },
            { "restart", Restart },
            { "clear", Clear },
            { "help", Help },
            { "commands", Commands }
        };

        _menuCommands = new Dictionary<string, CommandHandler>()
        {
            { "cfile", CreateFile }
        };

        _createFileCommands = new Dictionary<string, CommandHandler>()
        {
            { _subCmdCommandKey, CreateFile }
        };

        _multiRefactorFiles_RemoveCharsCommands = new Dictionary<string, CommandHandler>()
        {
            { _subCmdCommandKey, RemoveNCharsFromNFileNames }
        };
    }

    private static Dictionary<string, CommandHandler> GetPageCommands()
    {
        var ret = new Dictionary<string, CommandHandler>();
        switch(CurrentPage) {
            case PageTypes.Menu:
                ret = _menuCommands;
                break;

            case PageTypes.CreateFile:
                ret = _createFileCommands;
                break;

            default:
                WriteLineHighlight($"Could not find any commands for page: {CurrentPage}.");
                break;
        }

        return ret;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Help: \n");
        WriteLineHighlight("help - Prints all common commands.");
        WriteLineHighlight("quit - Stops the application.");
        WriteLineHighlight("clear - Clears the application from text.");
        WriteLineHighlight("restart - Restarts the application (TBD).");
        WriteLineHighlight("commands - Displays all available commands for the current page.");
        Console.WriteLine("\n");
    }

    private static string GetSubCmdValue(string input, string text, string completeMsg)
    {
        string ret = string.Empty;

        if (!_writingSubCmd)
        {
            Console.Write(text);
            _writingSubCmd = true;
        }
        else
        {
            ret = input;
            _writingSubCmd = false;
            _currentPageStep++;
            Console.WriteLine(completeMsg);
        }

        return ret;
    }

    // Commands
    private static void Quit(string input)
    {
        WriteLineHighlight("Quitting application...");
        _exit = true;
    }

    private static void Restart(string input)
    {
        // TODO: Implement something here..
        WriteLineHighlight("To be added...");
    }

    private static void Clear(string input)
    {
        Console.Clear();
    }

    private static void Help(string input)
    {
        PrintHelp();
    }

    private static void Commands(string input)
    {
        WriteLineHighlight($"~~~ Commands for page: {CurrentPage}\n");
        var commandMap = GetPageCommands();
        foreach (string command in commandMap.Keys) {
            if (command != _subCmdCommandKey) {
                Console.WriteLine($"{command}");
            }
        }
        WriteLineHighlight("\n~~~ End \n");
    }

    private static void CreateFile(string input)
    {
        if (CurrentPage != PageTypes.CreateFile) {
            CurrentPage = PageTypes.CreateFile;
        }

        if (!_writingSubCmd && _currentPageStep == 1) {
            Console.WriteLine("\n ~~~ Creating a file:");
            Console.WriteLine("Welcome to the menu for creating a file!");
        }
        
        switch(_currentPageStep)
        {
            case 1:
                var rootDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/";

                if (!string.IsNullOrEmpty(input)) {
                    RootPath = rootDesktopPath + GetSubCmdValue(input, $"Enter root path: {rootDesktopPath}", $"Root path set: {rootDesktopPath + input}\n");
                    CreateFile("");
                }
                break;

            case 2:
                if (!_writingSubCmd && _currentPageStep == 2) {
                    Console.WriteLine("If you want the file to use a template please enter the template path within the root directory. If not type nothing and hit enter.");
                    WriteLineHighlight("(help-template - Displays what a template is)\n");
                }
                
                Template = RootPath + GetSubCmdValue(input, $"Enter template file path: {RootPath}", $"Template path set: {RootPath + input}\n");

                if (!string.IsNullOrEmpty(input)) {                    
                    // Make sure the file actually exists before we move on.
                    if (File.Exists(Template)) {
                        WriteLineHighlight("File check: OK (File exists).\n");
                        CreateFile("");
                    }
                    else {
                        WriteLineHighlight($"File: {Template} could not be found! Process has been reset.");
                        ReturnToMenu("");
                    }
                }
                break;

            case 3:
                var extension = GetSubCmdValue(input, "Enter file extension: .", $"File extension set: .{input}");
                if (!string.IsNullOrEmpty(extension)) {
                    CreateFileExtension = extension;
                    CreateFile("");
                }
                break;

            case 4:

                break;
        }
    }

    private static void ReturnToMenu(string input)
    {
        // Reset the page step/process
        _currentPageStep = 1;
        CurrentPage = PageTypes.Menu;
        _writingSubCmd = false;
    }

    private static void RemoveNCharsFromNFileNames(string input)
    {
        // TODO: Implement something here.
    }
}
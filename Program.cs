﻿
public enum PageTypes
{
    None = 0,
    Menu,
    CreateFile
}

class Program
{
    private static bool _exit = false;
    public static PageTypes CurrentPage = PageTypes.None;

    // Key: command, Value: Command handler.
    private static Dictionary<string, CommandHandler>? _menuCommands;
    private static Dictionary<string, CommandHandler>? _commonCommands; // Can be used on any page.

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
            Console.Write("Enter command: ");
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
        _commonCommands = new Dictionary<string, CommandHandler>()
        {
            { "quit", Quit },
            { "restart", Restart },
            { "clear", Clear },
            { "help", Help }
        };

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

    private static void CreateFile(string input)
    {
        CurrentPage = PageTypes.CreateFile;
        Console.WriteLine("\n ~~~ Creating a file:");
        Console.WriteLine("Welcome to the menu for creating a file!");
    }
}
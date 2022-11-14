
public enum PageTypes
{
    None = 0,
    Menu,
    CreateFile,
    MultiRefactorFiles_RemoveChars,
    RemovePrefixFromName
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
    private static Dictionary<string, CommandHandler>? _removeStringFromNamesCommands;

    // Properties
    public static string DesktopPath { get; private set; } = string.Empty;
    public static string RootPath { get; private set; } = string.Empty;
    public static string Template { get; private set; } = string.Empty;
    public static string FileExtension { get; private set; } = string.Empty;
    public static string FullRefactorDirectoryPath { get; private set; } = string.Empty;
    public static string[]? DirectoryFiles { get; private set; }
    public static string DirectoryName { get; private set; } = string.Empty;
    public static int RefactorStartIndex { get; private set; } = 0;
    public static int RefactorTrimLength { get; private set; } = 1;
    public static string? RemoveString { get; private set; }

    static void Main(string[] args)
    {
        Console.Title = "File Refactoring App";
        string input;
        InitCommands();

        // Set properties.
        DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/";

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

    /// <summary>
    /// Handle input by checking if the <see cref="CurrentPage"/> has a valid command
    /// within it's respective commandmap.
    /// </summary>
    /// <param name="commandInput"></param>
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

    /// <summary>
    /// Writes a line in the console, with yellow foreground color.
    /// </summary>
    /// <param name="text"></param>
    private static void WriteLineHighlight(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
    }

    /// <summary>
    /// Initialize all prefined available commands per page.
    /// </summary>
    private static void InitCommands()
    {
        _commonCommands = new Dictionary<string, CommandHandler>() {
            { "quit", Quit },
            { "restart", Restart },
            { "clear", Clear },
            { "help", Help },
            { "commands", Commands },
            { "return", ReturnToMenu },
            { "reset", ResetProperties }
        };

        _menuCommands = new Dictionary<string, CommandHandler>() {
            { "cfile", CreateFile },
            { "refnfiles", RemoveNCharsFromNFileNames },
            { "remnameprefixes", RemovePrefixFromNames }
        };

        _createFileCommands = new Dictionary<string, CommandHandler>() {
            { _subCmdCommandKey, CreateFile }
        };

        _multiRefactorFiles_RemoveCharsCommands = new Dictionary<string, CommandHandler>() {
            { _subCmdCommandKey, RemoveNCharsFromNFileNames }
        };

        _removeStringFromNamesCommands = new Dictionary<string, CommandHandler>() {
            { _subCmdCommandKey, RemovePrefixFromNames }
        };
    }

    /// <summary>
    /// Returns the dictionary for the application's current page.
    /// </summary>
    /// <returns></returns>
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

            case PageTypes.MultiRefactorFiles_RemoveChars:
                ret = _multiRefactorFiles_RemoveCharsCommands;
                break;

            case PageTypes.RemovePrefixFromName:
                ret = _removeStringFromNamesCommands;
                break;

            default:
                WriteLineHighlight($"Could not find any commands for page: {CurrentPage}.");
                break;
        }

        return ret;
    }

    /// <summary>
    /// Writes all the common commands, and a few more helpful tips.
    /// </summary>
    private static void PrintHelp()
    {
        Console.WriteLine("Help: \n");
        WriteLineHighlight("help - Prints all common commands.");
        WriteLineHighlight("quit - Stops the application.");
        WriteLineHighlight("clear - Clears the application from text.");
        WriteLineHighlight("restart - Restarts the application (TBD).");
        WriteLineHighlight("commands - Displays all available commands for the current page.");
        WriteLineHighlight("return - Returns to menu.");
        WriteLineHighlight("reset - Resets application properties (return does this for you too, unless you are using return on the menu).");
        Console.WriteLine("\n");
    }

    /// <summary>
    /// Returns input as further attribution to previous input.
    /// Example: Given a file path, further input is needed to specify the file
    /// extension that the file path is pointing too.
    /// <para>This function should only ever be called by a <see cref="CommandHandler"/> during a page process.</para>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="text"></param>
    /// <param name="completeMsg"></param>
    /// <returns></returns>
    private static string GetSubCmdValue(string input, string text, string completeMsg)
    {
        string ret = string.Empty;

        // If we are not expecting a sub command yet,
        // it means we are about to, and as thus we write
        // the expected value and set _writingSubCmd -> true.
        if (!_writingSubCmd) {
            Console.Write(text);
            _writingSubCmd = true;
        }
        // If we do expect a sub command
        // we return the input we wrote during this
        // (the input being the sub command) and increase the page step
        // representing how far along the process we are. A sucessor message
        // is written to let the user know, all went well.
        else {
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

    /// <summary>
    /// A 5 step process of creating a file, with or without a code/script template.
    /// </summary>
    /// <param name="input"></param>
    private static void CreateFile(string input)
    {
        if (CurrentPage != PageTypes.CreateFile) {
            CurrentPage = PageTypes.CreateFile;
        }

        if (!_writingSubCmd && _currentPageStep == 1) {
            Console.WriteLine("\n ~~~ Menu: Creating a file:");
            Console.WriteLine("Welcome to the menu for creating a file!");
        }
        
        switch(_currentPageStep)
        {
            case 1:
                if (!string.IsNullOrEmpty(input)) {
                    RootPath = DesktopPath + GetSubCmdValue(input, $"Enter root path: {DesktopPath}", $"Root path set: {DesktopPath + input}\n");
                    CreateFile("");
                }
                break;

            case 2:
                if (!_writingSubCmd) {
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
                var extension = GetSubCmdValue(input, "Enter file extension: .", $"File extension set: .{input}\n");
                if (!string.IsNullOrEmpty(extension)) {
                    FileExtension = extension;
                    CreateFile("");
                }
                break;

            case 4:
                // TODO: Remove this when code for this page step is added.
                ReturnToMenu("");
                break;
        }
    }

    private static void ReturnToMenu(string input)
    {
        if (CurrentPage == PageTypes.Menu) {
            WriteLineHighlight("You are already at the menu.");
            return;
        }

        // Reset the page step/process
        _currentPageStep = 1;
        CurrentPage = PageTypes.Menu;
        _writingSubCmd = false;
        FileExtension = string.Empty;
        DirectoryFiles = null;

        WriteLineHighlight("Returned to menu.");
    }

    /// <summary>
    /// A function that refactors every file within a directory (folder),
    /// removing n chars from every file's name. The offset and length of chars
    /// to be removed from file names can be adjusted dynamically.
    /// </summary>
    /// <param name="input"></param>
    private static void RemoveNCharsFromNFileNames(string input)
    {
        if (CurrentPage != PageTypes.MultiRefactorFiles_RemoveChars) {
            CurrentPage = PageTypes.MultiRefactorFiles_RemoveChars;
        }

        if (!_writingSubCmd && _currentPageStep == 1) {
            Console.WriteLine("\n ~~~ Menu: Removing multiple characters from multiple file names.");
        }

        switch(_currentPageStep)
        {
            case 1:
                if (!string.IsNullOrEmpty(input)) {
                    RootPath = DesktopPath + GetSubCmdValue(input, $"Enter root path: {DesktopPath}", $"Root path set: {DesktopPath + input}\n");
                    RemoveNCharsFromNFileNames("");
                }
                break;

            case 2:
                var extension = GetSubCmdValue(input, "Enter file extension: .", $"File extension set: .{input}\n");
                if (!string.IsNullOrEmpty(extension)) {
                    FileExtension = extension;
                    RemoveNCharsFromNFileNames("");
                }
                break;

            case 3:
                FullRefactorDirectoryPath = RootPath + GetSubCmdValue(input, $"Files directory path: {RootPath}", $"Directory: {RootPath + input}");

                if (!string.IsNullOrEmpty(input)) 
                {
                    List<string> directories = new List<string>();
                    try {
                        directories = Directory.GetDirectories(FullRefactorDirectoryPath, "*", SearchOption.AllDirectories).ToList();
                    } catch (Exception ex) {
                        WriteLineHighlight($"Could not find directory: {FullRefactorDirectoryPath}");
                        ReturnToMenu("");
                        return;
                    }
                    directories.Add(FullRefactorDirectoryPath);
                    foreach (string entry in directories)
                    {
                        DirectoryName = entry.Replace($@"{FullRefactorDirectoryPath}", "").Replace("\\", "");
                        DirectoryFiles = Directory.GetFiles(entry, $"*.{FileExtension}");
                        Console.WriteLine($"Contains: {DirectoryFiles.Length} files.");                        
                    }
                    RemoveNCharsFromNFileNames("");
                }
                break;

            case 4:
                var offset = GetSubCmdValue(input, "Enter name refactor index(offset): ", $"Start index (offset): {input}\n");               
                if (!string.IsNullOrEmpty(offset)) {
                    RefactorStartIndex = int.Parse(offset);
                    RemoveNCharsFromNFileNames("");
                }
                break;

            case 5:
                var trimLen = GetSubCmdValue(input, "Name refactor trim length: ", $"Name refactor trim length set: {input}\n");               
                if (!string.IsNullOrEmpty(trimLen)) {
                    RefactorTrimLength = int.Parse(trimLen);
                    RemoveNCharsFromNFileNames("");
                }
                break;

            case 6:
                if (!_writingSubCmd) {
                    if (DirectoryFiles == null) {
                        WriteLineHighlight("Unable to resolve directory. Please try again.");
                        ReturnToMenu("");
                        return;
                    }
                    WriteLineHighlight($"You are about to refactor {DirectoryFiles.Length}, are you sure you want to continue?");
                    WriteLineHighlight("y or Y - Do the refactor.");
                    WriteLineHighlight("n or N - Cancel and return to menu.");
                }

                var answer = GetSubCmdValue(input, "Confirm: ", "");
                if (answer.ToLower() == "y") 
                {
                    if (DirectoryFiles == null) {
                        WriteLineHighlight("Directory files not found.");
                        ReturnToMenu("");
                        return;
                    }

                    WriteLineHighlight("Refactoring files...");
                    WriteLineHighlight("Please wait for the success prompt.");
                    foreach (var file in DirectoryFiles) {
                        var fileName = $"{(DirectoryName == "" ? "" : $"{DirectoryName}/")}{Path.GetFileName(file)}";
                        Console.WriteLine($"Refactoring: {fileName}");
                        var fileSrc = FullRefactorDirectoryPath;
                        string newFileName = fileName.Remove(RefactorStartIndex, RefactorTrimLength);
                        File.Move(fileSrc + fileName, fileSrc + newFileName);
                    }

                    WriteLineHighlight($"Succesfully refactored: {DirectoryFiles.Length} files!\n");
                }
                else if (answer.ToLower() == "n") {
                    ReturnToMenu("");
                    return;
                }

                // TODO: Remove this when another step to this process has been added.
                // ReturnToMenu("");
                break;
        }
    }

    private static void ResetProperties(string input)
    {
        // Reset the page step/process
        _currentPageStep = 1;
        _writingSubCmd = false;
        FileExtension = string.Empty;
        DirectoryFiles = null;

        WriteLineHighlight("Application properties has been reset!");
    }

    private static void RemovePrefixFromNames(string input)
    {
        if (CurrentPage != PageTypes.RemovePrefixFromName) {
            CurrentPage = PageTypes.RemovePrefixFromName;
        }

        if (!_writingSubCmd && _currentPageStep == 1) {
            Console.WriteLine("\n ~~~ Menu: Removing prefix from file names.");
        }

        switch(_currentPageStep)
        {
            case 1:                
                if (!string.IsNullOrEmpty(input)) {
                    RootPath = DesktopPath + GetSubCmdValue(input, $"Enter root path: {DesktopPath}", $"Root path set: {DesktopPath + input}\n");
                    RemovePrefixFromNames("");
                }
                break;

            case 2:
                var extension = GetSubCmdValue(input, "Enter file extension: .", $"File extension set: .{input}\n");
                if (!string.IsNullOrEmpty(extension)) {
                    FileExtension = extension;
                    RemovePrefixFromNames("");
                }
                break;

            case 3:
                FullRefactorDirectoryPath = RootPath + GetSubCmdValue(input, $"Files directory path: {RootPath}", $"Directory: {RootPath + input}");

                if (!string.IsNullOrEmpty(input)) 
                {
                    List<string> directories;
                    try {
                        directories = Directory.GetDirectories(FullRefactorDirectoryPath, "*", SearchOption.AllDirectories).ToList();
                    } catch (Exception ex) {
                        WriteLineHighlight($"Could not find directory: {FullRefactorDirectoryPath}");
                        ReturnToMenu("");
                        return;
                    }
                    directories.Add(FullRefactorDirectoryPath);
                    foreach (string entry in directories)
                    {
                        DirectoryName = entry.Replace($@"{FullRefactorDirectoryPath}", "").Replace("\\", "");
                        DirectoryFiles = Directory.GetFiles(entry, $"*.{FileExtension}");
                        Console.WriteLine($"Contains: {DirectoryFiles.Length} files.");                        
                    }
                    RemovePrefixFromNames("");
                }
                break;

            case 4:
                var removeString = GetSubCmdValue(input, "Enter name prefix to be removed: ", $"name prefix to be removed: {input}\n");               
                if (!string.IsNullOrEmpty(removeString)) {
                    RemoveString = removeString;
                    RemovePrefixFromNames("");
                }
                break;

            case 5:
                if (!_writingSubCmd) {
                    if (DirectoryFiles == null) {
                        WriteLineHighlight("Unable to resolve directory. Please try again.");
                        ReturnToMenu("");
                        return;
                    }
                    WriteLineHighlight($"You are about to refactor {DirectoryFiles.Length}, are you sure you want to continue?");
                    WriteLineHighlight("y or Y - Do the refactor.");
                    WriteLineHighlight("n or N - Cancel and return to menu.");
                }

                var answer = GetSubCmdValue(input, "Confirm: ", "");
                if (answer.ToLower() == "y") 
                {
                    if (DirectoryFiles == null) {
                        WriteLineHighlight("Directory files not found.");
                        ReturnToMenu("");
                        return;
                    }

                    WriteLineHighlight("Refactoring files...");
                    WriteLineHighlight("Please wait for the success prompt.");
                    foreach (var file in DirectoryFiles) {                       
                        var fileName = $"{(DirectoryName == "" ? "" : $"{DirectoryName}/")}{Path.GetFileName(file)}";
                        int removeStringStartPos = 0;
                        int removeStringEndPos = 1;

                        if (string.IsNullOrEmpty(FileExtension)) {
                            WriteLineHighlight("No file extension!");
                            ReturnToMenu("");
                            return;
                        }

                        bool containsRemoveString = fileName.Contains(RemoveString);
                        if (!containsRemoveString) {
                            WriteLineHighlight($"String: {RemoveString} not found in name: {fileName}.");
                            ReturnToMenu("");
                            return;
                        }

                        Console.WriteLine($"Refactoring: {fileName}...");
                        removeStringStartPos = fileName.IndexOf(RemoveString);

                        string trimmedFileName = fileName.Remove(0, removeStringStartPos);
                        int extensionStartPos = trimmedFileName.IndexOf($".{FileExtension}");
                        string stringToRemove = trimmedFileName.Remove(extensionStartPos, $".{FileExtension}".Length);

                        Console.WriteLine($"Remove string: {RemoveString} starts at pos: {removeStringStartPos}, and ends on pos: {removeStringEndPos}.");
                        var fileSrc = FullRefactorDirectoryPath;
                        string newFileName = fileName.Remove(removeStringStartPos, stringToRemove.Length);
                        File.Move(fileSrc + fileName, fileSrc + newFileName);
                    }

                    WriteLineHighlight($"Succesfully refactored: {DirectoryFiles.Length} files!\n");
                }
                else if (answer.ToLower() == "n") {
                    ReturnToMenu("");
                    return;
                }

                // TODO: Remove this when another step to this process has been added.
                // ReturnToMenu("");
                break;
        }
    }
}
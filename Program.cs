using System.CommandLine;
using System.CommandLine.Invocation;

using Realize;

// See https://aka.ms/new-console-template for more information
// Create a root command with some options
var rootCommand = new RootCommand
    {
        new Option<bool>(
            "--dry-run",
            "Run through the supplied input csproj"),
        new Option<bool>(
            "--replace-existing",
            "Replaces the existing project and creates a backup"),
        new Option<DirectoryInfo>(
            "--project-dir",
            "The C# Project directory"),
        new Option<FileInfo>(
            "--input-proj",
            "The C# Project file to process"),
        new Option<FileInfo>(
            "--output-proj",
            "The updated C# Project file to write"),
    };

rootCommand.Description = "Realize C# Project linked files to real files";

// Note that the parameters of the handler method are matched according to the names of the options
rootCommand.Handler = CommandHandler.Create<bool, bool, DirectoryInfo, FileInfo, FileInfo>((dryRun, replaceExisting, projectDir, inputProj, outputProj) =>
{
    var projectDirExists = projectDir?.Exists ?? false;
    var inputCsProjExists = inputProj?.Exists ?? false;
    var outputCsProjExists = outputProj?.Exists ?? false;

    if (!projectDirExists && !inputCsProjExists)
    {
        System.Console.Error.WriteLine("The --project-dir and the --input-proj are required and must both exist.");
        return;
    }

    if (!dryRun && !replaceExisting && outputProj == null)
    {
        System.Console.Error.WriteLine("The --output-proj is required if this isn't a dry run and the existing project isn't being replaced.");
        return;
    }


    var pd = new ProjectProcessor(dryRun, replaceExisting, projectDir!, inputProj!, outputProj);

    pd.Process();

});

// Parse the incoming args and invoke the handler
return rootCommand.InvokeAsync(args).Result;
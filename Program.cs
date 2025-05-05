using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using ExternalLocalizer.Hjson;

namespace TmlHjsonLinter;

public partial class Program
{
    static int Main(string[] args)
    {

        Console.WriteLine("TML Hjson Linter");

        var path = Directory.GetCurrentDirectory();
        if (args.Length == 1)
        {
            path = args[0];
        }
        else if (args.Length > 1)
        {
            Console.Error.WriteLine($"Too many arguments: {args.Length} > 1");
            return -1;
        }

        if (!Path.Exists(path))
        {
            Console.Error.WriteLine($"Path does not exist: {path}");
            return -1;
        }


        List<string> files;

        if (Directory.Exists(path))
        {
            _rootPath = path;
            files = Directory.EnumerateFiles(path, "*.hjson", SearchOption.AllDirectories).ToList();
        }
        else
        {
            if (Path.GetExtension(path) != ".hjson")
            {
                Console.Error.WriteLine($"File is not a .hjson file: {path}");
                return -1;
            }

            _rootPath = Path.GetDirectoryName(path);
            files = [path];
        }

        Console.WriteLine($"Found hjson files: {files.Count}");
        Console.WriteLine("");


        var success = 0;
        var failure = 0;
        var failureFiles = new List<string>();

        foreach (var file in files)
        {
            //Console.WriteLine($"Linting file: {GetRelativePath(file)}");
            if (Lint(file, out var values))
            {
                Console.WriteLine($"\t\x1b[32mOK\x1b[0m: {GetRelativePath(file)} with {values.Count} keys");
                success++;

            }
            else
            {
                Console.WriteLine($"\t\x1b[31mFAIL\x1b[0m: {GetRelativePath(file)}");
                failure++;
                failureFiles.Add(file);
            }
            Console.WriteLine("");
        }

        Console.WriteLine("Summary:");
        Console.WriteLine($"\t\x1b[32mSuccess\x1b[0m: {success}");
        Console.WriteLine($"\t\x1b[31mFailure\x1b[0m: {failure}");
        Console.WriteLine("");
        Console.WriteLine("\x1b[31mFailure files:\x1b[0m");
        foreach (var file in failureFiles)
            Console.WriteLine($"\t{GetRelativePath(file)}");

        return failure;
    }

    [GeneratedRegex(@"^(.+) At line (\d+), column (\d)+$", RegexOptions.Compiled)]
    private static partial Regex ErrorRegex();

    static bool Lint(string file, [NotNullWhen(true)] out List<(string, string)>? values)
    {
        var content = File.ReadAllText(file);
        try
        {
            values = HjsonCustom.Parse(content, "");

        }
        catch (ArgumentException e)
        {
            var match = ErrorRegex().Match(e.Message);
            if (match.Success)
            {
                string message = match.Groups[1].Value;
                int line = int.Parse(match.Groups[2].Value);
                int column = int.Parse(match.Groups[3].Value);

                Annotate(message, file, line, column);
            }
            else
            {
                Annotate(e.Message, file);
            }
            values = null;
            return false;
        }

        if (values.Count == 0)
        {
            Annotate("No keys found in Hjson file", file);
            values = null;
            return false;
        }

        return true;
    }

    static void Annotate(string message, string file, int line = 0, int column = 0)
    {
        Console.Error.WriteLine($"::error file={GetRelativePath(file)},line={line},col={column}::{message}");
    }

    static string? _rootPath;

    static string GetRelativePath(string path)
    {
        if (_rootPath != null)
        {
            path = Path.GetRelativePath(_rootPath, path);
        }
        return path.Replace("\\", "/");
    }
}

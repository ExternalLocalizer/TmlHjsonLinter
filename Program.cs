using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using ExternalLocalizer.Hjson;

namespace TmlHjsonLinter;

public partial class Program
{
    static int Main(string[] args)
    {

        var path = Directory.GetCurrentDirectory();
        if (args.Length == 1)
        {
            path = args[0];
        }
        else if (args.Length > 1)
        {
            Console.Error.WriteLine("Too many arguments");
            return -1;
        }

        if (!Path.Exists(path))
        {
            Console.Error.WriteLine("Path does not exist");
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
                Console.Error.WriteLine("File is not a .hjson file");
                return -1;
            }

            _rootPath = Path.GetDirectoryName(path);
            files = [path];
        }


        var success = 0;
        var failure = 0;
        var failureFiles = new List<string>();

        foreach (var file in files)
        {
            //Console.WriteLine($"Linting file: {GetRelativePath(file)}");
            if (Lint(file, out var values))
            {
                success++;

            }
            else
            {
                failure++;
                failureFiles.Add(file);
            }
            Console.WriteLine("");
        }

        Console.WriteLine("Linting complete");
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

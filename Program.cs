using System.Text.RegularExpressions;

using ExternalLocalizer.Hjson;

namespace TmlHjsonLinter;

public partial class Program
{
    [GeneratedRegex(@"^(.+) At line (\d+), column (\d)+$", RegexOptions.Compiled)]
    private static partial Regex ErrorRegex();

    static string? _rootPath;
    static void Main(string[] args)
    {

        var path = Directory.GetCurrentDirectory();
        if (args.Length == 1)
        {
            path = args[0];
        }
        else if (args.Length > 1)
        {
            Console.Error.WriteLine("Too many arguments");
            return;
        }

        if (!Path.Exists(path))
        {
            Console.Error.WriteLine("Path does not exist");
            return;
        }

        if (Directory.Exists(path))
        {
            _rootPath = path;
            foreach (string file in Directory.EnumerateFiles(path, "*.hjson", SearchOption.AllDirectories))
                Lint(file);
        }
        else
        {
            if (Path.GetExtension(path) != ".hjson")
            {
                Console.Error.WriteLine("File is not a .hjson file");
                return;
            }
            _rootPath = Path.GetDirectoryName(path);
            Lint(path);
        }

        Console.WriteLine("Linting complete");
    }

    static void Lint(string file)
    {
        var content = File.ReadAllText(file);
        List<(string, string)> jsonValue;
        try
        {
            jsonValue = HjsonCustom.Parse(content, "");

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
            return;
        }

        if (jsonValue.Count == 0)
        {
            Annotate("No keys found in Hjson file", file);
            return;
        }

        Console.WriteLine($"Successfully parsed Hjson file: {file}, {jsonValue.Count} keys found");
    }

    static void Annotate(string message, string file, int line = 0, int column = 0)
    {
        if (_rootPath != null)
        {
            file = Path.GetRelativePath(_rootPath, file);
        }
        file = file.Replace("\\", "/");

        Console.Error.WriteLine($"::error file={file},line={line},col={column}::{message}");
    }
}

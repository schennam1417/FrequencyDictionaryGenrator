using System.Text;
using Serilog;

// Configure Serilog at the top-level
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("C:\\Surekha\\Dictionary\\Logs\\logfile.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    if (args.Length != 2)
    {
        Log.Error("Invalid number of arguments. Expected: 2, Provided: {ArgsLength}", args.Length);
        Console.WriteLine("Error: Please provide exactly two arguments - input file path and output file path.");
        return;
    }

    string inputFilePath = args[0];
    string outputFilePath = args[1];

    Log.Information("Starting word frequency analysis.");
    Log.Debug("Input file path: {InputFilePath}, Output file path: {OutputFilePath}", inputFilePath, outputFilePath);

    // Validate input file
    if (!File.Exists(inputFilePath))
    {
        Log.Error("Input file does not exist: {InputFilePath}", inputFilePath);
        Console.WriteLine("Error: Input file does not exist.");
        return;
    }

    // Read input file
    Log.Information("Reading input file.");
    string inputText = File.ReadAllText(inputFilePath, Encoding.UTF8);

    // Process word frequencies
    Log.Information("Processing word frequencies.");
    var wordFrequencies = GetWordFrequencies(inputText);

    // Write output file
    Log.Information("Writing results to output file.");
    WriteWordFrequenciesToFile(wordFrequencies, outputFilePath);

    Log.Information("Word frequency analysis completed successfully.");
    Console.WriteLine("Word frequency analysis completed successfully.");
}
catch (UnauthorizedAccessException ex)
{
    Log.Error(ex, "Access to the file is denied.");
    Console.WriteLine("Error: Access to the file is denied. Please check file permissions.");
}
catch (IOException ex)
{
    Log.Error(ex, "An I/O error occurred.");
    Console.WriteLine($"Error: An I/O error occurred. {ex.Message}");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unexpected error occurred.");
    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
}
finally
{
    Log.CloseAndFlush();
}

static Dictionary<string, int> GetWordFrequencies(string text)
{
    // split text into words
    var words = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => word.Trim().ToLowerInvariant());

    // Count word frequencies
    var frequencyDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    foreach (var word in words)
    {
        if (frequencyDictionary.ContainsKey(word))
        {
            frequencyDictionary[word]++;
        }
        else
        {
            frequencyDictionary[word] = 1;
        }
    }

    // Sort by frequency descending, then alphabetically
    return frequencyDictionary
        .OrderByDescending(kvp => kvp.Value)
        .ThenBy(kvp => kvp.Key)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}

static void WriteWordFrequenciesToFile(Dictionary<string, int> wordFrequencies, string outputFilePath)
{
    try
    {
        using var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);
        foreach (var kvp in wordFrequencies)
        {
            writer.WriteLine($"{kvp.Key}:{kvp.Value}");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error writing to output file: {OutputFilePath}", outputFilePath);
        throw;
    }
}

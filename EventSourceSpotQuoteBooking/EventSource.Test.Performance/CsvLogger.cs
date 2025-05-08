using System.Text;

namespace EventSource.Test.Performance;

public static class CsvLogger
{
    private static readonly object fileLock = new();
    private static readonly HashSet<string> initializedFiles = new();
    private static readonly string TestResultsDir;
    private static readonly DateTime Date = DateTime.UtcNow;

    static CsvLogger()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && dir.Name != "bin")
        {
            dir = dir.Parent;
        }

        var projectRoot = dir?.Parent?.Parent?.FullName ?? Directory.GetCurrentDirectory();
        TestResultsDir = Path.Combine(projectRoot, "TestResults");

        if (!Directory.Exists(TestResultsDir))
        {
            Directory.CreateDirectory(TestResultsDir);
        }
    }

    public static void LogRepo(
        string testName,
        bool eventTrue,
        bool entityTrue,
        bool personalTrue,
        string sizeName,
        double mb,
        int count,
        int propCount,
        int nodes,
        IEnumerable<long> durations
    )
    {
        var fullPath = Path.Combine(TestResultsDir, $"{testName}-{Date:yyyy-MM-dd-HH-mm-ss}.csv");

        var sb = new StringBuilder();
        foreach (var duration in durations)
            sb.AppendLine(
                $"{testName};{sizeName};{mb};{count};{nodes};{propCount};{eventTrue};{entityTrue};{personalTrue};{duration:F2}"
            );

        var line = sb.ToString();

        lock (fileLock)
        {
            if (!initializedFiles.Contains(fullPath))
            {
                if (!File.Exists(fullPath))
                {
                    File.WriteAllText(
                        fullPath,
                        "TestName;SizeName;Size (mb);Count;Nodes;Props;EventStoreEnabled;EntityStoreEnabled;PersonalDataStoreEnabled;Time (ms)\n",
                        Encoding.UTF8
                    );
                }

                initializedFiles.Add(fullPath);
            }

            File.AppendAllText(fullPath, line);
        }
    }
}

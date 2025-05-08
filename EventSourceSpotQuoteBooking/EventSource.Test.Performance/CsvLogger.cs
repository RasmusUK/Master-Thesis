using System.Text;

namespace EventSource.Test.Performance;

public static class CsvLogger
{
    private static readonly object fileLock = new();
    private static readonly HashSet<string> initializedFiles = new();

    public static readonly string TestResultsDir;

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

    public static void LogRepoCreate(
        string fileName,
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
        var sorted = durations.OrderBy(x => x).ToList();
        var min = sorted.FirstOrDefault();
        var max = sorted.LastOrDefault();
        var avg = durations.Average();
        var total = durations.Sum();

        var fullPath = Path.Combine(TestResultsDir, fileName);

        lock (fileLock)
        {
            if (!initializedFiles.Contains(fullPath))
            {
                if (!File.Exists(fullPath))
                {
                    File.WriteAllText(
                        fullPath,
                        "TestName;SizeName;Size (mb);Count;Nodes;Props;EventStoreEnabled;EntityStoreEnabled;PersonalDataStoreEnabled;Min (ms);Max (ms);Avg (ms);Total (ms)\n",
                        Encoding.UTF8
                    );
                }

                initializedFiles.Add(fullPath);
            }

            var line =
                $"{testName};{sizeName};{mb};{count};{nodes};{propCount};{eventTrue};{entityTrue};{personalTrue};{min:F2};{max:F2};{avg:F2};{total:F2}";
            File.AppendAllText(fullPath, line + Environment.NewLine);
        }
    }
}

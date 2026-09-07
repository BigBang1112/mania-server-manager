using System.Collections;

namespace ManiaServerManager.Tests;

internal sealed class TestEnvironment : IDisposable
{
    private readonly Dictionary<string, string?> originalVariables;

    public TestEnvironment()
    {
        originalVariables = GetMsmVariables();
        ClearMsmVariables();
        Set("MSM_SERVER_TYPE", "TM2020");
        Set("MSM_ONLY_SETUP", "True");
    }

    public void Set(string name, string? value) => Environment.SetEnvironmentVariable(name, value);

    public void Dispose()
    {
        ClearMsmVariables();
        foreach (var (name, value) in originalVariables)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    private static Dictionary<string, string?> GetMsmVariables()
    {
        return Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .Where(entry => entry.Key is string name && name.StartsWith("MSM_", StringComparison.Ordinal))
            .ToDictionary(entry => (string)entry.Key, entry => entry.Value as string);
    }

    private static void ClearMsmVariables()
    {
        foreach (var name in GetMsmVariables().Keys)
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }
}

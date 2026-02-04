using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dalamud.Plugin.Services;
using MemoriaAlphaSonnetv2.Models;

namespace MemoriaAlphaSonnetv2.Services;

/// <summary>
/// Loads and manages MSQ quest data from hierarchical JSON structure
/// Structure: Data/{expansion}/{patch}/1-msq.json
/// Phase 1: Loads ALL 1-msq.json files (proof-of-concept)
/// Future: Load based on TocService milestone detection
/// </summary>
public class QuestService
{
    private readonly IPluginLog _log;
    private readonly string _dataDirectory;

    private List<Quest> _quests;

    /// <summary>
    /// All loaded quests (read-only access)
    /// </summary>
    public IReadOnlyList<Quest> Quests => _quests.AsReadOnly();

    /// <summary>
    /// Performance metric: How long quest data took to load (milliseconds)
    /// </summary>
    public long LoadTimeMs { get; private set; }

    /// <summary>
    /// Number of JSON files successfully loaded
    /// </summary>
    public int FilesLoaded { get; private set; }

    /// <summary>
    /// Constructor loads quest data immediately (eager loading)
    /// </summary>
    /// <param name="log">Dalamud logging service</param>
    /// <param name="pluginDirectory">Root plugin directory (contains Data/)</param>
    public QuestService(IPluginLog log, string pluginDirectory)
    {
        _log = log;
        _dataDirectory = Path.Combine(pluginDirectory, "Data");
        _quests = new List<Quest>();

        LoadQuestData();
    }

    /// <summary>
/// Load quest JSON files from Data/ directory
/// Phase 1: Only loads 1-msq.json as proof-of-concept
/// </summary>
private void LoadQuestData()
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        // Check if Data directory exists
        if (!Directory.Exists(_dataDirectory))
        {
            _log.Error($"[QuestService] Data directory not found: {_dataDirectory}");
            return;
        }

        // Find all 1-msq.json files recursively
        var msqFiles = Directory.GetFiles(_dataDirectory, "1-msq.json", SearchOption.AllDirectories);

        if (msqFiles.Length == 0)
        {
            _log.Warning("[QuestService] No 1-msq.json files found in Data/ directory");
            return;
        }

        _log.Information($"[QuestService] Found {msqFiles.Length} MSQ files to load");

        // Load each file
        int totalQuestsLoaded = 0;
        FilesLoaded = 0;

        foreach (var filePath in msqFiles)
        {
            try
            {
                // Extract patch info from path (e.g., "2.x/2.0")
                var relativePath = Path.GetRelativePath(_dataDirectory, filePath);
                var patchInfo = Path.GetDirectoryName(relativePath); // "2.x\2.0"

                // Read and deserialize JSON (using wrapper model)
                var jsonContent = File.ReadAllText(filePath);
                var questFile = JsonSerializer.Deserialize<QuestFile>(jsonContent);

                if (questFile == null)
                {
                    _log.Warning($"[QuestService] Failed to deserialize: {patchInfo}/1-msq.json");
                    continue;
                }

                if (questFile.Quests == null || questFile.Quests.Count == 0)
                {
                    _log.Verbose($"[QuestService] Empty quest array: {patchInfo}/1-msq.json");
                    FilesLoaded++; // Count it as loaded (just empty)
                    continue;
                }

                // Add to master list
                _quests.AddRange(questFile.Quests);
                totalQuestsLoaded += questFile.Quests.Count;
                FilesLoaded++;

                _log.Verbose($"[QuestService] Loaded {questFile.Quests.Count} quests from {patchInfo}/1-msq.json");
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[QuestService] Failed to load file: {filePath}");
            }
        }

        stopwatch.Stop();
        LoadTimeMs = stopwatch.ElapsedMilliseconds;

        // Log summary
        _log.Information($"[QuestService] Loaded {totalQuestsLoaded} quests from {FilesLoaded}/{msqFiles.Length} files in {LoadTimeMs}ms");
        
        if (_quests.Count > 0)
        {
            _log.Information($"[QuestService] Quest range: {_quests.First().Name} → {_quests.Last().Name}");
        }
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        _log.Error(ex, "[QuestService] Failed to load quest data");
    }
}


    /// <summary>
    /// Get quest by ID (for completion checks)
    /// </summary>
    public Quest? GetQuestById(uint questId)
    {
        return _quests.FirstOrDefault(q => q.Id == questId);
    }

    /// <summary>
    /// Get quests by expansion abbreviation
    /// </summary>
    public IEnumerable<Quest> GetQuestsByExpansion(string expansion)
    {
        return _quests.Where(q => q.Expansion.Equals(expansion, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get quests by patch number
    /// </summary>
    public IEnumerable<Quest> GetQuestsByPatch(string patch)
    {
        return _quests.Where(q => q.Patch == patch);
    }

    /// <summary>
    /// Get quest statistics by expansion
    /// </summary>
    public Dictionary<string, int> GetQuestCountsByExpansion()
    {
        return _quests.GroupBy(q => q.Expansion)
                      .ToDictionary(g => g.Key, g => g.Count());
    }
}

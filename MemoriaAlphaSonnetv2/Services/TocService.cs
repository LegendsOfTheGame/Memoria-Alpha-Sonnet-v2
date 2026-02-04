using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dalamud.Plugin.Services;
using MemoriaAlphaSonnetv2.Models;

namespace MemoriaAlphaSonnetv2.Services;

/// <summary>
/// Service for loading and querying Table of Contents (milestone quests).
/// Loads toc.json eagerly on construction - TOC is always needed before quest filtering.
/// </summary>
public class TocService
{
    // Dalamud services injected via constructor
    private readonly IPlayerState _playerState;  // For checking quest completion
    private readonly IPluginLog _log;             // For logging
    private readonly string _tocFilePath;         // Path to toc.json
    
    /// <summary>
    /// Gets all TOC entries (loaded during construction).
    /// </summary>
    public List<TocEntry> Entries { get; private set; }
    
    // TEACHING NOTE: Simple property with private setter
    // Data loaded once in constructor, immutable after that
    
    
    public TocService(IPlayerState playerState, IPluginLog log, string pluginDirectory)
    {
        _playerState = playerState;
        _log = log;
        
        // Build path to toc.json (in same directory as plugin DLL)
        _tocFilePath = Path.Combine(pluginDirectory, "toc.json");
        
        // EAGER LOAD: Parse TOC immediately (blocks plugin init for ~2ms)
        // WHY: TOC acts as "gateway" - must know milestones before loading quest files
        // Small file (2KB) = acceptable startup cost for cleaner architecture
        Entries = LoadTocJson();
        
        _log.Information($"TocService initialized with {Entries.Count} TOC entries");
    }
    
    
    /// <summary>
    /// Loads toc.json from disk and deserializes to TocEntry list.
    /// Called once during construction.
    /// </summary>
    private List<TocEntry> LoadTocJson()
    {
        try
        {
            // Check if file exists
            if (!File.Exists(_tocFilePath))
            {
                _log.Error($"toc.json not found at: {_tocFilePath}");
                return new List<TocEntry>(); // Return empty list (fail gracefully)
            }
            
            // Read entire file as string
            var jsonContent = File.ReadAllText(_tocFilePath);
            
            // Deserialize JSON to List<TocEntry>
            // System.Text.Json automatically maps JSON properties to TocEntry properties
            var entries = JsonSerializer.Deserialize<List<TocEntry>>(jsonContent);
            
            if (entries == null)
            {
                _log.Warning("toc.json deserialized to null, returning empty list");
                return new List<TocEntry>();
            }
            
            _log.Information($"Loaded {entries.Count} TOC entries from toc.json");
            return entries;
        }
        catch (Exception ex)
        {
            // Log error but don't crash the plugin
            // Defensive: Plugin can still run without TOC (just won't show milestones)
            _log.Error(ex, "Failed to load toc.json");
            return new List<TocEntry>();
        }
    }
    
    
    /// <summary>
    /// Checks if a specific quest ID is completed by the current character.
    /// STUB: Returns false for now (Thread 2 scope - implement in Thread 3).
    /// </summary>
    public bool IsQuestComplete(int questId)
    {
        // TODO (Thread 3): Use IQuestManager to check actual completion status
        // For now, always return false so we can test the TOC loading logic
        return false;
    }
    
    
    /// <summary>
    /// Finds the highest completed MSQ milestone for the current character.
    /// Checks only "Final" role entries (end of patch cycles).
    /// Returns patch number (e.g., "2.5", "7.0") or null if none completed.
    /// </summary>
    public string? GetHighestCompletedMilestone()
    {
        // Filter to only "Final" role entries (end of patch)
        var finalEntries = Entries.Where(e => e.Role == "Final").ToList();
        
        // TEACHING NOTE: LINQ Where() filters the list
        // Think: "Give me only entries where Role equals 'Final'"
        // ToList() converts the filtered IEnumerable back to List
        
        if (finalEntries.Count == 0)
        {
            _log.Warning("No 'Final' entries found in TOC");
            return null;
        }
        
        // Iterate backwards (newest patches first: 7.3 → 2.0)
        // Stop at first completed milestone (assumes sorted by patch order in JSON)
        for (int i = finalEntries.Count - 1; i >= 0; i--)
        {
            var entry = finalEntries[i];
            
            // Check if ANY quest ID in this milestone is complete
            // (Player only needs to complete ONE path for the milestone to count)
            if (entry.Ids.Any(questId => IsQuestComplete(questId)))
            {
                _log.Information($"Highest MSQ milestone: {entry.Patch} ({entry.Name})");
                return entry.Patch;
            }
        }
        
        // No milestones completed yet (brand new character)
        _log.Information("No MSQ milestones completed yet");
        return null;
    }
}

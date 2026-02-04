using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MemoriaAlphaSonnetv2.Models;

/// <summary>
/// Represents a single quest in the MSQ progression
/// Maps to JSON quest files with Title/Id/Area/Start/Level structure
/// </summary>
public class Quest
{
    /// <summary>
    /// Quest title/name
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Array of quest IDs (some quests have multiple IDs for different starting cities)
    /// Stored as array, but exposed as single ID for simplicity
    /// </summary>
    [JsonPropertyName("Id")]
    public List<uint> IdArray { get; set; } = new();

    /// <summary>
    /// Primary quest ID (first in array)
    /// </summary>
    [JsonIgnore]
    public uint Id => IdArray.Count > 0 ? IdArray[0] : 0;

    /// <summary>
    /// Geographic area where quest takes place
    /// </summary>
    [JsonPropertyName("Area")]
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// Starting city/location for quest
    /// </summary>
    [JsonPropertyName("Start")]
    public string Start { get; set; } = string.Empty;

    /// <summary>
    /// Level requirement for the quest
    /// </summary>
    [JsonPropertyName("Level")]
    public int Level { get; set; }

    // Helper properties for compatibility with existing code
    
    /// <summary>
    /// Alias for Title (for code consistency)
    /// </summary>
    [JsonIgnore]
    public string Name => Title;

    /// <summary>
    /// Alias for Area (for code consistency)
    /// </summary>
    [JsonIgnore]
    public string Location => Area;

    /// <summary>
    /// Expansion abbreviation (derived from QuestFile metadata)
    /// </summary>
    [JsonIgnore]
    public string Expansion { get; set; } = string.Empty;

    /// <summary>
    /// Patch number (derived from QuestFile metadata)
    /// </summary>
    [JsonIgnore]
    public string Patch { get; set; } = string.Empty;

    /// <summary>
    /// Drawer identifier (e.g., "1-msq", "2-NewEra")
    /// </summary>
    [JsonIgnore]
    public string Drawer { get; set; } = string.Empty;  // 🆕 ADD THIS

    /// <summary>
    /// For debugging - shows key quest info in logs
    /// </summary>
    public override string ToString()
    {
        var idList = IdArray.Count > 1 ? $"[{string.Join(", ", IdArray)}]" : IdArray.FirstOrDefault().ToString();
        return $"{Title} (Lv{Level}, {Area}, ID: {idList})";
    }
    
/// <summary>
/// Grand Company restriction (e.g., "Twin Adder", "Maelstrom", "Immortal Flames")
/// </summary>
[JsonPropertyName("Gc")]
public string Gc { get; set; } = string.Empty;  // 🆕 ADD THIS
}

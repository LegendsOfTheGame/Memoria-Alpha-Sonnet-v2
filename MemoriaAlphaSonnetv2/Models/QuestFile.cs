using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MemoriaAlphaSonnetv2.Models;

/// <summary>
/// Represents a quest JSON file with metadata wrapper
/// Matches structure: { "expansion": "2.0", "drawer": "1-msq", "quests": [...] }
/// </summary>
public class QuestFile
{
    /// <summary>
    /// Expansion/patch identifier (e.g., "2.0", "3.1")
    /// </summary>
    [JsonPropertyName("expansion")]
    public string Expansion { get; set; } = string.Empty;

    /// <summary>
    /// Drawer/category identifier (e.g., "1-msq", "2-NewEra")
    /// </summary>
    [JsonPropertyName("drawer")]
    public string Drawer { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable title for this quest collection
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Array of quests in this file
    /// </summary>
    [JsonPropertyName("quests")]
    public List<Quest> Quests { get; set; } = new();
}

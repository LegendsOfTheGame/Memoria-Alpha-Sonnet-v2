namespace MemoriaAlphaSonnetv2.Models;

/// <summary>
/// Represents a single milestone entry from toc.json (Table of Contents).
/// Marks either the start or final quest of a patch cycle.
/// </summary>
public class TocEntry
{
    // Patch number (e.g., "2.5", "7.0")
    public required string Patch { get; init; }
    
    // Expansion abbreviation (ARR, HW, SB, ShB, EW, DT)
    public required string Expansion { get; init; }
    
    // "Start" or "Final" - marks beginning or end of patch story arc
    public required string Role { get; init; }
    
    // Quest name (e.g., "Before the Dawn", "Dawntrail")
    public required string Name { get; init; }
    
    // Array of quest IDs (handles alternate starting cities or job variations)
    // A player only needs to complete ONE of these IDs to count as "milestone reached"
    public required int[] Ids { get; init; }
    
    // TEACHING NOTES:
    // - int[] (array) not List<int>: JSON deserializes to arrays naturally
    // - 'required' keyword (C# 11): Forces all properties to be set
    // - 'init' keyword (C# 9): Immutable after construction (thread-safe)
    // - We'll check if ANY Id in this array is complete = milestone unlocked
}

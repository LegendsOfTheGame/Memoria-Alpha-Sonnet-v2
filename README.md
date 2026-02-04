## Current Status

**Version:** 14.2.0.2  
**Status:** ARR 2.0 MSQ + NewEra Complete

### Quest Coverage
- ✅ **290 Main Scenario Quests** (2.0) - Complete ARR storyline
- ✅ **6 Chronicles of a New Era Quests** (2.0)
  - Hard Mode Primals: Ifrit, Garuda, Titan
  - Binding Coil of Bahamut: Turns 1-5 unlock
- ⏳ Feature quests (planned)
- ⏳ Beast tribe quests (planned)
- ⏳ Class/Job quests (planned)
- ⏳ Seasonal events (planned)

### Technical Details
- **Total 2.0 Quests:** 296
- **Drawer Types Loaded:** 2 (MSQ, NewEra)
- **Load Performance:** ~11ms for quest service initialization
- **Data Architecture:** 36 patch folders ready (2.0 through 7.5)


## Installation

Enable dev plugins in XIVLauncher settings, then place plugin in %AppData%\XIVLauncher\devPlugins\MemoriaAlphaSonnetv2\

## Development

Requires Visual Studio 2026, .NET 10.0, Dalamud API 14.

git clone <repo-url>
Open MemoriaAlphaSonnetv2.sln in Visual Studio 2026
Build → Build Solution

## Versioning

Format: AA.B.E.D (API.Band.Expansion.Drawer)

See Memoria Alpha Versioning Specific.txt for details.

## Contributing

PRs welcome. This is a learning project so code quality evolves over time.

## Acknowledgments

- SamplePlugin: https://github.com/goatcorp/SamplePlugin
- QuestTracker: https://github.com/isaiahcat/QuestTracker

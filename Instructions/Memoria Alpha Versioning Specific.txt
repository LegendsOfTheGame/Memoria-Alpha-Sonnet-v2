# Memoria Alpha Versioning Specification

## Format: AA.B.E.D

### AA - Dalamud API Version
- Current: **14** (FFXIV Patch 7.4-7.49)
- Next: **15** (FFXIV Patch 7.5+, late April 2026)
- Increments: When new Dalamud API releases (major game patches)

### B - Expansion Band Index
- 2 = A Realm Reborn (ARR 2.x)
- 3 = Heavensward (HW 3.x)
- 4 = Stormblood (StB 4.x)
- 5 = Shadowbringers (ShB 5.x)
- 6 = Endwalker (EW 6.x)
- 7 = Dawntrail (DT 7.x)

### E - Expansion Patch Number
- 0 = x.0 (launch patch)
- 1 = x.1
- 2 = x.2
- 3 = x.3
- 4 = x.4
- 5 = x.5

### D - Drawer Completion Milestone
- 0 = Folder exists, no drawers complete
- 1 = 1-msq.json complete
- 2 = 2-NewEra.json complete
- 3 = 3-Feature.json complete
- 4 = 4-Beasts.json complete
- 5 = 5-Class.json complete
- 6 = 6-Seasonal.json complete
- 7 = 7-Other.json complete
- 9 = Entire expansion complete

## Current Target
**14.2.0.1** = API 14, ARR 2.0, MSQ drawer complete

## Example Progression
- 14.2.0.0 → Foundation exists
- 14.2.0.1 → ARR 2.0 MSQ complete ← **First milestone**
- 14.2.5.7 → All ARR drawers complete
- 15.3.0.1 → API 15, HW 3.0 MSQ complete

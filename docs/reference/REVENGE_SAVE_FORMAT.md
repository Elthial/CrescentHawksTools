# Revenge save-game format

`SAVEGAME.DAT` is a fixed `0x61C0`-byte container with six save slots.
Multi-byte values are little-endian. Its boundary table is verified against the
original file layout rather than inferred from labels in individual saves.

## Container layout

| Region | Offset/size | Meaning |
| --- | --- | --- |
| Boundary table | `0x0000`, 9 × `uint16` | `{0007,0012,009A,10CB,20FC,312D,415E,518F,61C0}` |
| Slot directory | `0x0012`, 6 × `0x16` | Five control bytes plus a 17-byte editor label view |
| Slot 1 payload | `0x009A..0x10CA` | `0x1031` bytes |
| Slot 2 payload | `0x10CB..0x20FB` | `0x1031` bytes |
| Slot 3 payload | `0x20FC..0x312C` | `0x1031` bytes |
| Slot 4 payload | `0x312D..0x415D` | `0x1031` bytes |
| Slot 5 payload | `0x415E..0x518E` | `0x1031` bytes |
| Slot 6 payload | `0x518F..0x61BF` | `0x1031` bytes |

The original C view treats a descriptor as an occupied byte plus a 21-byte
name field. The tooling exposes five control bytes plus the 17 printable bytes
used by the editor. These are two views of the same `0x16` bytes, not different
disk formats.

## Slot payload

| Relative offset | Length/count | Meaning |
| ---: | ---: | --- |
| `0x0000` | `0x0221` | Campaign snapshot |
| `0x0221` | 24 × `0x96` | Live unit records |

### Campaign snapshot

| Offset | Size | Meaning |
| ---: | ---: | --- |
| `0x0000` | 2 | Campaign stage index |
| `0x0002` | 2 | Scenario variant index |
| `0x0004` | 1 | Campaign phase |
| `0x0005` | 1 | Training-sequence flag |
| `0x0006` | 2 | Campaign flags |
| `0x0008..0x001B` | 20 | Named and unresolved campaign-state words |
| `0x001C` | 3 | Scenario clock |
| `0x001F` | 6 | Training-scenario completion flags |
| `0x0025` | 24 | Scenario-unit map X coordinates |
| `0x003D` | 24 | Scenario-unit map Y coordinates |
| `0x0055..0x006B` | 23 | Partially named scenario state |
| `0x006C` | 18 × `0x0F` | Unit-selection/scenario cards |
| `0x017A` | 13 × `0x0A` | Lance assignments |
| `0x01FC` | 37 | Player pilot availability |

The snapshot ends exactly at `0x0221`. A scenario card stores geometry, state,
persistent-unit and assignment slots, two pilot selections, and an original
four-byte DOS sprite-buffer address. A lance assignment stores geometry,
style, persistent/live unit slots and pilot ID.

The live unit format is documented in [Revenge units](REVENGE_UNITS.md).
Use `dump-save`, `compare-save`, `analyze-save-set`, `export-save-state` and
`import-save-state` for typed inspection. Unknown snapshot bytes are retained
verbatim; a friendly name is not evidence for silently discarding them.

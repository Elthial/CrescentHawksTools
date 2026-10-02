# Inception save-game format

The verified save record is `0x0F49` bytes. Its first `0x0F44` bytes are the
saved campaign-state block; the final five bytes contain the party's world-map
coordinates and associated tail state. Multi-byte values are little-endian.

## Top-level regions

| Offset | Length | Count | Meaning |
| ---: | ---: | ---: | --- |
| `0x0000` | 1 | 1 | Header/status byte |
| `0x0001` | `0x11` | 8 | Party character records |
| `0x0089` | `0x11` | 8 | Enemy character records |
| `0x0111` | `0x7D` | 4 | Party BattleMech records |
| `0x0305` | `0x7D` | 4 | Enemy BattleMech records |
| `0x04F9` | `0x0800` | 1 | Probable world-map visibility block |
| `0x0CF9` | 1 | 1 | Story/Citadel mission-state byte |
| `0x0D5D` | 4 | 1 | C-Bills |
| `0x0D61` | 4 each | 3 | DefHes, NasDiv and BakPhar stock values |
| `0x0F45` | 2 | 1 | Party world-map X coordinate |
| `0x0F47` | 2 | 1 | Party world-map Y coordinate |

The character and mech subrecords are described in
[Inception characters](INCEPTION_CHARACTERS.md) and
[Inception BattleMechs](INCEPTION_MECHS.md).

## Repeated-record offsets

| Record | Party offsets | Enemy offsets |
| --- | --- | --- |
| Characters | `0001, 0012, 0023, 0034, 0045, 0056, 0067, 0078` | `0089, 009A, 00AB, 00BC, 00CD, 00DE, 00EF, 0100` |
| BattleMechs | `0111, 018E, 020B, 0288` | `0305, 0382, 03FF, 047C` |

## Evidence notes and open questions

- The original investigation identified `3092:C164` as an executable-owned
  initial-state block corresponding to save data. The typed parser verifies the
  disk layout; a complete byte-for-byte provenance map back to that segment is
  not yet published.
- The historical note “`246C:0B80` — little map?” is retained as an unresolved
  hypothesis. It is not a verified save-field name and must not be used as a
  parser contract.
- `0x04F9..0x0CF8` behaves like map visibility in current evidence, hence the
  deliberate “probable” qualifier.

Use `dump-save FILE --json` for the container and `dump-mech FILE SLOT --json`
for a typed mech record. JSON output preserves unknown/raw fields so new names
can be added without losing evidence.

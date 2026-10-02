# Inception format notes

## Save (`GAME1` …)

The recognized save is exactly `0x0F49` bytes. Character records are `0x20`
bytes and BattleMech records are `0x7D` bytes. The editor exposes only mapped
fields, retains the original byte array, fingerprints it with SHA-256, applies
field changes over that source, and refuses to overwrite the source file.

A BattleMech record contains a 16-byte name/status area, tonnage, 11 current
armor bytes, eight current internal-structure bytes, packed actuator state,
engine heat-sink capacity, ten ammunition bytes, walk/jump movement, `0x23`
critical-slot bytes, corresponding maximum arrays, damage counters, pilot and
rider IDs, and upgrade flags.

## Building scripts (`.BLD`)

| Offset | Size | Meaning |
| ---: | ---: | --- |
| `0x00` | 2 | encoded payload length; equals file length minus two |
| `0x02` | variable | bytewise encoded script payload |

Each stored payload byte is decoded as `((stored + 0x29) & 0xFF) ^ 0xE9`.
Opcodes occupy `0xE4` through `0xFF`; lower bytes are inline data/text. Branch
tables are bounded to the decoded payload. Ambiguous tables stop disassembly.

## Animation (`.ANM`)

The fixed header is `0x60` bytes and includes a 32-byte playback-control area
and timing values. The remainder is a tokenized accumulated frame stream for
88×88 four-bit pixels (`0x12E8` packed bytes per frame). Positive controls copy
literals; negative controls repeat a byte; zero introduces an extended repeat.

## Music (`.SIF`)

SIF is a headerless stream of four note-code bytes per logical frame. `0x80`
is rest. PC-speaker playback consumes bytes sequentially; Tandy playback uses
four channels. WAV output is a portable square-wave rendering, not an exact
electrical model.

## Maps (`.MTP`)

Standard maps have a `0x1D` header followed by byte tile IDs. Known profiles
define dimensions, tile set, and storage order. One exceptional map uses a
compact `0x0300` layout. JSON preserves source and normalized tile IDs.

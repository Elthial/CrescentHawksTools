# Revenge format notes

## Graphics

- **CPS**: 10-byte envelope, stored length, compression type 1–3, declared
  decoded length 64,000, then Westwood LZW-12/LZW-14/RLE data for 320×200 pixels.
- **CMP**: 16-bit stored payload length, type byte, four-bit packed pixels;
  type 2 traverses vertically.
- **ICN**: CMP-style envelope for 250 sequential 16×16 tiles. Physical trailing
  bytes are reported separately from the declared stream.
- **COL**: 768 bytes, 256 RGB triples with six-bit components (`0..63`).
- **FNT**: stored length, 128 glyph offsets, height/width, then glyph rows.

## Maps and scenes

MAP begins with 16-bit width and height, followed by eight bytes that duplicate
the first eight grid cells, then `width × height` byte tile IDs. The duplicate
prefix is validated.

SCENE starts with a 16-bit stored length and fixed resource selectors, then a
bounded bytecode area and 72 NUL-terminated messages. Unknown opcodes stop
conservative traversal; branches may not leave the script span.

## Unit templates

`MECHTYPE.DAT` is exactly 89 records of `0x96` bytes. Records carry type,
movement, heat-sink capacity, armor/internal arrays, equipment locations,
ammunition, tactical sprite selection, and damage-model information.
BattleMechs, vehicles, and infantry share the envelope but not every meaning.

## Saves

`SAVEGAME.DAT` is `0x61C0` bytes and contains fixed slots. Editing is a
fingerprinted patch over original bytes. Only verified fields are writable;
identity, campaign state, maxima, and unknown bytes remain read-only.

## Audio

MUS files are standard MIDI (`MThd`). Digital descriptors select bounded BIN
resource spans; samples are unsigned 8-bit mono PCM at 8,000 Hz. PC-speaker
streams end in `0xFF` and must terminate inside their exact pointer span.

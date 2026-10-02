# Format specifications

| Game | Specification | Formats |
| --- | --- | --- |
| Inception | [Container and record formats](INCEPTION_FORMATS.md) | SAVE, BLD, ANM, SIF, MTP |
| Inception | [Graphics](INCEPTION_GRAPHICS.md) | CMP, ICN |
| Revenge | [Container and record formats](REVENGE_FORMATS.md) | CPS, CMP, ICN, COL, FNT, MAP, SCENE, unit, save, audio |

All integer fields are little-endian unless stated otherwise. Offsets are
file-relative. Parsers reject arithmetic overflow, out-of-range spans, missing
terminators, and inconsistent stored lengths.

# Revenge maps, scenes and graphics resources

## Tactical maps

Every `.MAP` file begins with a 12-byte header. The first four bytes are width
and height as little-endian words; the next eight duplicate the first eight
tile IDs. Exactly `width × height` tile bytes follow.

| File | Dimensions | File size | Normal icon set(s) |
| --- | ---: | ---: | --- |
| MAP0.MAP | 55×70 | 3,862 | ICONSET0 |
| MAP1.MAP | 50×100 | 5,012 | ICONSET1 |
| MAP2.MAP | 100×200 | 20,012 | ICONSET0 |
| MAP3.MAP | 200×100 | 20,012 | ICONSET2 |
| MAP4.MAP | 60×100 | 6,012 | ICONSET3 |
| MAP5.MAP | 100×100 | 10,012 | ICONSET4 |
| MAP6.MAP | 100×100 | 10,012 | ICONSET5 |
| MAP7.MAP | 100×100 | 10,012 | ICONSET6 |
| MAP8.MAP | 160×100 | 16,012 | ICONSET7 |
| MAP9.MAP | 100×100 | 10,012 | ICONSET7 |
| MAPA.MAP | 125×100 | 12,512 | ICONSET7 |

`ICONSET0..7.ICN` are normal terrain sheets and `DESTROY0..7.ICN` are their
destruction counterparts. Each verified sheet contains 250 16×16 tiles.
`MECHSET1.ICN` and `MECHSET2.ICN` contain tactical unit graphics.

## Scenario catalogue

Each scene contains a 73-entry offset table, `0x92` bytes of metadata, an
instruction stream and 72 message slots. The map and icon-set selectors below
come directly from scene metadata. Instruction bytes are counts excluding the
two-byte reserved word.

| Scene | Map | Icon set | Instruction bytes | Non-empty messages |
| --- | --- | --- | ---: | ---: |
| SCENE1 | MAP0 | ICONSET0 | 166 | 14 |
| SCENE2 | MAP0 | ICONSET0 | 187 | 19 |
| SCENE3 | MAP0 | ICONSET0 | 193 | 29 |
| SCENE4 | MAP0 | ICONSET0 | 128 | 28 |
| SCENE5 | MAP1 | ICONSET1 | 705 | 24 |
| SCENE6 | MAP1 | ICONSET1 | 219 | 29 |
| SCENE7 | MAP0 | ICONSET0 | 244 | 16 |
| SCENE8 | MAP2 | ICONSET0 | 303 | 32 |
| SCENE9 | MAP2 | ICONSET0 | 336 | 24 |
| SCENEA | MAP2 | ICONSET0 | 193 | 30 |
| SCENEB | MAP3 | ICONSET2 | 488 | 44 |
| SCENEC | MAP3 | ICONSET2 | 456 | 46 |
| SCENED | MAP4 | ICONSET3 | 346 | 47 |
| SCENEE | MAP3 | ICONSET2 | 494 | 46 |
| SCENEF | MAP5 | ICONSET4 | 423 | 47 |
| SCENEG | MAP5 | ICONSET4 | 408 | 46 |
| SCENEH | MAP5 | ICONSET4 | 393 | 46 |
| SCENEI | MAP5 | ICONSET4 | 342 | 45 |
| SCENEJ | MAP5 | ICONSET4 | 429 | 46 |
| SCENEK | MAP5 | ICONSET4 | 483 | 46 |
| SCENEL | MAP6 | ICONSET5 | 389 | 31 |
| SCENEM | MAP6 | ICONSET5 | 486 | 45 |
| SCENEN | MAP7 | ICONSET6 | 589 | 45 |
| SCENEO | MAP8 | ICONSET7 | 600 | 47 |
| SCENEP | MAP8 | ICONSET7 | 666 | 47 |
| SCENEQ | MAP9 | ICONSET7 | 451 | 45 |
| SCENER | MAPA | ICONSET7 | 582 | 46 |
| SCENES | MAP8 | ICONSET7 | 479 | 46 |
| SCENET | MAP4 | ICONSET3 | 365 | 48 |
| SCENEU | MAP5 | ICONSET4 | 273 | 37 |

## Other decoded resource families

| Family | Exported material |
| --- | --- |
| CPS screens | 39 320×200 images plus a manifest with compression and palette provenance |
| Palettes | 22 palette previews/metadata files |
| Fonts | Two rendered font sheets |
| Tactical tiles | Eight terrain, eight destruction and two mech sheets |
| Digital sound | 24 decoded entries |
| PC speaker | 33 synthesized effect files |
| Scene evidence | Disassembly, messages, metadata and map renders for SCENE1..SCENEU |

Run the full extraction workflow rather than committing generated graphics or
copyrighted game data to the repository.

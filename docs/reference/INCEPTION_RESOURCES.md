# Inception maps, animations and sprites

The [combat effects reference](INCEPTION_COMBAT_EFFECTS.md) maps weapons to
graphics and sound, including the actual missile loops and common hit flashes.

This page separates verified format facts from descriptive identifications.
`V` means verified by parser/runtime evidence, `P` probable, and `H` a useful
historical identification still awaiting stronger proof.

## Maps

All standard maps have a typed header and 16×16-pixel tiles. `MAP15` is the
headerless star map. “Traversal” describes the storage-to-image conversion,
not a game-world direction.

| File | Size (tiles) | Tile set | Traversal | Description | Confidence |
| --- | ---: | --- | --- | --- | --- |
| MAP1 | 64×64 | BTTLTECH.ICN | row-major/block | Citadel and training centre | P |
| MAP2 | 64×64 | BTTLTECH.ICN | row-major/block | Starport and city | P |
| MAP3 | 32×32 | BTTLTECH.ICN | block-to-row-major | Prison | P |
| MAP4 | 32×32 | BTTLTECH.ICN | block-to-row-major | Village/local map | H |
| MAP5 | 32×32 | BTTLTECH.ICN | block-to-row-major | Village/local map | H |
| MAP6 | 32×32 | BTTLTECH.ICN | block-to-row-major | Village/local map | H |
| MAP7 | 32×32 | BTTLTECH.ICN | block-to-row-major | Village/local map | H |
| MAP8 | 32×32 | BTTLTECH.ICN | block-to-row-major | Village/local map | H |
| MAP9 | 32×32 | BTTLTECH.ICN | block-to-row-major | Village/local map | H |
| MAP10 | 32×32 | BTTLTECH.ICN | block-to-row-major | Village/local map | H |
| MAP11 | 64×64 | DESTRUCT.ICN | row-major/block | Destroyed Citadel | P |
| MAP12 | 8×8 | BTTLTECH.ICN | row-major/block | Inventor's hut | P |
| MAP13 | 8×8 | BTTLTECH.ICN | row-major/block | Cache exterior | P |
| MAP14 | 64×64 | STARLEAG.ICN | row-major/block | Star League cache | P |
| MAP15 | 32×24 | MAP.ICN | block-to-row-major | Star map | V |

## O animation catalogue

| ID | Description | Confidence |
| ---: | --- | --- |
| O0 | Mech startup, including failed-start prefix | V |
| O1 | Infantry/manpack weapon versus mech | H |
| O2 | Neurohelmet conversation | H |
| O3 | Cockpit hit | H |
| O4 | Locust firing | H |
| O5 | Neurohelmet smoking/failure | H |
| O6 | Crescent Hawks secret card | H |
| O7 | Wasp firing | H |
| O8 | Siren/alarm | H |
| O9 | Jenner step | H |
| O10 | Hyperpulse transmission | V |
| O11 | DropShip arrival | V |
| O12 | Katrina | H |
| O13 | Jason bowing | H |
| O14 | DropShip departure / The End | V |
| O15 | Lyran blast doors opening | H |
| O16 | Wasp loses an arm | P |
| O17 | Citadel secretary and banner | H |
| O18 | Mech technicians/shop | H |
| O19 | Draconis hall | H |
| O20 | Arena poster | H |
| O21 | Weapon-shop owner | H |

## Mech sprite IDs

The complete `MECHS.ICN` extraction contains 376 source sprites arranged by
the exporter into 77 named animation sequences. Use `export-mech-spritesheet`
with `--metadata FILE.json` for every frame coordinate. These individually named IDs are
the subset with preserved game-logic names:

| ID | Name |
| ---: | --- |
| `0x68` | Missile animation base |
| `0x82–0x91` | Actual missile flight frames; direction-specific loops, not debris |
| `0x78` | Locust combat right 01 |
| `0x79` | Locust combat right 02 |
| `0x7A` | Locust combat left 01 |
| `0x7B` | Locust combat left 02 |
| `0x7C` | Large fire |
| `0x7D` | Small fire |
| `0x7E` | Small impact |
| `0x7F` | Large impact |
| `0x80` | Locust wreck |
| `0x81` | Commando wreck |
| `0xA2` | Commando combat forward |
| `0xA3` | Commando combat rear |
| `0xA4` | Commando combat right |
| `0xA5` | Commando combat left |
| `0xFA` | Projectile-impact animation base |
| `0x176/0x177` | Actual common combat hit flashes; not fallen infantry |

IDs not listed here are not unknown bytes: they are still available in the
generated sequence metadata, but do not yet have equally strong individual
semantic names. This avoids promoting guessed labels into format guarantees.

Two existing export-group names are historical misidentifications:
`effects.debris` contains the verified missile frames `0x82–0x91`, and
`infantry.fallen` contains the common hit flashes `0x176/0x177`. The exporter
names remain unchanged; use source sprite IDs and the verified playback loops
in the combat-effects page. Likewise, base `0xFA` plus stream frames
`0x7C,0x7D,0x7C` produces `0x176,0x177,0x176`; it does not draw the raw
fire sprites `0x7C/0x7D` at that call site.

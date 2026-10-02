# Inception characters, armour and skills

## Character IDs

The character record stores a one-byte name ID. These eleven names are the
complete executable table.

| ID | Name | ID | Name |
| ---: | --- | ---: | --- |
| 0 | Jason | 6 | Possum |
| 1 | Rex | 7 | Marco |
| 2 | Edward | 8 | Rusty |
| 3 | Russ | 9 | Hunter |
| 4 | Rick | 10 | Hawk |
| 5 | Zeke |  |  |

## Character record

Each infantry character occupies `0x11` bytes.

| Offset | Size | Meaning |
| ---: | ---: | --- |
| `0x00` | 1 | Character/name ID |
| `0x01` | 1 each | Body, dexterity and charisma |
| `0x04` | 7 | Skills in the order below |
| `0x0B` | 1 | Weapon-table index |
| `0x0C` | 1 | Mech assignment |
| `0x0D` | 1 | Armour type |
| `0x0E` | 1 | Current armour value |
| `0x0F` | 1 | Health |
| `0x10` | 1 | Training flags |

Training-flags bit 0 is Tech and bit 1 is Medical. The remaining bits are not
yet named by sufficient evidence.

## Infantry armour

| ID | Armour | Durability | Purchase cost | Repair cost/point |
| ---: | --- | ---: | ---: | ---: |
| 0 | None | 0 | — | — |
| 1 | Flak Vest | 25 | 50 | 1 |
| 2 | Flak Suit | 40 | 150 | 2 |
| 3 | Lt Env Suit | 30 | 200 | 5 |
| 4 | Hv Env Suit | 50 | 10,000 | 20 |
| 5 | Ablative | 50 | 1,000 | 20 |

## Skills and training

| Skill ID | Skill | Normal school training | Specialist training |
| ---: | --- | --- | --- |
| 0 | Bows and blades | yes | — |
| 1 | Pistol | yes | — |
| 2 | Rifle | yes | — |
| 3 | Gunnery | no | — |
| 4 | Piloting | no | — |
| 5 | Tech | no | 500 C-Bills |
| 6 | Medical | no | 500 C-Bills |

Displayed skill levels are 0 completely unskilled, 1 amateur, 2 competent,
3 whiz and 4 grand master. The normal school teaches only skills 0–2. Its
verified cost formula is `75 + 125 × current level`.

| Advance | Cost |
| --- | ---: |
| Level 0 → 1 | 75 |
| Level 1 → 2 | 200 |
| Level 2 → 3 | 325 |
| Level 3 → 4 | 450 |
| Already level 4 | refused |

The weapon-to-skill relationship is shown in
[Inception weapons and ranges](INCEPTION_WEAPONS.md).

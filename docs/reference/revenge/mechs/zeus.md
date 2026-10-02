# Zeus

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x31` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 80 |
| Walking movement | 4 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0xB0` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 22 | 22 |
| Left Torso | 0 | 18 | 18 |
| Center Torso | 0 | 26 | 26 |
| Right Torso | 0 | 18 | 18 |
| Right Arm | 0 | 22 | 22 |
| Left Leg | 0 | 24 | 24 |
| Right Leg | 0 | 24 | 24 |
| Rear Left Torso | 0 | 6 | 6 |
| Rear Center Torso | 0 | 9 | 9 |
| Rear Right Torso | 0 | 6 | 6 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 13 | 13 |
| Left Torso | 0 | 17 | 17 |
| Center Torso | 0 | 25 | 25 |
| Right Torso | 0 | 17 | 17 |
| Right Arm | 0 | 13 | 13 |
| Left Leg | 0 | 17 | 17 |
| Right Leg | 0 | 17 | 17 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 10 | stored capacity |
| Critical-slot heat sinks | 7 | equipment entries |
| Gunnery target number | 4 | lower is better |
| Target movement modifier | 0 | stored tactical modifier |

## Special equipment

| Equipment/state | Present |
| --- | --- |
| Improved missiles | False |
| Inferno rockets | False |
| Beagle active probe | False |
| Indirect-fire spotting | False |
| Accuracy upgrade | False |
| Double heat sinks | False |
| CASE | False |

## Weapons

| # | Weapon | Location | Equipment | State | Damage | Heat | Range min/max |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | AC/5 | Left Arm | `0x0B` | Operational | 5 | 1 | 3/18 |
| 1 | Large Laser | Left Torso | `0x05` | Operational | 8 | 8 | 0/15 |
| 2 | Medium Laser | Left Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 3 | Medium Laser | Center Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 4 | LRM-15 | Right Arm | `0x13` | Operational | 11 | 5 | 6/21 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 20 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 8 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

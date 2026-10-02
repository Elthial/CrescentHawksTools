# Dragon

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x1F` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 60 |
| Walking movement | 5 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x40` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 14 | 14 |
| Left Torso | 0 | 16 | 16 |
| Center Torso | 0 | 27 | 27 |
| Right Torso | 0 | 16 | 16 |
| Right Arm | 0 | 14 | 14 |
| Left Leg | 0 | 18 | 18 |
| Right Leg | 0 | 18 | 18 |
| Rear Left Torso | 0 | 8 | 8 |
| Rear Center Torso | 0 | 12 | 12 |
| Rear Right Torso | 0 | 8 | 8 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 10 | 10 |
| Left Torso | 0 | 14 | 14 |
| Center Torso | 0 | 20 | 20 |
| Right Torso | 0 | 14 | 14 |
| Right Arm | 0 | 10 | 10 |
| Left Leg | 0 | 14 | 14 |
| Right Leg | 0 | 14 | 14 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 10 | stored capacity |
| Critical-slot heat sinks | 0 | equipment entries |
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
| 0 | Medium Laser | Left Arm | `0x03` | Operational | 5 | 3 | 0/9 |
| 1 | Medium Laser | Left Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 2 | LRM-10 | Center Torso | `0x12` | Operational | 7 | 4 | 6/21 |
| 3 | AC/5 | Right Arm | `0x0B` | Operational | 5 | 1 | 3/18 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 40 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 24 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

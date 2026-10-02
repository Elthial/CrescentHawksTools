# Atlas

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x36` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 100 |
| Walking movement | 3 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0xB0` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 34 | 34 |
| Left Torso | 0 | 32 | 32 |
| Center Torso | 0 | 47 | 47 |
| Right Torso | 0 | 32 | 32 |
| Right Arm | 0 | 34 | 34 |
| Left Leg | 0 | 41 | 41 |
| Right Leg | 0 | 41 | 41 |
| Rear Left Torso | 0 | 10 | 10 |
| Rear Center Torso | 0 | 14 | 14 |
| Rear Right Torso | 0 | 10 | 10 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 17 | 17 |
| Left Torso | 0 | 21 | 21 |
| Center Torso | 0 | 31 | 31 |
| Right Torso | 0 | 21 | 21 |
| Right Arm | 0 | 17 | 17 |
| Left Leg | 0 | 21 | 21 |
| Right Leg | 0 | 21 | 21 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 10 | stored capacity |
| Critical-slot heat sinks | 10 | equipment entries |
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
| 1 | LRM-20 | Left Torso | `0x14` | Operational | 15 | 6 | 6/21 |
| 2 | SRM-6 | Left Torso | `0x17` | Operational | 9 | 4 | 0/9 |
| 3 | Medium Laser | Center Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 4 | Medium Laser | Center Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 5 | AC/20 | Right Torso | `0x0F` | Operational | 20 | 7 | 0/9 |
| 6 | Medium Laser | Right Arm | `0x03` | Operational | 5 | 3 | 0/9 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 10 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 12 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 15 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

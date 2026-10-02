# Black Hawk

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x43` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 50 |
| Walking movement | 5 |
| Jump movement | 5 |
| Damage model | BattleMech |
| Tactical sprite base | `0x80` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 16 | 16 |
| Left Torso | 0 | 15 | 15 |
| Center Torso | 0 | 23 | 23 |
| Right Torso | 0 | 15 | 15 |
| Right Arm | 0 | 16 | 16 |
| Left Leg | 0 | 20 | 20 |
| Right Leg | 0 | 20 | 20 |
| Rear Left Torso | 0 | 7 | 7 |
| Rear Center Torso | 0 | 8 | 8 |
| Rear Right Torso | 0 | 7 | 7 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 8 | 8 |
| Left Torso | 0 | 12 | 12 |
| Center Torso | 0 | 16 | 16 |
| Right Torso | 0 | 12 | 12 |
| Right Arm | 0 | 8 | 8 |
| Left Leg | 0 | 12 | 12 |
| Right Leg | 0 | 12 | 12 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 20 | stored capacity |
| Critical-slot heat sinks | 8 | equipment entries |
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
| Double heat sinks | True |
| CASE | False |

## Weapons

| # | Weapon | Location | Equipment | State | Damage | Heat | Range min/max |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Gauss Rifle | Left Arm | `0x0E` | Operational | 15 | 1 | 2/21 |
| 1 | SRM-4 | Left Torso | `0x16` | Operational | 6 | 3 | 0/9 |
| 2 | Sm Pulse | Right Torso | `0x02` | Operational | 3 | 2 | 0/6 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 8 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 25 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

# Mad Cat

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x44` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 75 |
| Walking movement | 5 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x90` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 24 | 24 |
| Left Torso | 0 | 25 | 25 |
| Center Torso | 0 | 36 | 36 |
| Right Torso | 0 | 25 | 25 |
| Right Arm | 0 | 24 | 24 |
| Left Leg | 0 | 32 | 32 |
| Right Leg | 0 | 32 | 32 |
| Rear Left Torso | 0 | 7 | 7 |
| Rear Center Torso | 0 | 9 | 9 |
| Rear Right Torso | 0 | 7 | 7 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 12 | 12 |
| Left Torso | 0 | 16 | 16 |
| Center Torso | 0 | 23 | 23 |
| Right Torso | 0 | 16 | 16 |
| Right Arm | 0 | 12 | 12 |
| Left Leg | 0 | 16 | 16 |
| Right Leg | 0 | 16 | 16 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 18 | stored capacity |
| Critical-slot heat sinks | 12 | equipment entries |
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
| 0 | ER PPC | Left Arm | `0x09` | Operational | 15 | 15 | 0/21 |
| 1 | Med Pulse | Left Torso | `0x04` | Operational | 7 | 4 | 0/12 |
| 2 | Med Pulse | Left Torso | `0x04` | Operational | 7 | 4 | 0/12 |
| 3 | Med Pulse | Left Torso | `0x04` | Operational | 7 | 4 | 0/12 |
| 4 | SRM-6 | Right Torso | `0x17` | Operational | 9 | 4 | 0/9 |
| 5 | ER PPC | Right Arm | `0x09` | Operational | 15 | 15 | 0/21 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 15 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

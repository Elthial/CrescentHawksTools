# Griffin+

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x4E` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 55 |
| Walking movement | 5 |
| Jump movement | 5 |
| Damage model | BattleMech |
| Tactical sprite base | `0x60` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 18 | 18 |
| Left Torso | 0 | 21 | 21 |
| Center Torso | 0 | 23 | 23 |
| Right Torso | 0 | 21 | 21 |
| Right Arm | 0 | 18 | 18 |
| Left Leg | 0 | 20 | 20 |
| Right Leg | 0 | 20 | 20 |
| Rear Left Torso | 0 | 7 | 7 |
| Rear Center Torso | 0 | 8 | 8 |
| Rear Right Torso | 0 | 7 | 7 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 9 | 9 |
| Left Torso | 0 | 13 | 13 |
| Center Torso | 0 | 18 | 18 |
| Right Torso | 0 | 13 | 13 |
| Right Arm | 0 | 9 | 9 |
| Left Leg | 0 | 13 | 13 |
| Right Leg | 0 | 13 | 13 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 16 | stored capacity |
| Critical-slot heat sinks | 4 | equipment entries |
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
| CASE | True |

## Weapons

| # | Weapon | Location | Equipment | State | Damage | Heat | Range min/max |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Small Laser | Left Arm | `0x01` | Operational | 3 | 1 | 0/3 |
| 1 | LRM-20 | Right Torso | `0x14` | Operational | 15 | 6 | 6/21 |
| 2 | ER PPC | Right Arm | `0x09` | Operational | 15 | 15 | 0/21 |

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
| LRM-20 | 0 | 12 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

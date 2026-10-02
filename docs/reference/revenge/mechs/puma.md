# Puma

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x42` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 35 |
| Walking movement | 6 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x70` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 12 | 12 |
| Left Torso | 0 | 12 | 12 |
| Center Torso | 0 | 16 | 16 |
| Right Torso | 0 | 12 | 12 |
| Right Arm | 0 | 12 | 12 |
| Left Leg | 0 | 14 | 14 |
| Right Leg | 0 | 14 | 14 |
| Rear Left Torso | 0 | 4 | 4 |
| Rear Center Torso | 0 | 6 | 6 |
| Rear Right Torso | 0 | 4 | 4 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 6 | 6 |
| Left Torso | 0 | 8 | 8 |
| Center Torso | 0 | 11 | 11 |
| Right Torso | 0 | 8 | 8 |
| Right Arm | 0 | 6 | 6 |
| Left Leg | 0 | 8 | 8 |
| Right Leg | 0 | 8 | 8 |

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
| CASE | False |

## Weapons

| # | Weapon | Location | Equipment | State | Damage | Heat | Range min/max |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | LRM-20 | Left Arm | `0x14` | Operational | 15 | 6 | 6/21 |
| 1 | Sm Pulse | Left Torso | `0x02` | Operational | 3 | 2 | 0/6 |
| 2 | Small Laser | Center Torso | `0x01` | Operational | 3 | 1 | 0/3 |
| 3 | Sm Pulse | Right Torso | `0x02` | Operational | 3 | 2 | 0/6 |
| 4 | LRM-20 | Right Arm | `0x14` | Operational | 15 | 6 | 6/21 |

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
| LRM-20 | 0 | 24 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

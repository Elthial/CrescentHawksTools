# Scorpion

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x1D` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 55 |
| Walking movement | 6 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x60` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 8 | 8 |
| Left Arm | 0 | 10 | 10 |
| Left Torso | 0 | 11 | 11 |
| Center Torso | 0 | 24 | 24 |
| Right Torso | 0 | 11 | 11 |
| Right Arm | 0 | 10 | 10 |
| Left Leg | 0 | 10 | 10 |
| Right Leg | 0 | 10 | 10 |
| Rear Left Torso | 0 | 5 | 5 |
| Rear Center Torso | 0 | 8 | 8 |
| Rear Right Torso | 0 | 5 | 5 |

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
| 0 | PPC | Right Torso | `0x08` | Operational | 10 | 10 | 3/18 |
| 1 | SRM-6 | Right Torso | `0x17` | Operational | 9 | 4 | 0/9 |

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

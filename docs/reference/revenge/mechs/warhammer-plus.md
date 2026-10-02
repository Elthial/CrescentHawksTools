# Warhammer+

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x52` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 70 |
| Walking movement | 4 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x30` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 20 | 20 |
| Left Torso | 0 | 17 | 17 |
| Center Torso | 0 | 22 | 22 |
| Right Torso | 0 | 17 | 17 |
| Right Arm | 0 | 20 | 20 |
| Left Leg | 0 | 15 | 15 |
| Right Leg | 0 | 15 | 15 |
| Rear Left Torso | 0 | 8 | 8 |
| Rear Center Torso | 0 | 9 | 9 |
| Rear Right Torso | 0 | 8 | 8 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 11 | 11 |
| Left Torso | 0 | 15 | 15 |
| Center Torso | 0 | 22 | 22 |
| Right Torso | 0 | 15 | 15 |
| Right Arm | 0 | 11 | 11 |
| Left Leg | 0 | 15 | 15 |
| Right Leg | 0 | 15 | 15 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 28 | stored capacity |
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
| 0 | ER PPC | Left Arm | `0x09` | Operational | 15 | 15 | 0/21 |
| 1 | Med Pulse | Left Torso | `0x04` | Operational | 7 | 4 | 0/12 |
| 2 | SRM-6 | Right Torso | `0x17` | Operational | 9 | 4 | 0/9 |
| 3 | Med Pulse | Right Torso | `0x04` | Operational | 7 | 4 | 0/12 |
| 4 | ER PPC | Right Arm | `0x09` | Operational | 15 | 15 | 0/21 |

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
| SRM-6 | 0 | 30 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

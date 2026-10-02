# Orion

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x2C` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 75 |
| Walking movement | 4 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0x30` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 20 | 20 |
| Left Torso | 0 | 21 | 21 |
| Center Torso | 0 | 34 | 34 |
| Right Torso | 0 | 21 | 21 |
| Right Arm | 0 | 20 | 20 |
| Left Leg | 0 | 32 | 32 |
| Right Leg | 0 | 32 | 32 |
| Rear Left Torso | 0 | 9 | 9 |
| Rear Center Torso | 0 | 9 | 9 |
| Rear Right Torso | 0 | 9 | 9 |

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
| 1 | LRM-15 | Left Torso | `0x13` | Operational | 11 | 5 | 6/21 |
| 2 | SRM-4 | Left Torso | `0x16` | Operational | 6 | 3 | 0/9 |
| 3 | AC/10 | Right Torso | `0x0C` | Operational | 10 | 3 | 0/15 |
| 4 | Medium Laser | Right Arm | `0x03` | Operational | 5 | 3 | 0/9 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 20 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 16 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 50 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

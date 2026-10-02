# Cyclops

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x34` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 90 |
| Walking movement | 4 |
| Jump movement | 0 |
| Damage model | BattleMech |
| Tactical sprite base | `0xB0` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 9 | 9 |
| Left Arm | 0 | 10 | 10 |
| Left Torso | 0 | 20 | 20 |
| Center Torso | 0 | 30 | 30 |
| Right Torso | 0 | 20 | 20 |
| Right Arm | 0 | 10 | 10 |
| Left Leg | 0 | 17 | 17 |
| Right Leg | 0 | 17 | 17 |
| Rear Left Torso | 0 | 8 | 8 |
| Rear Center Torso | 0 | 11 | 11 |
| Rear Right Torso | 0 | 8 | 8 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 15 | 15 |
| Left Torso | 0 | 19 | 19 |
| Center Torso | 0 | 29 | 29 |
| Right Torso | 0 | 19 | 19 |
| Right Arm | 0 | 15 | 15 |
| Left Leg | 0 | 19 | 19 |
| Right Leg | 0 | 19 | 19 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 10 | stored capacity |
| Critical-slot heat sinks | 2 | equipment entries |
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
| 1 | LRM-10 | Left Torso | `0x12` | Operational | 7 | 4 | 6/21 |
| 2 | SRM-4 | Center Torso | `0x16` | Operational | 6 | 3 | 0/9 |
| 3 | AC/20 | Right Torso | `0x0F` | Operational | 20 | 7 | 0/9 |
| 4 | Medium Laser | Right Arm | `0x03` | Operational | 5 | 3 | 0/9 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 0 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 20 | Not stored separately |
| Machine Gun | 0 | 0 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 12 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 25 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

# Vulcan

[Back to Revenge BattleMech record layout and index](../../REVENGE_UNITS.md)

## Overview

| Field | Value |
| --- | ---: |
| Unit type ID | `0x10` |
| Record size | `0x96` (150 bytes) |
| Tonnage | 40 |
| Walking movement | 6 |
| Jump movement | 6 |
| Damage model | BattleMech |
| Tactical sprite base | `0xA0` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 8 | 8 |
| Left Arm | 0 | 6 | 6 |
| Left Torso | 0 | 8 | 8 |
| Center Torso | 0 | 10 | 10 |
| Right Torso | 0 | 8 | 8 |
| Right Arm | 0 | 6 | 6 |
| Left Leg | 0 | 10 | 10 |
| Right Leg | 0 | 10 | 10 |
| Rear Left Torso | 0 | 4 | 4 |
| Rear Center Torso | 0 | 6 | 6 |
| Rear Right Torso | 0 | 4 | 4 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Head | 0 | 3 | 3 |
| Left Arm | 0 | 6 | 6 |
| Left Torso | 0 | 10 | 10 |
| Center Torso | 0 | 12 | 12 |
| Right Torso | 0 | 10 | 10 |
| Right Arm | 0 | 6 | 6 |
| Left Leg | 0 | 10 | 10 |
| Right Leg | 0 | 10 | 10 |

## Systems

| System | Current/template value | Limit / meaning |
| --- | ---: | --- |
| Engine hits | 0 | 3 destroys |
| Sensor hits | 0 | 2 destroys |
| Current heat | 0 | 30 maximum engine scale |
| Engine heat-sink capacity | 9 | stored capacity |
| Critical-slot heat sinks | 1 | equipment entries |
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
| 0 | Mach. Gun | Left Arm | `0x10` | Operational | 2 | 0 | 0/3 |
| 1 | Medium Laser | Left Torso | `0x03` | Operational | 5 | 3 | 0/9 |
| 2 | AC/2 | Right Torso | `0x0A` | Operational | 2 | 1 | 4/24 |
| 3 | Small Laser | Right Arm | `0x01` | Operational | 3 | 1 | 0/3 |

## Ammunition pools

| Family | Minimum | Current template | Maximum |
| --- | ---: | ---: | --- |
| AC/2 | 0 | 45 | Not stored separately |
| AC/5 | 0 | 0 | Not stored separately |
| AC/10 | 0 | 0 | Not stored separately |
| LB-10X A/C | 0 | 0 | Not stored separately |
| Gauss Rifle | 0 | 0 | Not stored separately |
| AC/20 | 0 | 0 | Not stored separately |
| Machine Gun | 0 | 200 | Not stored separately |
| LRM-5 | 0 | 0 | Not stored separately |
| LRM-10 | 0 | 0 | Not stored separately |
| LRM-15 | 0 | 0 | Not stored separately |
| LRM-20 | 0 | 0 | Not stored separately |
| SRM-2 | 0 | 0 | Not stored separately |
| SRM-4 | 0 | 0 | Not stored separately |
| SRM-6 | 0 | 0 | Not stored separately |

> Revenge stores the current ammunition pool but no separate maximum-ammo array. For a pristine `MECHTYPE.DAT` template, “current template” is the starting load; this page does not invent an unrecorded maximum.

# UrbanMech - UM-R60

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The stock game UrbanMech is the [UM-R60](https://www.sarna.net/wiki/UrbanMech): a slow, heavily armoured 30-ton urban defender with an AC/10, Small Laser and two jump jets. This page preserves the executable's unusual current/max ammunition discrepancy.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `7` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 30 |
| Walking movement | 2 |
| Jump movement | 2 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 6 |
| Upgrade flags | `0x00` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 10 | 10 |
| Left Torso | 0 | 8 | 8 |
| Left Leg | 0 | 12 | 12 |
| Head | 0 | 9 | 9 |
| Center Torso | 0 | 11 | 11 |
| Right Arm | 0 | 10 | 10 |
| Right Torso | 0 | 8 | 8 |
| Right Leg | 0 | 12 | 12 |
| Rear Left Torso | 0 | 4 | 4 |
| Rear Center Torso | 0 | 8 | 8 |
| Rear Right Torso | 0 | 4 | 4 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 5 | 5 |
| Left Torso | 0 | 7 | 7 |
| Left Leg | 0 | 7 | 7 |
| Head | 0 | 3 | 3 |
| Center Torso | 0 | 10 | 10 |
| Right Arm | 0 | 5 | 5 |
| Right Torso | 0 | 7 | 7 |
| Right Leg | 0 | 7 | 7 |

## Actuators and systems

| System | Current | Maximum / destroyed at |
| --- | ---: | ---: |
| Left leg actuator nibble | 15 | 15 |
| Right leg actuator nibble | 15 | 15 |
| Left arm actuator nibble | 12 | 12 |
| Right arm actuator nibble | 12 | 12 |
| Engine hits | 0 | 3 destroys |
| Gyro hits | 0 | 2 destroys |
| Sensor hits | 0 | 2 destroys |
| Built-in heat sinks | 2 | — |
| Critical-slot heat sinks | 8 | — |
| Life-support state (raw) | 1 | — |

## Weapons

| # | Weapon | Location | Component | State | Damage | Heat | Range S/M/L |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Small Laser | Left Arm | `0x10` | Operational | 3 | 1 | 6/9/12 |
| 1 | Autocannon/10 | Right Arm | `0x16` | Operational | 10 | 3 | 18/33/48 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Autocannon/10 | 0 | 20 | 20 |
| 2 | Unassigned | 0 | 0 | Unlimited (`0xFF`) |
| 3 | Unassigned | 0 | 0 | Unlimited (`0xFF`) |
| 4 | Unassigned | 0 | 0 | 25 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

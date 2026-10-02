# Stinger - STG-3R

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The stock game Stinger matches the [STG-3R](https://www.sarna.net/wiki/Stinger): a jump-capable 20-ton scout carrying one Medium Laser and two Machine Guns. This page records the unmodified executable template.

## Mech-Lube variants

- [Stinger - STG-3Y](Stinger%20-%20STG-3Y.md)
- [Stinger - STG-3Y2](Stinger%20-%20STG-3Y2.md)

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `2` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 6 |
| Jump movement | 6 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 2 |
| Upgrade flags | `0x00` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 4 | 4 |
| Left Torso | 0 | 6 | 6 |
| Left Leg | 0 | 5 | 5 |
| Head | 0 | 4 | 4 |
| Center Torso | 0 | 6 | 6 |
| Right Arm | 0 | 4 | 4 |
| Right Torso | 0 | 6 | 6 |
| Right Leg | 0 | 5 | 5 |
| Rear Left Torso | 0 | 2 | 2 |
| Rear Center Torso | 0 | 4 | 4 |
| Rear Right Torso | 0 | 2 | 2 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 3 | 3 |
| Left Torso | 0 | 5 | 5 |
| Left Leg | 0 | 4 | 4 |
| Head | 0 | 3 | 3 |
| Center Torso | 0 | 6 | 6 |
| Right Arm | 0 | 3 | 3 |
| Right Torso | 0 | 5 | 5 |
| Right Leg | 0 | 4 | 4 |

## Actuators and systems

| System | Current | Maximum / destroyed at |
| --- | ---: | ---: |
| Left leg actuator nibble | 15 | 15 |
| Right leg actuator nibble | 15 | 15 |
| Left arm actuator nibble | 15 | 15 |
| Right arm actuator nibble | 15 | 15 |
| Engine hits | 0 | 3 destroys |
| Gyro hits | 0 | 2 destroys |
| Sensor hits | 0 | 2 destroys |
| Built-in heat sinks | 4 | — |
| Critical-slot heat sinks | 6 | — |
| Life-support state (raw) | 1 | — |

## Weapons

| # | Weapon | Location | Component | State | Damage | Heat | Range S/M/L |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Machine Gun | Left Arm | `0x18` | Operational | 2 | 0 | 6/9/12 |
| 1 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 2 | Machine Gun | Right Arm | `0x18` | Operational | 2 | 0 | 6/9/12 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Machine Gun | 0 | 100 | 100 |
| 1 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Machine Gun | 0 | 100 | 100 |
| 3 | Unassigned | 0 | 0 | 0 |
| 4 | Unassigned | 0 | 0 | 0 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

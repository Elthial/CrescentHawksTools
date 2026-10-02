# Jenner - JR7-D

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The stock game Jenner is the [JR7-D](https://www.sarna.net/wiki/Jenner): a fast 35-ton striker with four Medium Lasers, an SRM-4 and five jump jets. This page records the unmodified executable template.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `5` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 35 |
| Walking movement | 7 |
| Jump movement | 5 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 9 |
| Upgrade flags | `0x00` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 4 | 4 |
| Left Torso | 0 | 8 | 8 |
| Left Leg | 0 | 6 | 6 |
| Head | 0 | 7 | 7 |
| Center Torso | 0 | 10 | 10 |
| Right Arm | 0 | 4 | 4 |
| Right Torso | 0 | 8 | 8 |
| Right Leg | 0 | 6 | 6 |
| Rear Left Torso | 0 | 4 | 4 |
| Rear Center Torso | 0 | 3 | 3 |
| Rear Right Torso | 0 | 4 | 4 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 6 | 6 |
| Left Torso | 0 | 8 | 8 |
| Left Leg | 0 | 8 | 8 |
| Head | 0 | 3 | 3 |
| Center Torso | 0 | 11 | 11 |
| Right Arm | 0 | 6 | 6 |
| Right Torso | 0 | 8 | 8 |
| Right Leg | 0 | 8 | 8 |

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
| Built-in heat sinks | 10 | — |
| Critical-slot heat sinks | 0 | — |
| Life-support state (raw) | 1 | — |

## Weapons

| # | Weapon | Location | Component | State | Damage | Heat | Range S/M/L |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Medium Laser | Left Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 1 | Medium Laser | Left Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 2 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 3 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 4 | SRM-4 | Center Torso | `0x1F` | Operational | 2 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 4 | SRM-4 | 0 | 25 | 25 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

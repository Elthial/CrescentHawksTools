# Spectator - Internal Record

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

Spectator is not a BattleTech chassis variant. It is a game-internal, Locust-derived arena observer record with deliberately minimal current armour and structure, preserved here because it occupies a normal ``0x7D`` template slot.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `6` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 0 |
| Jump movement | 0 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 0 |
| Upgrade flags | `0x00` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 0 | 4 |
| Left Torso | 0 | 0 | 8 |
| Left Leg | 0 | 0 | 8 |
| Head | 0 | 1 | 8 |
| Center Torso | 0 | 1 | 10 |
| Right Arm | 0 | 0 | 4 |
| Right Torso | 0 | 0 | 8 |
| Right Leg | 0 | 0 | 8 |
| Rear Left Torso | 0 | 1 | 2 |
| Rear Center Torso | 0 | 1 | 2 |
| Rear Right Torso | 0 | 1 | 2 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 0 | 3 |
| Left Torso | 0 | 1 | 5 |
| Left Leg | 0 | 0 | 4 |
| Head | 0 | 1 | 3 |
| Center Torso | 0 | 1 | 6 |
| Right Arm | 0 | 0 | 3 |
| Right Torso | 0 | 1 | 5 |
| Right Leg | 0 | 0 | 4 |

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
| Built-in heat sinks | 6 | — |
| Critical-slot heat sinks | 4 | — |
| Life-support state (raw) | 1 | — |

## Weapons

| # | Weapon | Location | Component | State | Damage | Heat | Range S/M/L |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Machine Gun | Left Arm | `0x18` | Operational | 2 | 0 | 6/9/12 |
| 1 | Machine Gun | Right Arm | `0x18` | Operational | 2 | 0 | 6/9/12 |
| 2 | Medium Laser | Center Torso | `0x11` | Operational | 5 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Machine Gun | 0 | 0 | 100 |
| 1 | Machine Gun | 0 | 0 | 100 |
| 2 | Medium Laser | 0 | 0 | Unlimited (`0xFF`) |
| 3 | Unassigned | 0 | 0 | 0 |
| 4 | Unassigned | 0 | 0 | 0 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

> Spectator is an inert arena pseudo-unit derived from the Locust template. Its zero movement and current ammunition are intentional runtime state; its higher maximum fields are not player-usable equipment.

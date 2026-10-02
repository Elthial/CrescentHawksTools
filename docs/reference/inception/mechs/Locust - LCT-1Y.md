# Locust - LCT-1Y

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

Mech-Lube converts the machine-gun LCT-1V into the energy-armed LCT-1Y. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Locust); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| LCT-1V | LCT-1Y | 11,000 C-bills |

This is the first package in the chassis upgrade sequence.

### Changes from LCT-1V

- Replaces both arm-mounted Machine Guns with Medium Lasers.
- Changes ammunition ordinals 0 and 1 to the unlimited energy-weapon value.
- Retains the original armour and 8/12 ground movement.
- Final armament: three Medium Lasers.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `0` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 8 |
| Jump movement | 0 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 0 |
| Upgrade flags | `0x01` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 4 | 4 |
| Left Torso | 0 | 8 | 8 |
| Left Leg | 0 | 8 | 8 |
| Head | 0 | 8 | 8 |
| Center Torso | 0 | 10 | 10 |
| Right Arm | 0 | 4 | 4 |
| Right Torso | 0 | 8 | 8 |
| Right Leg | 0 | 8 | 8 |
| Rear Left Torso | 0 | 2 | 2 |
| Rear Center Torso | 0 | 2 | 2 |
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
| 0 | Medium Laser | Left Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 1 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 2 | Medium Laser | Center Torso | `0x11` | Operational | 5 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Unassigned | 0 | 0 | 0 |
| 4 | Unassigned | 0 | 0 | 0 |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

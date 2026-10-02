# Stinger - STG-3Y2

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The second Mech-Lube package turns the STG-3Y into the armoured, seven-laser STG-3Y2. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Stinger); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| STG-3Y | STG-3Y2 | 13,200 C-bills |

This is the second package; the first package must already be installed and is paid for separately.

### Changes from STG-3Y

- Removes all six jump jets, reducing jump movement from 6 to 0.
- Replaces current and maximum armour with the LCT-1V profile.
- Adds two Medium Lasers in the center torso.
- Changes ammunition ordinals 5 and 6 to the unlimited energy-weapon value.
- Final armament: three Medium Lasers and four Small Lasers.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `2` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 6 |
| Jump movement | 0 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 2 |
| Upgrade flags | `0x03` |

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
| 0 | Small Laser | Left Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 1 | Small Laser | Left Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 2 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 3 | Small Laser | Right Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 4 | Small Laser | Right Arm | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 5 | Medium Laser | Center Torso | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 6 | Medium Laser | Center Torso | `0x11` | Operational | 5 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 4 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 5 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 6 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

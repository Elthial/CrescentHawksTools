# Locust - LCT-1Y2

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The second Mech-Lube package turns the LCT-1Y into the slower, more heavily armed LCT-1Y2. The designation is cross-checked against [Sarna](https://www.sarna.net/wiki/Locust); the record values below come from the game executable.

## Upgrade

| From | To | Price |
| --- | --- | ---: |
| LCT-1Y | LCT-1Y2 | 15,800 C-bills |

This is the second package; the first package must already be installed and is paid for separately.

### Changes from LCT-1Y

- Installs one Small Laser in each side torso.
- Reduces walking movement from 8 to 7 after fitting a salvaged Panther engine.
- Clears the engine-hit counter when the replacement engine is installed.
- Final armament: three Medium Lasers and two Small Lasers.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `0` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 20 |
| Walking movement | 7 |
| Jump movement | 0 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 0 |
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
| 1 | Small Laser | Left Torso | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 2 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 3 | Small Laser | Right Torso | `0x10` | Operational | 3 | 1 | 9/18/27 |
| 4 | Medium Laser | Center Torso | `0x11` | Operational | 5 | 3 | 12/21/30 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 4 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 5 | Unassigned | 0 | 0 | 0 |
| 6 | Unassigned | 0 | 0 | 0 |
| 7 | Unassigned | 0 | 0 | 0 |
| 8 | Unassigned | 0 | 0 | 0 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

# Chameleon - TRC-4B

[Back to Inception BattleMech record layout and index](../../INCEPTION_MECHS.md)

The game Chameleon matches the older [TRC-4B training configuration](https://www.sarna.net/wiki/Chameleon), including four Small Lasers rather than the later CLN-7V's three. Its deliberately reduced internal structure makes the scripted Jenner attack destroy it quickly; those reduced values also carry into save data.

> **Preservation note:** the unusually low internal-structure values are exact
> executable data, not a decoding or transcription error. Both the current
> structure field at `2FE8:0500` and maximum structure field at `2FE8:0545`
> contain `00 00 00 01 01 00 00 00`. This is the special Chameleon template
> selected by the training-mission path, not a normalized tabletop Chameleon
> record. Its internal structure is deliberately reduced so the attacking
> Jenners can quickly destroy the otherwise heavily armoured 50-ton Chameleon
> during the scripted Kurita assault. The same reduced current and maximum
> structure arrays carry through into saved Chameleon records.

## Overview

| Field | Value |
| --- | ---: |
| Template ID | `4` |
| Record size | `0x7D` (125 bytes) |
| Tonnage | 50 |
| Walking movement | 6 |
| Jump movement | 6 |
| Pilot ID | 0 |
| Rider ID | 255 |
| Upgrade-package base | 200 |
| Upgrade flags | `0x00` |

## Armour

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 10 | 10 |
| Left Torso | 0 | 10 | 10 |
| Left Leg | 0 | 12 | 12 |
| Head | 0 | 9 | 9 |
| Center Torso | 0 | 16 | 16 |
| Right Arm | 0 | 10 | 10 |
| Right Torso | 0 | 10 | 10 |
| Right Leg | 0 | 12 | 12 |
| Rear Left Torso | 0 | 2 | 2 |
| Rear Center Torso | 0 | 3 | 3 |
| Rear Right Torso | 0 | 2 | 2 |

## Internal structure

| Location | Minimum | Current | Maximum |
| --- | ---: | ---: | ---: |
| Left Arm | 0 | 0 | 0 |
| Left Torso | 0 | 0 | 0 |
| Left Leg | 0 | 0 | 0 |
| Head | 0 | 1 | 1 |
| Center Torso | 0 | 1 | 1 |
| Right Arm | 0 | 0 | 0 |
| Right Torso | 0 | 0 | 0 |
| Right Leg | 0 | 0 | 0 |

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
| Built-in heat sinks | 0 | — |
| Critical-slot heat sinks | 10 | — |
| Life-support state (raw) | 1 | — |

## Weapons

| # | Weapon | Location | Component | State | Damage | Heat | Range S/M/L |
| ---: | --- | --- | ---: | --- | ---: | ---: | --- |
| 0 | Medium Laser | Left Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 1 | Small Laser | Left Torso | `0x10` | Operational | 3 | 1 | 6/9/12 |
| 2 | Small Laser | Left Torso | `0x10` | Operational | 3 | 1 | 6/9/12 |
| 3 | Large Laser | Right Arm | `0x12` | Operational | 8 | 8 | 18/33/48 |
| 4 | Medium Laser | Right Arm | `0x11` | Operational | 5 | 3 | 12/21/30 |
| 5 | Small Laser | Right Torso | `0x10` | Operational | 3 | 1 | 6/9/12 |
| 6 | Small Laser | Right Torso | `0x10` | Operational | 3 | 1 | 6/9/12 |
| 7 | Machine Gun | Center Torso | `0x18` | Operational | 2 | 0 | 6/9/12 |
| 8 | Machine Gun | Center Torso | `0x18` | Operational | 2 | 0 | 6/9/12 |

## Ammunition state

| Weapon ordinal | Weapon | Minimum | Current | Maximum |
| ---: | --- | ---: | ---: | ---: |
| 0 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 1 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 2 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 3 | Large Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 4 | Medium Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 5 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 6 | Small Laser | 0 | Unlimited (`0xFF`) | Unlimited (`0xFF`) |
| 7 | Machine Gun | 0 | 100 | 100 |
| 8 | Machine Gun | 0 | 100 | 100 |
| 9 | Unassigned | 0 | 0 | 0 |

> Unassigned ordinals are retained because all ten bytes are part of the original record. A non-zero value in an unassigned ordinal is preserved data, not automatically usable ammunition.

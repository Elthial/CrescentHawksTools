# Revenge tactical sprite reference

The tactical sprite selector stored at unit offset `0x88` is a local base in
the combined mech-sheet address space. The exporter maps it to a global sprite
base by adding `0x100`. BattleMech/vehicle groups normally provide four
friendly and four opposing facings, spaced by two tiles; infantry uses one
tile per allegiance.

| Local base | Global base | Friendly tiles | Opposing tiles | Unit types sharing the group |
| ---: | ---: | --- | --- | --- |
| `0x00` | `0x100` | `00,02,04,06` | `08,0A,0C,0E` | Wasp, Stinger, Commando, Spider, Valkyrie, Firestarter and variants |
| `0x10` | `0x110` | `10,12,14,16` | `18,1A,1C,1E` | Locust, Javelin, Jenner, Ostscout, Cicada and variants |
| `0x20` | `0x120` | `20,22,24,26` | `28,2A,2C,2E` | Hermes II, Whitworth, Blackjack, Hatchetman and Blackjack+ |
| `0x30` | `0x130` | `30,32,34,36` | `38,3A,3C,3E` | Crusader, JagerMech, Thunderbolt, Archer, Grasshopper, Warhammer, Orion and variants |
| `0x40` | `0x140` | `40,42,44,46` | `48,4A,4C,4E` | UrbanMech, Dragon, Ostroc, Ostsol, Quickdraw, Rifleman and variants |
| `0x50` | `0x150` | `50,52,54,56` | `58,5A,5C,5E` | Phoenix Hawk, Vindicator, Centurion, Enforcer, Hunchback, Trebuchet, Mobile HQ and variants |
| `0x60` | `0x160` | `60,62,64,66` | `68,6A,6C,6E` | Dervish, Griffin, Shadow Hawk, Scorpion, Wolverine and variants |
| `0x70` | `0x170` | `70,72,74,76` | `78,7A,7C,7E` | Rommel Tank, Puma |
| `0x80` | `0x180` | `80,82,84,86` | `88,8A,8C,8E` | Pegasus, Drillson, Black Hawk |
| `0x90` | `0x190` | `90,92,94,96` | `98,9A,9C,9E` | Galleon, Skulker, Mad Cat |
| `0xA0` | `0x1A0` | `A0,A2,A4,A6` | `A8,AA,AC,AE` | Panther, Assassin, Clint, Vulcan, Catapult, Marauder, Stalker and variant |
| `0xB0` | `0x1B0` | `B0,B2,B4,B6` | `B8,BA,BC,BE` | Awesome, Charger, Goliath, Victor, Zeus, BattleMaster, Cyclops, Banshee, Atlas, APC and variant |
| `0xD0` | `0x1D0` | `D0,D2,D4,D6` | `D8,DA,DC,DE` | Ammo Carrier |
| `0xD6` | `0x1D6` | `D6` | `D8` | Elementals |
| `0xD8` | `0x1D8` | `D8` | `DA` | Infantry, Jump Infantry |

These are selector groups, not unique chassis art IDs. Several chassis share a
silhouette group by design. `Export-AllAssets.ps1 -Game Revenge` writes
`unit-sprites.json` so code can consume the association without scraping this
Markdown table.

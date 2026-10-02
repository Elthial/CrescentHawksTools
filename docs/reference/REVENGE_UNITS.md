# Revenge BattleMech record sheets

Each link is a complete stock `MECHTYPE.DAT` BattleMech record. Vehicles and infantry use different damage models and are intentionally excluded from BattleMech record sheets.

## `0x96`-byte unit-record layout

| Offset | Size | Field |
| ---: | ---: | --- |
| `0x00` | 1 | Deployment state |
| `0x01` | 1 | Unit type ID |
| `0x02` | 2 | Walking and jump movement |
| `0x04` | 8 | Current internal structure |
| `0x0C` | 11 | Current armour |
| `0x17` | 8 | Maximum internal structure |
| `0x1F` | 11 | Maximum armour |
| `0x2A` | 14 | Weapon-family counts |
| `0x38` | 28 | Fourteen current ammunition pools |
| `0x54` | 1 | Engine heat-sink capacity |
| `0x55` | 47 | Critical and equipment slots |
| `0x84` | 1 | Tonnage |
| `0x85` | 6 | Engine damage, heat and sprite state |
| `0x8B` | 7 | Tactical modifiers and special equipment |
| `0x92` | 3 | Pilot ID, experience and allegiance |
| `0x95` | 1 | Damage-model selector |

BattleMech durability order is head, left arm, left torso, centre torso, right torso, right arm, left leg and right leg, followed by rear left, centre and right torso armour. Equipment groups are head (1), left arm (8), left torso (12), centre torso (2), right torso (12), right arm (8), left leg (2) and right leg (2).

The **minimum** column on each sheet is the playable lower bound of zero; it is not stored as another durability array. Current and maximum durability are separate record fields. Ammunition has a current pool only, so the sheets explicitly mark maximum as not stored rather than inventing one.

## Chassis records

### Inner Sphere stock BattleMechs

- [Locust](revenge/mechs/locust.md)
- [Wasp](revenge/mechs/wasp.md)
- [Stinger](revenge/mechs/stinger.md)
- [Commando](revenge/mechs/commando.md)
- [Javelin](revenge/mechs/javelin.md)
- [Spider](revenge/mechs/spider.md)
- [UrbanMech](revenge/mechs/urbanmech.md)
- [Valkyrie](revenge/mechs/valkyrie.md)
- [Firestarter](revenge/mechs/firestarter.md)
- [Jenner](revenge/mechs/jenner.md)
- [Ostscout](revenge/mechs/ostscout.md)
- [Panther](revenge/mechs/panther.md)
- [Assassin](revenge/mechs/assassin.md)
- [Cicada](revenge/mechs/cicada.md)
- [Clint](revenge/mechs/clint.md)
- [Hermes II](revenge/mechs/hermes-ii.md)
- [Vulcan](revenge/mechs/vulcan.md)
- [Whitworth](revenge/mechs/whitworth.md)
- [Blackjack](revenge/mechs/blackjack.md)
- [Hatchetman](revenge/mechs/hatchetman.md)
- [Phoenix Hawk](revenge/mechs/phoenix-hawk.md)
- [Vindicator](revenge/mechs/vindicator.md)
- [Centurion](revenge/mechs/centurion.md)
- [Enforcer](revenge/mechs/enforcer.md)
- [Hunchback](revenge/mechs/hunchback.md)
- [Trebuchet](revenge/mechs/trebuchet.md)
- [Dervish](revenge/mechs/dervish.md)
- [Griffin](revenge/mechs/griffin.md)
- [Shadow Hawk](revenge/mechs/shadow-hawk.md)
- [Scorpion](revenge/mechs/scorpion.md)
- [Wolverine](revenge/mechs/wolverine.md)
- [Dragon](revenge/mechs/dragon.md)
- [Ostroc](revenge/mechs/ostroc.md)
- [Ostsol](revenge/mechs/ostsol.md)
- [Quickdraw](revenge/mechs/quickdraw.md)
- [Rifleman](revenge/mechs/rifleman.md)
- [Catapult](revenge/mechs/catapult.md)
- [Crusader](revenge/mechs/crusader.md)
- [JagerMech](revenge/mechs/jagermech.md)
- [Thunderbolt](revenge/mechs/thunderbolt.md)
- [Archer](revenge/mechs/archer.md)
- [Grasshopper](revenge/mechs/grasshopper.md)
- [Warhammer](revenge/mechs/warhammer.md)
- [Marauder](revenge/mechs/marauder.md)
- [Orion](revenge/mechs/orion.md)
- [Awesome](revenge/mechs/awesome.md)
- [Charger](revenge/mechs/charger.md)
- [Goliath](revenge/mechs/goliath.md)
- [Victor](revenge/mechs/victor.md)
- [Zeus](revenge/mechs/zeus.md)
- [BattleMaster](revenge/mechs/battlemaster.md)
- [Stalker](revenge/mechs/stalker.md)
- [Cyclops](revenge/mechs/cyclops.md)
- [Banshee](revenge/mechs/banshee.md)
- [Atlas](revenge/mechs/atlas.md)

### Clan BattleMechs

- [Puma](revenge/mechs/puma.md)
- [Black Hawk](revenge/mechs/black-hawk.md)
- [Mad Cat](revenge/mechs/mad-cat.md)

### Upgraded variants

- [Locust+](revenge/mechs/locust-plus.md)
- [Locust*](revenge/mechs/locust-star.md)
- [Wasp+](revenge/mechs/wasp-plus.md)
- [Wasp*](revenge/mechs/wasp-star.md)
- [Commando+](revenge/mechs/commando-plus.md)
- [Commando*](revenge/mechs/commando-star.md)
- [Blackjack+](revenge/mechs/blackjack-plus.md)
- [Phoenix Hawk+](revenge/mechs/phoenix-hawk-plus.md)
- [Phoenix Hawk*](revenge/mechs/phoenix-hawk-star.md)
- [Griffin+](revenge/mechs/griffin-plus.md)
- [Griffin*](revenge/mechs/griffin-star.md)
- [Rifleman+](revenge/mechs/rifleman-plus.md)
- [Rifleman*](revenge/mechs/rifleman-star.md)
- [Warhammer+](revenge/mechs/warhammer-plus.md)
- [Warhammer*](revenge/mechs/warhammer-star.md)
- [Marauder+](revenge/mechs/marauder-plus.md)
- [Jenner+](revenge/mechs/jenner-plus.md)
- [Dragon+](revenge/mechs/dragon-plus.md)
- [Shadow Hawk+](revenge/mechs/shadow-hawk-plus.md)
- [BattleMaster+](revenge/mechs/battlemaster-plus.md)

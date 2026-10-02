# Revenge pilot reference

Pilot IDs index parallel 59-entry given-name and surname tables. Blank given
names are intentional. Player save snapshots persist availability for the
first 37 IDs (`0x00..0x24`); the complete roster also includes scenario and
training identities.

| ID | Display name | ID | Display name |
| ---: | --- | ---: | --- |
| `00` | Jason Youngblood | `1E` | J.P. Berez |
| `01` | Rex Pearce | `1F` | Michell Dornbrook |
| `02` | Kurt Graham | `20` | Carl Montague |
| `03` | “Grease” Anderson | `21` | Sgt. Krall |
| `04` | Reilly | `22` | Jason Youngblood |
| `05` | Casey | `23` | “Grease” Anderson |
| `06` | Corrigan | `24` | Jen Pearce |
| `07` | Hall | `25` | Romero |
| `08` | Chuck Hunter | `26` | Carpenter |
| `09` | Clark Barth | `27` | Cronenberg |
| `0A` | Nance Tatum | `28` | DePalma |
| `0B` | Carmen Ledesma | `29` | Dante |
| `0C` | Tomas Bellamy | `2A` | Raimi |
| `0D` | Mitchel Latham | `2B` | Hooper |
| `0E` | Donald Louie | `2C` | Cunningham |
| `0F` | Christopher Christman | `2D` | Chiun |
| `10` | Wade Townsend | `2E` | Toranaga |
| `11` | Stewart Stewart | `2F` | Yamashita |
| `12` | Robert Bouy | `30` | Ichi |
| `13` | Joseph Hill | `31` | Hosaka |
| `14` | Gabby Accardi | `32` | Higa |
| `15` | Amanda Briggs | `33` | Sanjuro |
| `16` | Darrel Lebling | `34` | Takagi |
| `17` | Stewart Galley | `35` | Jackson |
| `18` | Marco Blank | `36` | Lewis |
| `19` | Stefan Meretzky | `37` | Wylie |
| `1A` | “Brain” Moriarty | `38` | Morton |
| `1B` | Geoff O'Neill | `39` | Vandenburg |
| `1C` | Marcus Berlyn | `3A` | Trainee |
| `1D` | Stella Kirsch |  |  |

The duplicated and unusual names are a literal transcription of the two
tables; for example ID `0x11` really combines “Stewart” with “Stewart”. Do not
normalize these values without additional evidence.

| Experience ID | Label |
| ---: | --- |
| 0 | Green |
| 1 | Regular |
| 2 | Veteran |
| 3 | Elite |

A pilot roster record is four bytes: signed identity ID, experience index,
gunnery index and availability. A live unit stores pilot ID at record offset
`0x92` and pilot experience at `0x93`.

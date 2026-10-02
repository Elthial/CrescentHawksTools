# Publication policy

This repository publishes original tooling, tests, and factual format
descriptions. It does not publish either game, a runnable installation, or an
asset pack.

Allowed repository content includes record layouts, offsets, hashes, enum
values, algorithms, short identifiers, typed interoperability tables, and
small reviewed screenshots used to explain the tools. Large verbatim binary
tables are avoided when equivalent typed metadata is sufficient.

Never commit executables, external game data, saves, extracted images, audio,
scripts, maps, or bulk generated reports. Local conformance runs may read those
files and may generate ignored output. A hash identifies evidence; it does not
license or redistribute the corresponding bytes.

Recovered semantics use three confidence levels:

- **Verified**: established by consumers, repeated files, or runtime tests;
- **Probable**: strongly supported but still interpretive;
- **Unknown**: retained as a neutral field or byte range.

Changing a probable label into a format guarantee requires reproducible
evidence and a verification update.

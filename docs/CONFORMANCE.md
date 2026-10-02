# Known versions and local conformance

The tools validate structure first and identify exact executable profiles by
length and SHA-256. An unknown hash is reported as
`unknown-compatible-executable`; it is not rejected when its structures are
otherwise valid.

| Game | Profile | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| Inception | `installed-reference` | 152,429 | `F2A9A023D79927B8072DE11DDD6E03DA6D3357D49181D8FBA6D12988BF8CC0EE` |
| Revenge | `version-1.00-reference` | 256,411 | `C4502165E251F3E2A5005B5C0E450E6B335B14C2BE9C035B0FD2D53CBB30667E` |

These hashes are metadata only. The corresponding executables are not included.

Run asset-independent verification first:

```powershell
./Build.ps1
```

Then inventory legally owned installations:

```powershell
dotnet run --project src/InceptionTools -- inventory --game-dir D:\Games\BTECH --hash
dotnet run --project src/RevengeTools -- inventory --game-dir D:\Games\REVENGE --hash
```

Revenge can additionally exercise local saves and the CPS/ICN/MAP/SCENE corpus:

```powershell
dotnet run --project tests/RevengeTools.Verification --configuration Release -- \
  D:\Games\REVENGE\SAVES D:\Games\REVENGE
```

Keep generated reports outside the repository or under an ignored output
directory. Conformance failure reports the first bounded structural error; it
must never be converted into a best-effort decode.

Installation inputs are capped at 128 MiB before allocation. This is far above
the known DOS assets while preventing a malformed or substituted file from
driving an unbounded `ReadAllBytes` allocation. Format-specific length and
decompression limits remain stricter.

# Contributing

Contributions that improve format coverage, validation, portability, or
documentation are welcome.

## Ground rules

1. Do not commit original executables, game installations, saves, extracted
   asset sets, recordings, or generated output directories.
2. Keep parsing bounded. Invalid offsets, lengths, opcodes, and compression
   streams must fail with a useful error rather than reading beyond input.
3. Preserve unknown bytes when editing saves. Never overwrite the source save,
   and keep dry-run and fingerprint protections intact.
4. Distinguish verified behavior from probable interpretation. Do not turn an
   attractive guess into a format contract.
5. Keep both toolsets dependency-free unless a dependency provides a clear,
   reviewed preservation benefit.
6. Add or extend verification coverage with behavioral changes.
7. Documentation images require explicit review, must be kept small, and must
   be recorded as original-rights material in `NOTICE.md` and
   `LICENSES/README.md`.

## Before submitting

```powershell
dotnet build CrescentHawksTools.sln --configuration Release
dotnet run --project tests/InceptionTools.Verification --configuration Release
dotnet run --project tests/RevengeTools.Verification --configuration Release
```

Do not include local extraction output in a pull request.

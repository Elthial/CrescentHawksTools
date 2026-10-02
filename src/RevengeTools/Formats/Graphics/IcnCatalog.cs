using RevengeTools.Formats.Units;

namespace RevengeTools.Formats.Graphics;

public enum IcnCatalogKind
{
    Terrain,
    Destruction,
    TacticalSprites,
    Unknown
}

public sealed record IcnCatalogProfile(
    string SourceFile,
    IcnCatalogKind Kind,
    int? SetIndex,
    string ProvenRole);

public sealed record UnitSpriteCatalogEntry(
    int LocalTileBase,
    int GlobalSpriteBase,
    bool IsInfantry,
    IReadOnlyList<int> FriendlyFacingTiles,
    IReadOnlyList<int> OpposingFacingTiles,
    IReadOnlyList<int> UnitTypeIds,
    IReadOnlyList<string> UnitNames);

public static class IcnCatalog
{
    public const int TacticalGlobalSpriteBase = 0x100;
    public const int TilesPerTacticalRow = 0x10;

    public static IcnCatalogProfile Classify(string sourceFile)
    {
        string fileName = Path.GetFileName(sourceFile).ToUpperInvariant();
        if (TryReadIndexedName(fileName, "ICONSET", 0, 7, out int iconSet))
            return new(fileName, IcnCatalogKind.Terrain, iconSet,
                "SCENE metadata selects this terrain-tile catalog alongside MAPx.MAP.");
        if (TryReadIndexedName(fileName, "DESTROY", 0, 7, out int destroySet))
            return new(fileName, IcnCatalogKind.Destruction, destroySet,
                "Companion destruction/effect catalog sharing the ICONSET selector.");
        if (TryReadIndexedName(fileName, "MECHSET", 1, 2, out int mechSet))
            return new(fileName, IcnCatalogKind.TacticalSprites, mechSet,
                "Tactical unit/effect catalog addressed by global sprite IDs 0x100..0x1F9.");
        return new(fileName, IcnCatalogKind.Unknown, null,
            "No executable-backed Revenge ICN role profile is known for this filename.");
    }

    public static IReadOnlyList<UnitSpriteCatalogEntry> BuildUnitSpriteCatalog(
        IReadOnlyList<UnitRecord> templates)
    {
        return templates
            .Where(unit => unit.IsPopulated)
            .GroupBy(unit => (int)unit.TacticalSpriteBase)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                int tileBase = group.Key;
                bool isInfantry = group.All(unit =>
                    unit.DamageModelKind == UnitDamageModel.Infantry);
                if (!isInfantry && tileBase % TilesPerTacticalRow != 0)
                    throw new InvalidDataException(
                        $"Non-infantry sprite base 0x{tileBase:X2} is not aligned to a MECHSET row.");
                int finalTile = tileBase + (isInfantry ? 0x02 : 0x0E);
                if (finalTile >= IcnImage.TileCount)
                    throw new InvalidDataException(
                        $"Unit sprite base 0x{tileBase:X2} exceeds the 250-tile MECHSET catalog.");
                return new UnitSpriteCatalogEntry(
                    tileBase,
                    TacticalGlobalSpriteBase + tileBase,
                    isInfantry,
                    isInfantry ? new[] { tileBase } :
                        new[] { tileBase, tileBase + 2, tileBase + 4, tileBase + 6 },
                    isInfantry ? new[] { tileBase + 2 } :
                        new[] { tileBase + 8, tileBase + 10, tileBase + 12, tileBase + 14 },
                    group.Select(unit => (int)unit.UnitTypeId).OrderBy(id => id).ToArray(),
                    group.OrderBy(unit => unit.UnitTypeId).Select(unit => unit.UnitName).ToArray());
            })
            .ToArray();
    }

    private static bool TryReadIndexedName(string fileName, string prefix,
        int minimum, int maximum, out int index)
    {
        index = -1;
        if (!fileName.StartsWith(prefix, StringComparison.Ordinal) ||
            !fileName.EndsWith(".ICN", StringComparison.Ordinal))
            return false;
        string suffix = fileName[prefix.Length..^4];
        return int.TryParse(suffix, out index) && index >= minimum && index <= maximum;
    }
}

namespace RevengeTools.Formats.Maps;

public sealed record MapResourceProfile(
    string MapFile,
    string IconSetFile,
    IReadOnlyList<string> SceneFiles);

public static class MapResourceCatalog
{
    private static readonly IReadOnlyDictionary<string, MapResourceProfile> Profiles =
        new[]
        {
            Profile("MAP0.MAP", 0, "SCENE1.DAT", "SCENE2.DAT", "SCENE3.DAT", "SCENE4.DAT"),
            Profile("MAP1.MAP", 1, "SCENE5.DAT", "SCENE6.DAT", "SCENE7.DAT"),
            Profile("MAP2.MAP", 0, "SCENE8.DAT", "SCENE9.DAT", "SCENEA.DAT"),
            Profile("MAP3.MAP", 2, "SCENEB.DAT", "SCENEC.DAT", "SCENEE.DAT"),
            Profile("MAP4.MAP", 3, "SCENED.DAT", "SCENET.DAT"),
            Profile("MAP5.MAP", 4, "SCENEF.DAT", "SCENEG.DAT", "SCENEH.DAT", "SCENEI.DAT", "SCENEJ.DAT", "SCENEK.DAT", "SCENEU.DAT"),
            Profile("MAP6.MAP", 5, "SCENEL.DAT", "SCENEM.DAT"),
            Profile("MAP7.MAP", 6, "SCENEN.DAT"),
            Profile("MAP8.MAP", 7, "SCENEO.DAT", "SCENEP.DAT", "SCENES.DAT"),
            Profile("MAP9.MAP", 7, "SCENEQ.DAT"),
            Profile("MAPA.MAP", 7, "SCENER.DAT")
        }.ToDictionary(profile => profile.MapFile, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<MapResourceProfile> All => Profiles.Values.ToArray();

    public static MapResourceProfile Resolve(string mapFile)
    {
        string fileName = Path.GetFileName(mapFile);
        return Profiles.TryGetValue(fileName, out MapResourceProfile? profile)
            ? profile
            : throw new InvalidDataException(
                $"No SCENE-verified ICONSET mapping is known for {fileName}; pass --icons explicitly.");
    }

    private static MapResourceProfile Profile(string mapFile, int iconSet,
        params string[] sceneFiles) =>
        new(mapFile, $"ICONSET{iconSet}.ICN", sceneFiles);
}

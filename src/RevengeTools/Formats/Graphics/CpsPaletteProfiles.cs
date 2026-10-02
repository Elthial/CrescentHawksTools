namespace RevengeTools.Formats.Graphics;

public static class CpsPaletteProfiles
{
    private static readonly IReadOnlyDictionary<string, string> Profiles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["BASEEXT.CPS"] = "BASEEXT.COL", ["BLAZACE.CPS"] = "BLAZACE.COL",
            ["BRUSHES.CPS"] = "MAPS.COL", ["BTBORDER.CPS"] = "SELCTION.COL",
            ["CHIUN.CPS"] = "FACES.COL", ["CHIUN'50.CPS"] = "FACES.COL",
            ["COMM.CPS"] = "FACES.COL", ["DSATTACK.CPS"] = "DSATTACK.COL",
            ["DSLYONS.CPS"] = "DSLYONS.COL", ["ENDGAME.CPS"] = "ENDGAME.COL",
            ["FINAL.CPS"] = "FINAL.COL", ["GREASE.CPS"] = "FACES.COL",
            ["HALFDS.CPS"] = "HALFDS.COL", ["HALFDSSH.CPS"] = "HALFDS.COL",
            ["HAWKLOGO.CPS"] = "HAWKLOGO.COL", ["ISMAP1.CPS"] = "ISMAPS.COL",
            ["ISMAP2.CPS"] = "ISMAPS.COL", ["ISMAPO.CPS"] = "ISMAPS.COL",
            ["JEN.CPS"] = "FACES.COL", ["JUMPSHIP.CPS"] = "JUMPSHIP.COL",
            ["KELL.CPS"] = "FACES.COL", ["KHBASE.CPS"] = "KHBASE.COL",
            ["KHHANGER.CPS"] = "KHHANGER.COL", ["KURT.CPS"] = "FACES.COL",
            ["MAP1.CPS"] = "MAPS.COL", ["MAP2.CPS"] = "MAPS.COL",
            ["MECHBAY.CPS"] = "MECHBAY.COL", ["MECHS1.CPS"] = "SELCTION.COL",
            ["MECHS2.CPS"] = "SELCTION.COL", ["ORDERS.CPS"] = "SELCTION.COL",
            ["PILOTS1.CPS"] = "SELCTION.COL", ["PILOTS2.CPS"] = "SELCTION.COL",
            ["QUADPICS.CPS"] = "QUADPICS.COL", ["REX.CPS"] = "FACES.COL",
            ["RIP.CPS"] = "RIP.COL", ["SHAW.CPS"] = "FACES.COL",
            ["SWAMP.CPS"] = "SWAMP.COL", ["TANK.CPS"] = "TANK.COL",
            ["TITLE.CPS"] = "TITLE.COL"
        };

    public static string GetPaletteFileName(string cpsFileName)
    {
        string name = Path.GetFileName(cpsFileName);
        return Profiles.TryGetValue(name, out string? palette) ? palette :
            throw new InvalidDataException($"No explicit palette profile exists for {name}; pass --palette FILE.COL.");
    }
}

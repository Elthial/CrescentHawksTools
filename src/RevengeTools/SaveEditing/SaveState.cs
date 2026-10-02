namespace RevengeTools.SaveEditing;

public sealed class SaveState
{
    public const string TextFormatId = "BTCHR_SAVE_STATE_V1";
    public required string SourceFileName { get; set; }
    public required string SourceSha256 { get; set; }
    public required int SlotNumber { get; set; }
    public required string Label { get; set; }
    public required int CampaignStage { get; set; }
    public required int ScenarioVariant { get; set; }
    public required int CampaignPhase { get; set; }
    public required int TrainingSequenceFlag { get; set; }
    public required int CampaignFlags { get; set; }
    public List<SaveUnitState> Units { get; } = new();
}

public sealed class SaveUnitState
{
    public required int Slot { get; set; }
    public required int DeploymentState { get; set; }
    public required int UnitTypeId { get; set; }
    public required string UnitName { get; set; }
    public required int[] CurrentInternal { get; set; }
    public required int[] CurrentArmor { get; set; }
    public required int[] MaximumInternal { get; set; }
    public required int[] MaximumArmor { get; set; }
    public required int[] CurrentAmmo { get; set; }
    public required int Tonnage { get; set; }
    public required int PilotId { get; set; }
    public required int Experience { get; set; }
    public required int Allegiance { get; set; }
}

public sealed record SaveStateChange(string Field, int FileOffset, byte OldValue, byte NewValue);

public sealed class SaveStateUpdateResult
{
    internal SaveStateUpdateResult(byte[] bytes, IReadOnlyList<SaveStateChange> changes)
    {
        Bytes = bytes;
        Changes = changes;
    }

    public byte[] Bytes { get; }
    public IReadOnlyList<SaveStateChange> Changes { get; }
}

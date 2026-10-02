using System.Collections.Generic;
using InceptionTools.Binary;

namespace InceptionTools.Records;

public sealed class SaveGameRecord
{
    public const int Length = 0x0F49;
    public const int SavedStateLength = 0x0F44;
    private readonly byte[] _rawBytes;
    private readonly byte[] _savedStateBytes;
    private readonly byte[] _probableWorldMapVisibility;
    private readonly List<CharacterRecord> _playerCharacters = new List<CharacterRecord>();
    private readonly List<CharacterRecord> _enemyCharacters = new List<CharacterRecord>();
    private readonly List<MechRecord> _playerMechs = new List<MechRecord>();
    private readonly List<MechRecord> _enemyMechs = new List<MechRecord>();

    private SaveGameRecord(BoundedBinaryReader reader)
    {
        reader.RequireExactLength(Length, "Save file");
        _rawBytes = reader.ReadBytes(0, Length, "save file");
        Header = reader.ReadByte(0x0000, "save header");
        _savedStateBytes = reader.ReadBytes(0x0001, SavedStateLength, "saved state block");

        for (int slot = 0; slot < 8; slot++)
            _playerCharacters.Add(ParseCharacter(reader, 0x0001 + slot * CharacterRecord.Length, "player character " + slot));
        for (int slot = 0; slot < 8; slot++)
            _enemyCharacters.Add(ParseCharacter(reader, 0x0089 + slot * CharacterRecord.Length, "enemy character " + slot));
        for (int slot = 0; slot < 4; slot++)
            _playerMechs.Add(ParseMech(reader, 0x0111 + slot * MechRecord.Length, "player mech " + slot));
        for (int slot = 0; slot < 4; slot++)
            _enemyMechs.Add(ParseMech(reader, 0x0305 + slot * MechRecord.Length, "enemy mech " + slot));

        _probableWorldMapVisibility = reader.ReadBytes(0x04F9, 0x0800, "probable world-map visibility");
        StoryStateAt0CF9 = reader.ReadByte(0x0CF9, "state byte 0x0CF9");
        Credits = reader.ReadUInt32LittleEndian(0x0D5D, "C-Bills");
        Stock0 = reader.ReadUInt32LittleEndian(0x0D61, "stock 0");
        Stock1 = reader.ReadUInt32LittleEndian(0x0D65, "stock 1");
        Stock2 = reader.ReadUInt32LittleEndian(0x0D69, "stock 2");
        PartyMapX = reader.ReadUInt16LittleEndian(0x0F45, "party map X");
        PartyMapY = reader.ReadUInt16LittleEndian(0x0F47, "party map Y");
    }

    public int Header { get; }
    public bool HeaderMatchesObservedProfile => Header == 0x0C;
    public byte[] SavedStateBytes => (byte[])_savedStateBytes.Clone();
    public IReadOnlyList<CharacterRecord> PlayerCharacters => _playerCharacters;
    public IReadOnlyList<CharacterRecord> EnemyCharacters => _enemyCharacters;
    public IReadOnlyList<MechRecord> PlayerMechs => _playerMechs;
    public IReadOnlyList<MechRecord> EnemyMechs => _enemyMechs;
    public byte[] ProbableWorldMapVisibility => (byte[])_probableWorldMapVisibility.Clone();
    public int StoryStateAt0CF9 { get; }
    public uint Credits { get; }
    public uint Stock0 { get; }
    public uint Stock1 { get; }
    public uint Stock2 { get; }
    public int PartyMapX { get; }
    public int PartyMapY { get; }
    public byte[] RawBytes => (byte[])_rawBytes.Clone();

    public static SaveGameRecord Parse(byte[] data, string sourceName = "save file")
    {
        return new SaveGameRecord(new BoundedBinaryReader(data, sourceName));
    }

    private static CharacterRecord ParseCharacter(BoundedBinaryReader reader, int offset, string name)
    {
        return CharacterRecord.Parse(reader.ReadBytes(offset, CharacterRecord.Length, name), name);
    }

    private static MechRecord ParseMech(BoundedBinaryReader reader, int offset, string name)
    {
        return MechRecord.Parse(reader.ReadBytes(offset, MechRecord.Length, name), name);
    }
}

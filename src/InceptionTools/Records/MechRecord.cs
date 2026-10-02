using InceptionTools.Binary;

namespace InceptionTools.Records
{
    public sealed class PackedActuatorByte
    {
        public PackedActuatorByte(int currentRaw, int maximumRaw)
        {
            CurrentRaw = currentRaw;
            MaximumRaw = maximumRaw;
        }

        public int CurrentRaw { get; }
        public int MaximumRaw { get; }
        public int CurrentLowNibble => CurrentRaw & 0x0F;
        public int CurrentHighNibble => CurrentRaw >> 4;
        public int MaximumLowNibble => MaximumRaw & 0x0F;
        public int MaximumHighNibble => MaximumRaw >> 4;
    }

    public sealed class MechRecord
    {
        public const int Length = 0x7D;
        private static readonly string[] KnownChassisNames =
            { "LOCUST", "WASP", "STINGER", "COMMANDO", "CHAMELEON", "JENNER", "SPECTATOR", "URBANMECH" };
        private readonly byte[] _rawBytes;
        private readonly byte[] _currentArmour;
        private readonly byte[] _currentStructure;
        private readonly byte[] _currentAmmo;
        private readonly byte[] _criticalSlotsRaw;
        private readonly byte[] _maximumArmour;
        private readonly byte[] _maximumStructure;
        private readonly byte[] _maximumAmmo;

        private MechRecord(BoundedBinaryReader reader)
        {
            reader.RequireExactLength(Length, "Mech record");
            _rawBytes = reader.ReadBytes(0, Length, "mech record");
            NameFirstByteRaw = reader.ReadByte(0x00, "name/status byte");
            Name = DecodeName(_rawBytes);
            Tonnage = reader.ReadByte(0x10, "tonnage");
            _currentArmour = reader.ReadBytes(0x11, 11, "current armour");
            _currentStructure = reader.ReadBytes(0x1C, 8, "current structure");
            ActuatorByte24 = new PackedActuatorByte(
                reader.ReadByte(0x24, "current actuator byte 24"),
                reader.ReadByte(0x69, "maximum actuator byte 69"));
            ActuatorByte25 = new PackedActuatorByte(
                reader.ReadByte(0x25, "current actuator byte 25"),
                reader.ReadByte(0x6A, "maximum actuator byte 6A"));
            EngineHeatSinks = reader.ReadByte(0x26, "engine heat sinks");
            _currentAmmo = reader.ReadBytes(0x27, 10, "current ammo");
            WalkMove = reader.ReadByte(0x31, "walk movement");
            JumpMove = reader.ReadByte(0x32, "jump movement");
            _criticalSlotsRaw = reader.ReadBytes(0x33, 0x23, "critical slots");
            _maximumArmour = reader.ReadBytes(0x56, 11, "maximum armour");
            _maximumStructure = reader.ReadBytes(0x61, 8, "maximum structure");
            _maximumAmmo = reader.ReadBytes(0x6B, 10, "maximum ammo");
            EngineHits = reader.ReadByte(0x75, "engine hits");
            GyroHits = reader.ReadByte(0x76, "gyro hits");
            SensorHits = reader.ReadByte(0x77, "sensor hits");
            Byte78ProbableLifeSupportState = reader.ReadByte(0x78, "probable life-support state");
            PilotId = reader.ReadByte(0x79, "pilot ID");
            RiderId = reader.ReadByte(0x7A, "rider ID");
            Byte7BProbableUpgradePackageBase = reader.ReadByte(0x7B, "probable upgrade package base");
            UpgradeLevelFlags = reader.ReadByte(0x7C, "upgrade level flags");
        }

        public string Name { get; }
        public int NameFirstByteRaw { get; }
        public bool HasProbableNoMechOrDestroyedMarker => (NameFirstByteRaw & 0x80) != 0;
        public int Tonnage { get; }
        public byte[] CurrentArmour => (byte[])_currentArmour.Clone();
        public byte[] CurrentStructure => (byte[])_currentStructure.Clone();
        public PackedActuatorByte ActuatorByte24 { get; }
        public PackedActuatorByte ActuatorByte25 { get; }
        public int EngineHeatSinks { get; }
        public byte[] CurrentAmmo => (byte[])_currentAmmo.Clone();
        public int WalkMove { get; }
        public int JumpMove { get; }
        public byte[] CriticalSlotsRaw => (byte[])_criticalSlotsRaw.Clone();
        public byte[] MaximumArmour => (byte[])_maximumArmour.Clone();
        public byte[] MaximumStructure => (byte[])_maximumStructure.Clone();
        public byte[] MaximumAmmo => (byte[])_maximumAmmo.Clone();
        public int EngineHits { get; }
        public int GyroHits { get; }
        public int SensorHits { get; }
        public int Byte78ProbableLifeSupportState { get; }
        public int PilotId { get; }
        public int RiderId { get; }
        public int Byte7BProbableUpgradePackageBase { get; }
        public int UpgradeLevelFlags { get; }
        public byte[] RawBytes => (byte[])_rawBytes.Clone();

        public static MechRecord Parse(byte[] data, string sourceName = "mech record")
        {
            return new MechRecord(new BoundedBinaryReader(data, sourceName));
        }

        private static string DecodeName(byte[] rawBytes)
        {
            var characters = new System.Text.StringBuilder(16);
            int start = rawBytes[0] == 0xFF ? 1 : 0;
            for (int index = start; index < 16; index++)
            {
                int value = rawBytes[index];
                if (index == 0)
                    value &= 0x7F;
                characters.Append((char)value);
            }
            string decoded = characters.ToString().TrimEnd('\0', ' ');
            if (rawBytes[0] != 0xFF || decoded.Length == 0)
                return decoded;

            foreach (string chassisName in KnownChassisNames)
                if (chassisName.Substring(1).Equals(decoded, System.StringComparison.Ordinal))
                    return chassisName;
            return decoded;
        }
    }
}

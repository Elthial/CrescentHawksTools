using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Class
{
    public class Actuator : CurrentToMaxValue
    {
        public Actuator(int CurrentValue, int MaxValue) : base(CurrentValue, MaxValue) { }
    }
    public class Ammo : CurrentToMaxValue
    {
        public Ammo(int CurrentValue, int MaxValue) : base(CurrentValue, MaxValue) { }
    }

    public class Armour : CurrentToMaxValue
    {
        public Armour(int CurrentValue, int MaxValue) : base(CurrentValue, MaxValue) { }       
    }

    public class InternalStructure : CurrentToMaxValue
    {
        public InternalStructure(int CurrentValue, int MaxValue) : base(CurrentValue, MaxValue) { }
    }

    public class CurrentToMaxValue
    {
        public CurrentToMaxValue(int CurrentValue, int MaxValue)
        {
            this.CurrentValue = CurrentValue;
            this.MaxValue = MaxValue;
        }

        public int CurrentValue { get; }
        public int MaxValue { get; }
        public override string ToString() => $"{CurrentValue} / {MaxValue}";       
    }

    public class Critical
    { 
        public Critical(int Size, byte[] Slots)
        {
            this.Size = Size;
            this.Slots = Slots;        
        }
        
        public int Size { get; }

        public byte[] Slots { get; }
    }

}

using System;

namespace osu.Game.Rulesets.Sentakki.IO
{
    public struct TransmissionData : IEquatable<TransmissionData>
    {
        public static TransmissionData Empty => new TransmissionData(0);

        public enum InfoType : byte
        {
            None,
            MetaStartPlay,
            MetaEndPlay,

            HitPerfect,
            HitGreat,
            HitGood,
            Miss,

            LanePressed,
        }

        public TransmissionData(byte input)
        {
            RawData = input;

            int value = 0;
            for (int i = 0; i < 3; ++i)
            {
                value += (input & 1) << i;
                input >>= 1;
            }
            Value = value;
            Type = (InfoType)input;
        }

        public TransmissionData(InfoType type, int value)
        {
            RawData = (byte)((int)type << 3);
            RawData += (byte)(value);
            Type = type;
            Value = value;
        }

        public byte RawData;

        public InfoType Type;

        public int Value;

        public override string ToString() => Type.ToString() + " " + Value;


        public override bool Equals(object obj) => obj is TransmissionData other && Equals(other);

        public bool Equals(TransmissionData other) => Type == other.Type && Value == other.Value;

        public override int GetHashCode() => (Type, Value).GetHashCode();

        public static bool operator ==(TransmissionData a, TransmissionData b) => a.Equals(b);
        public static bool operator !=(TransmissionData a, TransmissionData b) => !a.Equals(b);
    }
}

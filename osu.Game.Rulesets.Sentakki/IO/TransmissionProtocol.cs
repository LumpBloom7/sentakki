namespace osu.Game.Rulesets.Sentakki.IO
{
    public struct TransimssionData
    {
        public enum InfoType : byte
        {
            MetaStartPlay,
            MetaEndPlay,

            HitPerfect,
            HitGreat,
            HitGood,
            Miss,
        }

        public TransimssionData(byte input)
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

        public TransimssionData(InfoType type, int value)
        {
            RawData = (byte)((int)type << 3);
            RawData += (byte)(value);
            Type = type;
            Value = value;
        }

        public byte RawData;

        public InfoType Type;

        public int Value;
    }
}

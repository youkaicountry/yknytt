using IniParser.Model;

namespace YKnyttLib
{
    public class KnyttSwitch
    {
        public enum SwitchID
        {
            A = 0,
            B = 1,
            C = 2
        }

        public enum SwitchShape
        {
            SPOT = 0,
            FLOOR = 1,
            CIRCLE = 2,
            SQUARE = 3
        }

        public KnyttPoint Pos { get; protected set; }
        public SwitchID ID { get; protected set; }
        public SwitchShape Shape { get; private set; }
        public bool Visible { get; private set; }
        public bool Effect { get; private set; }
        public bool OnTouch { get; private set; }
        public bool DenyHologram { get; private set; }
        public string Sound { get; private set; }
        public bool AbsoluteTarget { get; set; }
        public bool AsOne { get; protected set; }
        public bool Repeat { get; protected set; }
        public virtual KnyttPoint AbsolutePosition { get; protected set; }

        public KnyttPoint RelativePosition
        {
            get { return AbsolutePosition - Pos; }
            set { AbsolutePosition = Pos + value; }
        }

        public KnyttPoint FormattedPosition
        {
            get { return AbsoluteTarget ? AbsolutePosition : RelativePosition; }
            set { if (AbsoluteTarget) { AbsolutePosition = value; } else { RelativePosition = value; } }
        }

        protected string prefix; 

        protected virtual void loadFromINI(KeyDataCollection data)
        {
            AbsoluteTarget = getBoolINIValue(data, "AbsoluteTarget", AbsoluteTarget);
            Effect = getBoolINIValue(data, "Effect", true);
            Visible = getBoolINIValue(data, "Visible", true);
            OnTouch = getBoolINIValue(data, "Touch", false);
            DenyHologram = getBoolINIValue(data, "DenyHologram", false);
            int shape = getIntINIValue(data, "Type");
            Shape = shape >= 0 && shape < 4 ? (SwitchShape)shape : SwitchShape.SPOT;
            Sound = getStringINIValue(data, "Sound");
        }
        
        protected bool getBoolINIValue(KeyDataCollection data, string name, bool @default = false)
        {
            string value = getStringINIValue(data, name);
            if (value == null) { return @default; }
            return value.ToLower().Equals("true") ? true : false;
        }

        protected int getIntINIValue(KeyDataCollection data, string name, int @default = 0)
        {
            string value = getStringINIValue(data, name);
            if (value == null) { return @default; }
            return int.TryParse(value, out var i) ? i : 0;
        }

        protected string getStringINIValue(KeyDataCollection data, string name)
        {
            char letter = "ABC"[(int)ID];
            string key = $"{prefix}{name}({letter})";
            if (!data.ContainsKey(key)) { return null; }
            return data[key];
        }
    }
}

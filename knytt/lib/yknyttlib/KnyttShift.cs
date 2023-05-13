using IniParser.Model;

namespace YKnyttLib
{
    public class KnyttShift : KnyttSwitch
    {
        public KnyttPoint AreaPos { get; }
        public bool Save { get; private set; }
        public bool Quantize { get; private set; }
        public string Cutscene { get; private set; }
        public bool StopMusic { get; private set; }
        public JuniValues.Flag FlagOn { get; private set; }
        public JuniValues.Flag FlagOff { get; private set; }
        public int Coin { get; private set; }
        public int Delay { get; protected set; }

        public KnyttPoint AbsoluteArea { get; set; }

        public KnyttPoint RelativeArea
        {
            get { return AbsoluteArea - AreaPos; }
            set { AbsoluteArea = AreaPos + value; }
        }

        public KnyttPoint FormattedArea
        {
            get { return AbsoluteTarget ? AbsoluteArea : RelativeArea; }
            set { if (AbsoluteTarget) { AbsoluteArea = value; } else { RelativeArea = value; }  }
        }

        private int positiveMod(int x, int m) { return (x % m + m) % m; }
        private int floorDiv(int x, int m) { return (x - positiveMod(x, m)) / m; }

        private KnyttPoint _absolute_position;
        public override KnyttPoint AbsolutePosition
        {
            get { return _absolute_position; }
            protected set
            {
                AbsoluteArea += new KnyttPoint(floorDiv(value.x, KnyttArea.AREA_WIDTH), floorDiv(value.y, KnyttArea.AREA_HEIGHT));
                _absolute_position = new KnyttPoint(positiveMod(value.x, KnyttArea.AREA_WIDTH), positiveMod(value.y, KnyttArea.AREA_HEIGHT));
            }
        }

        public KnyttShift(KnyttPoint area_pos, KnyttPoint shift_pos, SwitchID id)
        {
            AreaPos = area_pos;
            Pos = shift_pos;
            ID = id;
            AbsoluteTarget = false; // Relative by default
            AsOne = true;
            Repeat = true;
            prefix = "Shift";
        }

        public KnyttShift(KnyttPoint area_pos, KnyttPoint shift_pos, SwitchID id, KeyDataCollection data) : this(area_pos, shift_pos, id)
        {
            loadFromINI(data);
        }

        public KnyttShift(KnyttArea area, KnyttPoint shift_pos, SwitchID id) : this(area.Position, shift_pos, id, area.ExtraData) { }

        protected override void loadFromINI(KeyDataCollection data)
        {
            base.loadFromINI(data);
            FormattedArea = new KnyttPoint(getIntINIValue(data, "XMap"), getIntINIValue(data, "YMap"));
            FormattedPosition = new KnyttPoint(getIntINIValue(data, "XPos"), getIntINIValue(data, "YPos"));
            Save = getBoolINIValue(data, "Save", false);
            StopMusic = getBoolINIValue(data, "StopMusic", true);
            Quantize = getBoolINIValue(data, "Quantize", true);
            StopMusic = getBoolINIValue(data, "StopMusic", false);
            Cutscene = getStringINIValue(data, "Cutscene");
            FlagOn = JuniValues.Flag.Parse(getStringINIValue(data, "FlagOn"));
            FlagOff = JuniValues.Flag.Parse(getStringINIValue(data, "FlagOff"));
            Coin = getIntINIValue(data, "Coin");
            Delay = getIntINIValue(data, "Time");
        }
    }
}

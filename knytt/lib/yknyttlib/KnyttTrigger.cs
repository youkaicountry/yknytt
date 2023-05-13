using IniParser.Model;

namespace YKnyttLib
{
    public class KnyttTrigger : KnyttSwitch
    {
        public KnyttPoint ObjectID { get; private set; }
        public KnyttPoint EffectOffset { get; private set;}

        public KnyttTrigger(KnyttPoint trigger_pos, SwitchID id)
        {
            Pos = trigger_pos;
            ID = id;
            AbsoluteTarget = true;
            prefix = "Trig";
        }

        public KnyttTrigger(KnyttPoint trigger_pos, SwitchID id, KeyDataCollection data) : this(trigger_pos, id)
        {
            loadFromINI(data);
        }

        public KnyttTrigger(KnyttArea area, KnyttPoint shift_pos, SwitchID id) : this(shift_pos, id, area.ExtraData) { }

        protected override void loadFromINI(KeyDataCollection data)
        {
            base.loadFromINI(data);
            FormattedPosition = new KnyttPoint(getIntINIValue(data, "SpawnX"), getIntINIValue(data, "SpawnY"));
            ObjectID = new KnyttPoint(getIntINIValue(data, "Bank"), getIntINIValue(data, "Object"));
            EffectOffset = new KnyttPoint(getIntINIValue(data, "EffectX"), getIntINIValue(data, "EffectY"));
            AsOne = getBoolINIValue(data, "AsOne", true);
            Repeat = getBoolINIValue(data, "Repeat", false);
        }
    }
}

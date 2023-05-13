using System.Collections.Generic;

namespace YKnyttLib.Paging
{
    public abstract class KnyttPagingPolicy<T>
    {
        public delegate T PageInEvent(KnyttPoint location);
        public delegate void PageOutEvent(KnyttPoint location, T area);

        public PageInEvent OnPageIn { get; set; }
        public PageOutEvent OnPageOut { get; set; }

        protected KnyttPoint current_location = new KnyttPoint(int.MinValue, int.MinValue);

        public Dictionary<KnyttPoint, T> Areas{ get; } 

        public KnyttPagingPolicy()
        {
            this.Areas = new Dictionary<KnyttPoint, T>();
        }

        public void setLocation(KnyttPoint location)
        {
            if (this.current_location.Equals(location)) { return; }

            this._handlePaging(location);
            this.current_location = location;
        }

        protected void pageIn(KnyttPoint location)
        {
            T area = this.OnPageIn(location);
            this.Areas.Add(location, area);
        }

        protected void pageOut(KnyttPoint location)
        {
            T area = this.Areas[location];
            this.Areas.Remove(location);
            this.OnPageOut?.Invoke(location, area);
        }

        protected abstract void _handlePaging(KnyttPoint location);
    }
}

using System.Collections.Generic;

namespace YKnyttLib.Paging
{
    public partial class KnyttRectPaging<T> : KnyttPagingPolicy<T>
    {
        public KnyttPoint BorderSize { get; set; }

        public KnyttRectPaging(KnyttPoint border_size)
        {
            this.BorderSize = border_size;
        }

        // This is O(n) time
        protected override void _handlePaging(KnyttPoint location)
        {
            List<KnyttPoint> q_remove = new List<KnyttPoint>();
            List<KnyttPoint> q_add = new List<KnyttPoint>();
            // Iterate over the current set, and remove 
            foreach (var l in this.Areas.Keys)
            {
                if (!isIn(l, location)) { q_remove.Add(l); }
            }

            // Iterate over the new area and add any that aren't in this.Areas
            for (int y = location.Y - BorderSize.Y; y <= location.Y + BorderSize.Y; y++)
            {
                for (int x = location.X - BorderSize.X; x <= location.X + BorderSize.X; x++)
                {
                    var kp = new KnyttPoint(x, y);
                    if (!Areas.ContainsKey(kp)) { q_add.Add(kp); }
                }
            }

            // Now just parse the queues
            foreach (var l in q_remove)
            {
                pageOut(l);
            }

            foreach (var l in q_add)
            {
                pageIn(l);
            }
        }

        private bool isIn(KnyttPoint location, KnyttPoint test_location)
        {
            return location.X >= (test_location.X - BorderSize.X) && location.X <= (test_location.X + BorderSize.X) &&
                   location.Y >= (test_location.Y - BorderSize.Y) && location.Y <= (test_location.Y + BorderSize.Y);
        }
    }
}

using System.Collections.Generic;

namespace YKnyttLib.Paging
{
    public class KnyttRectPaging<T> : KnyttPagingPolicy<T>
    {
        public KnyttPoint BorderSize { get; private set; }

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
            for (int y = location.y - BorderSize.y; y <= location.y + BorderSize.y; y++)
            {
                for (int x = location.x - BorderSize.x; x <= location.x + BorderSize.x; x++)
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
            return location.x >= (test_location.x - BorderSize.x) && location.x <= (test_location.x + BorderSize.x) &&
                   location.y >= (test_location.y - BorderSize.y) && location.y <= (test_location.y + BorderSize.y);
        }
    }
}

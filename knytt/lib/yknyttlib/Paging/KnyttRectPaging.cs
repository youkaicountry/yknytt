using System.Collections.Generic;
using System.Threading.Tasks;

namespace YKnyttLib.Paging
{
    public class KnyttRectPaging<T> : KnyttPagingPolicy<T>
    {
        private LinkedList<KnyttPoint> q_remove = new LinkedList<KnyttPoint>();
        private LinkedList<KnyttPoint> q_add = new LinkedList<KnyttPoint>();
        private Task task = null;

        public KnyttPoint BorderSize { get; private set; }

        public KnyttRectPaging(KnyttPoint border_size)
        {
            this.BorderSize = border_size;
        }

        protected override void _handlePaging(KnyttPoint location)
        {
            // Iterate over the current set + queue, and remove 
            foreach (var l in this.Areas.Keys)
            {
                if (!isIn(l, location)) { q_remove.AddLast(l); }
            }

            for (var node = q_add.First; node != null; )
            {
                var next = node.Next;
                if (!isIn(node.Value, location)) { q_add.Remove(node); }
                node = next;
            }

            // Iterate over the new area and add any that aren't in this.Areas or queue
            for (int y = location.y - BorderSize.y; y <= location.y + BorderSize.y; y++)
            {
                for (int x = location.x - BorderSize.x; x <= location.x + BorderSize.x; x++)
                {
                    var kp = new KnyttPoint(x, y);
                    if (!Areas.ContainsKey(kp) && !q_add.Contains(kp)) { q_add.AddLast(kp); }
                    q_remove.Remove(kp);
                }
            }

            if (q_add.Contains(location)) // central area cannot wait
            {
                q_add.Remove(location);
                pageIn(location);
            }

            if (task != null) { return; }
            task = Task.Run(() => {
                // Now just parse the queues
                while (q_remove.Count > 0 && q_add.Count > 0)
                {
                    while (q_remove.Count > 0) { pageOut(q_remove.PopFirst()); }
                    while (q_add.Count > 0) { pageIn(q_add.PopFirst()); }
                }
                task = null;
            });
        }

        private bool isIn(KnyttPoint location, KnyttPoint test_location)
        {
            return location.x >= (test_location.x - BorderSize.x) && location.x <= (test_location.x + BorderSize.x) &&
                   location.y >= (test_location.y - BorderSize.y) && location.y <= (test_location.y + BorderSize.y);
        }
    }
}

public static class LinkedListExtension
{
    public static T PopFirst<T>(this LinkedList<T> list)
    {
        var el = list.First.Value;
        list.RemoveFirst();
        return el;
    }
}

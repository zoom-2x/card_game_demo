using System.Collections;
using System.Collections.Generic;

namespace CardGame.Data
{
    public class CommandEvent
    {
        event System.Action e;
        Queue<System.Action> event_queue = new Queue<System.Action>();

        public CommandEvent() {}

        public void register(System.Action callback)
        {
            event_queue.Enqueue(callback);
            e += callback;
        }

        public bool empty {
            get { return event_queue.Count == 0; }
        }

        public void clear()
        {
            while (event_queue.Count > 0)
            {
                System.Action callback = event_queue.Dequeue();
                e -= callback;
            }
        }

        public void trigger() {
            e?.Invoke();
        }
    }
}

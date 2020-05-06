using System;
using System.Collections.Generic;
using System.Text;

namespace DFSServer
{
    public static class MessageQueue
    {
        private static Queue<MessageQueueItem> queue = new Queue<MessageQueueItem>();

        public static void Enqueue(MessageQueueItem request)
        {
            queue.Enqueue(request);
        }

        public static MessageQueueItem Dequeue()
        {
            MessageQueueItem queueItem;
            bool result = queue.TryDequeue(out queueItem);
            if(result)
                return queueItem;
            return null;
        }

        public static MessageQueueItem Peek()
        {
            MessageQueueItem queueItem;
            bool result = queue.TryPeek(out queueItem);
            if (result)
                return queueItem;
            return null;
        }

        public static int GetCount()
        {
            return queue.Count;
        }
    }
}

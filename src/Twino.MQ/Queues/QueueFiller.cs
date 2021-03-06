using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twino.Protocols.TMQ;

namespace Twino.MQ.Queues
{
    /// <summary>
    /// Fills bulk data into a queue
    /// </summary>
    public class QueueFiller
    {
        private readonly ChannelQueue _queue;

        /// <summary>
        /// Creates new queue filler
        /// </summary>
        public QueueFiller(ChannelQueue queue)
        {
            _queue = queue;
        }

        /// <summary>
        /// Fills JSON object data to the queue
        /// </summary>
        public async Task<PushResult> FillJson<T>(IEnumerable<T> items, bool createAsSaved, bool highPriority) where T : class
        {
            if (_queue.Status == QueueStatus.Stopped)
                return PushResult.StatusNotSupported;

            int max = _queue.HighPriorityLinkedList.Count + _queue.RegularLinkedList.Count + items.Count();
            if (_queue.Options.MessageLimit > 0 && max > _queue.Options.MessageLimit)
                return PushResult.LimitExceeded;

            foreach (T item in items)
            {
                TmqMessage message = new TmqMessage(MessageType.Channel, _queue.Channel.Name);
                message.FirstAcquirer = true;
                message.HighPriority = highPriority;
                message.AcknowledgeRequired = _queue.Options.RequestAcknowledge;
                message.ContentType = _queue.Id;

                if (_queue.Options.UseMessageId)
                    message.SetMessageId(_queue.Channel.Server.MessageIdGenerator.Create());

                await message.SetJsonContent(item);

                QueueMessage qm = new QueueMessage(message, createAsSaved);

                if (highPriority)
                    lock (_queue.HighPriorityLinkedList)
                        _queue.HighPriorityLinkedList.AddLast(qm);
                else
                    lock (_queue.RegularLinkedList)
                        _queue.RegularLinkedList.AddLast(qm);
            }

            _queue.Info.UpdateHighPriorityMessageCount(_queue.HighPriorityLinkedList.Count);
            _queue.Info.UpdateRegularMessageCount(_queue.RegularLinkedList.Count);
            return PushResult.Success;
        }

        /// <summary>
        /// Fills JSON object data to the queue.
        /// Creates new TmqMessage and before writing content and adding into queue calls the action.
        /// </summary>
        public async Task<PushResult> FillJson<T>(IEnumerable<T> items, bool createAsSaved, Action<TmqMessage, T> action) where T : class
        {
            if (_queue.Status == QueueStatus.Stopped)
                return PushResult.StatusNotSupported;

            int max = _queue.HighPriorityLinkedList.Count + _queue.RegularLinkedList.Count + items.Count();
            if (_queue.Options.MessageLimit > 0 && max > _queue.Options.MessageLimit)
                return PushResult.LimitExceeded;

            foreach (T item in items)
            {
                TmqMessage message = new TmqMessage(MessageType.Channel, _queue.Channel.Name);
                message.FirstAcquirer = true;
                message.AcknowledgeRequired = _queue.Options.RequestAcknowledge;
                message.ContentType = _queue.Id;

                if (_queue.Options.UseMessageId)
                    message.SetMessageId(_queue.Channel.Server.MessageIdGenerator.Create());

                action(message, item);
                await message.SetJsonContent(item);

                QueueMessage qm = new QueueMessage(message, createAsSaved);

                if (message.HighPriority)
                    lock (_queue.HighPriorityLinkedList)
                        _queue.HighPriorityLinkedList.AddLast(qm);
                else
                    lock (_queue.RegularLinkedList)
                        _queue.RegularLinkedList.AddLast(qm);
            }

            _queue.Info.UpdateHighPriorityMessageCount(_queue.HighPriorityLinkedList.Count);
            _queue.Info.UpdateRegularMessageCount(_queue.RegularLinkedList.Count);
            return PushResult.Success;
        }

        /// <summary>
        /// Fills string data to the queue
        /// </summary>
        public PushResult FillString(IEnumerable<string> items, bool createAsSaved, bool highPriority)
        {
            if (_queue.Status == QueueStatus.Stopped)
                return PushResult.StatusNotSupported;

            int max = _queue.HighPriorityLinkedList.Count + _queue.RegularLinkedList.Count + items.Count();
            if (_queue.Options.MessageLimit > 0 && max > _queue.Options.MessageLimit)
                return PushResult.LimitExceeded;

            foreach (string item in items)
            {
                TmqMessage message = new TmqMessage(MessageType.Channel, _queue.Channel.Name);
                message.FirstAcquirer = true;
                message.HighPriority = highPriority;
                message.AcknowledgeRequired = _queue.Options.RequestAcknowledge;
                message.ContentType = _queue.Id;

                if (_queue.Options.UseMessageId)
                    message.SetMessageId(_queue.Channel.Server.MessageIdGenerator.Create());

                message.Content = new MemoryStream(Encoding.UTF8.GetBytes(item));
                message.Content.Position = 0;
                message.CalculateLengths();

                QueueMessage qm = new QueueMessage(message, createAsSaved);

                if (highPriority)
                    lock (_queue.HighPriorityLinkedList)
                        _queue.HighPriorityLinkedList.AddLast(qm);
                else
                    lock (_queue.RegularLinkedList)
                        _queue.RegularLinkedList.AddLast(qm);
            }

            _queue.Info.UpdateHighPriorityMessageCount(_queue.HighPriorityLinkedList.Count);
            _queue.Info.UpdateRegularMessageCount(_queue.RegularLinkedList.Count);
            return PushResult.Success;
        }

        /// <summary>
        /// Fills binary data to the queue
        /// </summary>
        public PushResult FillData(IEnumerable<byte[]> items, bool createAsSaved, bool highPriority)
        {
            if (_queue.Status == QueueStatus.Stopped)
                return PushResult.StatusNotSupported;

            int max = _queue.HighPriorityLinkedList.Count + _queue.RegularLinkedList.Count + items.Count();
            if (_queue.Options.MessageLimit > 0 && max > _queue.Options.MessageLimit)
                return PushResult.LimitExceeded;

            foreach (byte[] item in items)
            {
                TmqMessage message = new TmqMessage(MessageType.Channel, _queue.Channel.Name);
                message.FirstAcquirer = true;
                message.HighPriority = highPriority;
                message.AcknowledgeRequired = _queue.Options.RequestAcknowledge;
                message.ContentType = _queue.Id;

                if (_queue.Options.UseMessageId)
                    message.SetMessageId(_queue.Channel.Server.MessageIdGenerator.Create());

                message.Content = new MemoryStream(item);
                message.Content.Position = 0;
                message.CalculateLengths();

                QueueMessage qm = new QueueMessage(message, createAsSaved);

                if (highPriority)
                    lock (_queue.HighPriorityLinkedList)
                        _queue.HighPriorityLinkedList.AddLast(qm);
                else
                    lock (_queue.RegularLinkedList)
                        _queue.RegularLinkedList.AddLast(qm);
            }

            _queue.Info.UpdateHighPriorityMessageCount(_queue.HighPriorityLinkedList.Count);
            _queue.Info.UpdateRegularMessageCount(_queue.RegularLinkedList.Count);
            return PushResult.Success;
        }

        /// <summary>
        /// Fills TMQ Message objects to the queue
        /// </summary>
        public PushResult FillMessage(IEnumerable<TmqMessage> messages, bool isSaved)
        {
            if (_queue.Status == QueueStatus.Stopped)
                return PushResult.StatusNotSupported;

            int max = _queue.HighPriorityLinkedList.Count + _queue.RegularLinkedList.Count + messages.Count();
            if (_queue.Options.MessageLimit > 0 && max > _queue.Options.MessageLimit)
                return PushResult.LimitExceeded;

            foreach (TmqMessage message in messages)
            {
                message.SetTarget(_queue.Channel.Name);
                message.ContentType = _queue.Id;

                if (_queue.Options.UseMessageId && string.IsNullOrEmpty(message.MessageId))
                    message.SetMessageId(_queue.Channel.Server.MessageIdGenerator.Create());

                message.CalculateLengths();

                QueueMessage qm = new QueueMessage(message, isSaved);

                if (message.HighPriority)
                    lock (_queue.HighPriorityLinkedList)
                        _queue.HighPriorityLinkedList.AddLast(qm);
                else
                    lock (_queue.RegularLinkedList)
                        _queue.RegularLinkedList.AddLast(qm);
            }

            _queue.Info.UpdateHighPriorityMessageCount(_queue.HighPriorityLinkedList.Count);
            _queue.Info.UpdateRegularMessageCount(_queue.RegularLinkedList.Count);
            return PushResult.Success;
        }
    }
}
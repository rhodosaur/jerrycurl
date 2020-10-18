﻿using System;
using System.Collections.Generic;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Relations.Internal.Queues
{
    internal class RelationQueue<TList, TItem> : IRelationQueue
        where TList : IEnumerable<TItem>
    {
        private IEnumerator<TItem> innerEnumerator;

        public TList List => this.CurrentItem.List;
        public TItem Current => this.innerEnumerator.Current;
        public RelationQueueItem<TList> CurrentItem => this.innerQueue.Peek();
        public int Index => this.CurrentItem.Index;
        public int Depth => this.Metadata.Depth;
        public RelationQueueType Type { get; }
        public IRelationMetadata Metadata { get; }

        private Queue<RelationQueueItem<TList>> innerQueue = new Queue<RelationQueueItem<TList>>();
        private readonly List<RelationQueueItem<TList>> innerCache = new List<RelationQueueItem<TList>>();
        private bool usingCache = false;

        public RelationQueue(IRelationMetadata metadata, RelationQueueType queueType)
        {
            this.Metadata = metadata;
            this.Type = queueType;
        }

        public void Enqueue(RelationQueueItem<TList> item)
        {
            if (this.Type == RelationQueueType.Cartesian)
            {
                this.innerCache.Clear();
                this.innerQueue.Clear();
                this.usingCache = false;
            }

            if (!this.usingCache)
                this.innerQueue.Enqueue(item);

            if (this.innerEnumerator == null || this.Type == RelationQueueType.Cartesian)
                this.Start();
        }

        private void Start()
        {
            this.innerEnumerator?.Dispose();
            this.innerEnumerator = null;

            if (this.innerQueue.Count > 0)
            {
                this.CurrentItem.Reset();
                this.innerEnumerator = (this.CurrentItem.List ?? (IEnumerable<TItem>)Array.Empty<TItem>()).GetEnumerator();
            }
        }

        public string GetFieldName(string namePart) => this.CurrentItem.CombineWith(namePart);

        public bool Read()
        {
            while (this.innerQueue.Count > 0)
            {
                if (this.innerEnumerator.MoveNext())
                {
                    this.CurrentItem.Increment();

                    return true;
                }

                this.Dequeue();
            }

            if (this.Type == RelationQueueType.Cartesian)
                this.EnqueueCached();

            return false;
        }

        private void EnqueueCached()
        {
            this.innerQueue = new Queue<RelationQueueItem<TList>>(this.innerCache);
            this.usingCache = true;
            this.Start();
        }

        private void Dequeue()
        {
            RelationQueueItem<TList> dequeued = this.innerQueue.Dequeue();

            if (!this.usingCache && this.Type == RelationQueueType.Cartesian)
                this.innerCache.Add(dequeued);

            this.Start();
        }

        public void Dispose()
        {
            this.innerEnumerator?.Dispose();
        }
    }
}

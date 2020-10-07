﻿using Jerrycurl.Data.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Data.Queries.Internal.Nodes
{
    internal class AggregateNode
    {
        public MetadataNode Item { get; set; }
        public IBindingMetadata Metadata { get; set; }
        public int Index { get; set; }
    }
}

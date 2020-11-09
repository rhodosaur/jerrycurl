﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Data.Queries.Internal.IO.Readers;

namespace Jerrycurl.Data.Queries.Internal.IO.Targets
{
    internal class ListTarget2
    {
        public int Index { get; set; }
        public ParameterExpression Variable { get; set; }
        public MethodInfo AddMethod { get; set; }
        public ConstructorInfo NewList { get; set; }
    }
}

﻿using System.Collections.Generic;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Data.Test.Models;
using Jerrycurl.Relations;

namespace Jerrycurl.Data.Test.Model.Custom
{
    public class PriorityModel
    {
        [Key("X")]
        public int Id1 { get; set; }
        [Ref("Y")]
        public int Id2 { get; set; }

        public IList<InnerModel> Many { get; set; }
        public One<InnerModel> One { get; set; }
        public CustomList<InnerModel> Custom { get; set; }
        [One]
        public InnerModel One2 { get; set; }

        public class InnerModel
        {
            [Ref("X")]
            public int PriorityId1 { get; set; }

            [Key("Y")]
            public int PriorityId2 { get; set; }
        }
    }
}

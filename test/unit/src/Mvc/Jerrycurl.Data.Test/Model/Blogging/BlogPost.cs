﻿using System.Collections.Generic;
using Jerrycurl.Data.Metadata.Annotations;

namespace Jerrycurl.Data.Test.Model.Blogging
{
    internal class BlogPost
    {
        [Key("PK_BlogPost")]
        public int Id { get; set; }
        [Ref("PK_Blog")]
        public int BlogId { get; set; }
        [Ref("PK_Blog_2")]
        public int BlogId2 { get; set; }
        [Ref("PK_Blog_3")]
        public string BlogId3 { get; set; }
        [Ref("PK_Blog_4")]
        public int? BlogId4 { get; set; }
        [Ref("PK_Blog_5")]
        public int? BlogId5 { get; set; }
        public string Headline { get; set; }
        public string Content { get; set; }

        public IList<BlogComment> Comments { get; set; }
    }
}
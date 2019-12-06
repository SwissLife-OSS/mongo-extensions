using System;
using System.Collections.Generic;

namespace Models
{
    public class BlogPost
    {
        public string Titel { get; set; }
        public string Text { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public string UserId { get; set; }
    }

    public class Blog
    {
        public Guid Id { get; set; }
        public string Titel { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public string UserId { get; set; }
    }
}

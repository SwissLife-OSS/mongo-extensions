using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Blog
    {
        public string Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public IEnumerable<Tag> Tags { get; set; }

        public string Text { get; set; }

        public string UserId { get; set; }
    }
}

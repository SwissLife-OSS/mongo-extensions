using System;
using System.Collections.Generic;

namespace Models
{
    public class User
    {
        public User()
        {
            Posts = new List<Guid>();
        }

        public string UserId { get; set; }

        public string Email { get; set; }

        public string Nickname { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public IEnumerable<Guid> Posts { get; set; }
    }
}

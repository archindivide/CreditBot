using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreditBot
{
    public class User
    {
        public User(string u, int v)
        {
            UserName = u;
            Value = v;
        }

        public string UserName { get; set; }
        public int Value { get; set; }
    }
}

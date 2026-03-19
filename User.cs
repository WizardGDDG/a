using System;
using System.Collections.Generic;

namespace RussianLanguageTextbook
{
    [Serializable]
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public int Score { get; set; }
    }
}

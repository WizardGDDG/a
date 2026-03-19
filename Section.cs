using System;
using System.Collections.Generic;

namespace RussianLanguageTextbook
{
    [Serializable]
    public class Section
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<SimpleQuestion> Questions { get; set; } = new List<SimpleQuestion>();
    }

    [Serializable]
    public class SimpleQuestion
    {
        public string Text { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectOption { get; set; } // 0, 1, 2 или 3
    }
}

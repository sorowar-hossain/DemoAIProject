using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class IntentData
    {
        public string Text { get; set; } = "";
        public string Intent { get; set; } = "";
        public List<string> Tokens { get; set; } = new();

        public Dictionary<string, double> TFIDF { get; set; } = new();
        public Dictionary<string, double> TF { get; set; } = new();
    }
}

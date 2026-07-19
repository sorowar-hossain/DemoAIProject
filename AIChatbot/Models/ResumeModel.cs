using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class ResumeModel
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string Content { get; set; }

        public List<string> Tokens { get; set; } = new();

        public Dictionary<string, double> TFIDF { get; set; } = new();
        public Dictionary<string, double> TF { get; set; } = new(); 
        public double Score { get; set; }
    }
}

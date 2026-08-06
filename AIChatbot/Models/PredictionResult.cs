using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class PredictionResult
    {
        public string Sentiment { get; set; }
        public Dictionary<string, double> Scores { get; set; }
    }
}

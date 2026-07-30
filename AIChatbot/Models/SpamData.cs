using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class SpamData
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}

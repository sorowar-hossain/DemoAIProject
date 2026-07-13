using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class KnowledgeItem
    {
        public int Id { get; set; }

        public string Question { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public List<string> Keywords { get; set; } = new();
    }
}

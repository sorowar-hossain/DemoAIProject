using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class Intent
    {
        public string Name { get; set; }

        public List<string> Patterns { get; set; }

        public List<string> Responses { get; set; }
    }
}

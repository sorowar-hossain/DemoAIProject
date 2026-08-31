using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class SemanticSearchResult
    {
        public EmbedingDataModel Document { get; set; } = null!;
        public double Score { get; set; }
    }
}

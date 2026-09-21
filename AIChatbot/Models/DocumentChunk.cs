using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class DocumentChunk
    {
        public int Id { get; set; }

        public int PageNumber { get; set; }

        public string Text { get; set; } = string.Empty;
        public double[] Embedding {  get; set; }
        public string SectionTitle { get; internal set; }
    }
}

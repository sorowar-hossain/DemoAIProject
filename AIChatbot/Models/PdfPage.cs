using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class PdfPage
    {
        public int PageNumber { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}

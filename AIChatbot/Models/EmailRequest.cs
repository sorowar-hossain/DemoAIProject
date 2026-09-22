using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Models
{
    public class EmailRequest
    {
        public string Recipient { get; set; } = "";
        public string Purpose { get; set; } = "";
        public string Reason { get; set; } = "";
        public string Duration { get; set; } = "";

        // Tone means the style or manner in which the email should be written. 
        // such as Professional,Formal, Friendly
        public string Tone { get; set; } = "";
    }
}

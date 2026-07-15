using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Services
{
    public class TF_IDF_Service
    {
       /* Remember this sentence:

           TF tells us "How important is this word in this document?" 
           and IDF tells us "How unique is this word across all documents or corpus?" 
           TF-IDF combines both to identify the most representative keywords of a document.
       
        */
        private readonly TF_TermFrequencyService tfObj;
        private readonly IDF_InverseDocumentFrequencyService idfObj;

        private readonly Dictionary<string, double> tf;
        private readonly Dictionary<string, double> idf;

        public List<string> tokens = new()
                            {
                                "love",
                                "ai",
                                "because",
                                "ai",
                                "future",
                                "ai"
                            };
        public TF_IDF_Service()
        {
            tfObj = new TF_TermFrequencyService();
            idfObj = new IDF_InverseDocumentFrequencyService();

            tf = tfObj.CalculateTF(tokens);
            idf = idfObj.CalculateIDF();
        }

        public Dictionary<string, double> CalculateTFIDF()
        {
            Dictionary<string, double> tfidf = new();

            foreach (var word in tf.Keys)
            {
                if (idf.TryGetValue(word, out double idfValue))
                {
                    tfidf[word] = tf[word] * idfValue;
                }
            }

            return tfidf;
        }
    }
}

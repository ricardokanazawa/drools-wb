using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Extraction.Export
{
    public class AssociationRuleExport
    {
        private string RootPath { get; set; }
        private int ProviderId { get; set; }
        private FrequentClaimService DominantService { get; set; }
        private bool ConsequentRules { get; set; }

        public AssociationRuleExport(string path, int providerId, bool consequentRules, FrequentClaimService dominantService)
        {
            RootPath = path;
            ProviderId = providerId;
            DominantService = dominantService;
            ConsequentRules = consequentRules;
        }

        public void WriteToTransactionFile(List<Claim> claims, out int totalServices)
        {
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);

            var filepath = Path.Combine(RootPath, ProviderId + ".txt");

            if (File.Exists(filepath))
                File.Delete(filepath);

            totalServices = 0;
            using (StreamWriter writer = File.AppendText(filepath))
            {  
                for (int i = 0; i < claims.Count; i++)
                {
                    var sb = new StringBuilder();
                    foreach (var item in claims[i].Services)
                    {
                        totalServices++;
                        sb.Append(item.TableCode.PadLeft(2, '0'));
                        sb.Append("|");
                        sb.Append(item.ProcedureCode);
                        sb.Append(" ");
                    }

                    writer.WriteLine(sb.ToString());
                }                    
            }
        }        
    }
}

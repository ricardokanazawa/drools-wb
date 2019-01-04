using Extration.Export;
using Extration.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static Extration.Models.IntegracaoGestaoPlanosContext;

namespace Extration
{
    class Program
    {
        static void Main(string[] args)
        {   
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Console.WriteLine($"{ sw.Elapsed } Capturando dados...");

            var claims2 = new List<Claim>();

            using (var db = new IntegracaoGestaoPlanosContext())
            {
                List<TransacaoBeneficiario> lst = db.Transacoes.FromSql("[SP_TRANSACOES_EXP] @Id_Prestador = {0}, @Dt_inicio = {1}, @Dt_fim = {2}, @Cd_Procedimento = {3}",
                    1, new DateTime(2017, 1, 1), new DateTime(2018, 1, 1), "60002450").ToList();

                foreach (var transacao in lst)
                {
                    List<TransacaoBeneficiario> lst2 = db.Transacoes.FromSql("[SP_TRANSACOES_EXP] @Id_Prestador = {0}, @Dt_inicio = {1}, @Dt_fim = {2}, @Carteira = {3}",
                    1, new DateTime(2017, 1, 1), new DateTime(2018, 1, 1), transacao.Carteira).ToList();

                    foreach (var transacaoFim in lst2.Where(l => l.DataExecucao <= transacao.DataExecucao))
                    {
                       Claim claim = null;
                        var claimService = new ClaimService { ProcedureCode = transacaoFim.CodigoProcedimento, TableCode = transacaoFim.CodigoTabela.ToString(), UnitCost = transacaoFim.ValorUnitario };

                        if ((claim = claims2.FirstOrDefault(c => c.AuthorizationId == transacaoFim.Carteira)) != null)
                        {
                            if (!claim.Services.Any(s => s.Key == claimService.Key))
                                claim.Services.Add(claimService);
                        }
                        else
                        {
                            claims2.Add(new Claim
                            {
                                AuthorizationId = transacaoFim.Carteira,
                                Services = new HashSet<ClaimService> { claimService }
                            });
                        }
                    }                       
                }
            }
            var export = new AssociationRuleExport(@"c:\temp\transacoes\Prestador 1\", 61100016, true, null); // TMO
            export.WriteToTransactionFile(claims2, out int total);

            Console.WriteLine($"Guias: { claims2.Count }, Serviços: { total }");            
        }
    }
}

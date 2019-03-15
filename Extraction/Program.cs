using FpGrowthCore.Export;
using FpGrowthCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static FpGrowthCore.Models.IntegracaoGestaoPlanosContext;

namespace FpGrowthCore
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
                for (int i = 1; i <= 12; i++)
                {
                    var dataIni = new DateTime(2017, i, 1);
                    var dataFim = dataIni.AddMonths(1).AddSeconds(-1);

                    List<TransacaoBeneficiario> lst = db.Transacoes.FromSql("[SP_TRANSACOES_EXP] @Id_Prestador = {0}, @Dt_inicio = {1}, @Dt_fim = {2}",
                    2, dataIni, dataFim).ToList();

                    foreach (var transacao in lst.GroupBy(x => x.NumeroGuia))
                    {
                        if (transacao.FirstOrDefault(s => s.CodigoTabela == 22) == null)
                            continue;

                        if (transacao.Count() == 1 || transacao.Count() > 50)
                            continue;

                        if (transacao.All(s => s.CodigoTabela == 22))
                            continue;                        

                        foreach (var servico in transacao)
                        {
                            Claim claim = null;
                            var claimService = new ClaimService { ProcedureCode = servico.CodigoProcedimento, TableCode = servico.CodigoTabela.ToString(), UnitCost = servico.ValorUnitario };

                            if ((claim = claims2.FirstOrDefault(c => c.AuthorizationId == transacao.Key)) != null)
                            {
                                if (!claim.Services.Any(s => s.Key == claimService.Key))
                                    claim.Services.Add(claimService);
                            }
                            else
                            {
                                claims2.Add(new Claim
                                {
                                    AuthorizationId = transacao.Key,
                                    Services = new HashSet<ClaimService> { claimService }
                                });
                            }
                        }
                    }

                    var export = new AssociationRuleExport(@"c:\temp\transacoes\Prestador 2\", i, false, null); // TMO
                    export.WriteToTransactionFile(claims2, out int total);

                    Console.WriteLine($"Guias: { claims2.Count }, Serviços: { total }");
                    claims2 = new List<Claim>();
                }                
            }

            Console.ReadLine();
        }
    }
}

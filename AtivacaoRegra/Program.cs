using Dapper;
using FpGrowthCore.Export;
using FpGrowthCore.Models;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static FpGrowthCore.Models.EtlSistemasContext;
using static FpGrowthCore.Models.IntegracaoGestaoPlanosContext;

namespace FpGrowthCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //Extract();

            int codigoPrestador =3;
            Validate(codigoPrestador);
        }


        private static void Validate(int codigoPrestador)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Console.WriteLine($"{ sw.Elapsed } Capturando dados...");

            var claims2 = new List<Claim>();

            using (var db = new IntegracaoGestaoPlanosContext())
            {
                var client = new RestClient("http://35.198.48.120:8081/kie-server/services/rest/server/containers/instances/ExpertSystem");
                client.Timeout = 5000;

                ControleDrools controleDrools = new ControleDrools { maxRegras = 3 };

                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"{ sw.Elapsed } Processando mês { i }...");

                    if (!Directory.Exists(@"C:\Users\ricar\Downloads\transacoes-completo\PRESTADOR " + codigoPrestador + @"\TRANSACOES_PAGAS\MES " + i + @"\"))
                        Directory.CreateDirectory(@"C:\Users\ricar\Downloads\transacoes-completo\PRESTADOR " + codigoPrestador + @"\TRANSACOES_PAGAS\MES " + i + @"\");

                    var dataIni = new DateTime(2018, i, 1);
                    var dataFim = dataIni.AddMonths(1).AddSeconds(-1);

                    List<TransacaoBeneficiario> lst = db.Transacoes.FromSql("[DATABASE].[SP_TRANSACOES_EXP] @Id_Prestador = {0}, @Dt_inicio = {1}, @Dt_fim = {2}",
                    codigoPrestador, dataIni, dataFim).ToList();

                    CancellationTokenSource cts = new CancellationTokenSource();

                    ParallelOptions po = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 1,
                        CancellationToken = cts.Token
                    };

                    Parallel.ForEach(lst.GroupBy(x => x.NumeroGuia), po, transacao =>
                    {
                        //if (transacao.FirstOrDefault(s => s.CodigoTabela == 22) == null)
                        //    return;

                        if (transacao.Count() == 1 || transacao.Count() > 50)
                            return;

                        //if (transacao.All(s => s.CodigoTabela == 22))
                        //    return;

                        Console.WriteLine($"{ sw.Elapsed } Processando transação { transacao.Key }...");

                        BatchExecution batchExecution = new BatchExecution
                        {
                            inserts = new List<InsertCommand>(),
                            queryDrools = new QueryDrools
                            {
                                name = "queryValidacao",
                                outIdentifier = "validacoes"
                            }
                        };

                        GuiaDrools guiaDrools = new GuiaDrools
                        {
                            id = transacao.Key,
                            CodigoPrestador = codigoPrestador,
                            Movimentos = new List<MovimentoDrools>()
                        };

                        foreach (var servico in transacao)
                        {
                            guiaDrools.Movimentos.Add(new MovimentoDrools
                            {
                                codigoMovimento = servico.CodigoProcedimento,
                                codigoTabela = servico.CodigoTabela.ToString(),
                                dataExecucaoFormatado = servico.DataExecucao,
                                descricao = servico.DescricaoProcedimento,
                                valorUnitario = servico.ValorUnitario
                            });
                        }

                        batchExecution.inserts.Add(new InsertCommand { command = guiaDrools });
                        batchExecution.inserts.Add(new InsertCommand { command = controleDrools });

                        var request = new RestRequest(Method.POST);
                        request.AddHeader("postman-token", "91102cb1-394d-7619-49ab-bf50f217a8ae");
                        request.AddHeader("cache-control", "no-cache");
                        request.AddHeader("x-kie-contenttype", "XSTREAM");
                        request.AddHeader("authorization", "Basic YWRtaW46YWRtaW4=");
                        request.AddHeader("content-type", "application/xml");

                        var body = SerializeToXML(batchExecution, codigoPrestador, transacao.Key, i);
                        request.AddParameter("application/xml", body, ParameterType.RequestBody);


                        IRestResponse response = client.Execute(request);

                        Console.WriteLine($"{ sw.Elapsed } Retorno transação { transacao.Key }: {  response.StatusCode }");

                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            //Console.WriteLine($"{ sw.Elapsed } BODY ERRO: { body }");
                            Console.WriteLine($"{ sw.Elapsed } RETORNO ERRO: { response.StatusCode }");

                            using (TextWriter txtWriter = new StreamWriter(@"C:\Users\ricar\Downloads\transacoes-completo\PRESTADOR " + codigoPrestador + @"\TRANSACOES_PAGAS\MES " + i + @"\" + transacao.Key + "_ERRO.txt"))
                            {
                                txtWriter.Write(response.StatusCode);
                                txtWriter.Close();
                            }
                            Thread.Sleep(5000);
                            //cts.Cancel();                            
                        }

                        //po.CancellationToken.ThrowIfCancellationRequested();

                        if (!response.Content.Contains("com.ops.expertsystem.analise.ResultadoItem"))
                            return;

                        using (TextWriter txtWriter = new StreamWriter(@"C:\Users\ricar\Downloads\transacoes-completo\PRESTADOR " + codigoPrestador + @"\TRANSACOES_PAGAS\MES " + i + @"\" + transacao.Key + "_RESPONSE.xml"))
                        {
                            txtWriter.Write(response.Content);
                            txtWriter.Close();

                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(response.Content);

                            var descriptions = xmlDoc.GetElementsByTagName("descricao");

                            if (descriptions.Count > 0)
                            {
                                var claimYear = transacao.Key.Substring(0, 4);
                                var claimNumber = transacao.Key.Substring(4);

                                List<ClaimItem> claimItems = null;
                                using (SqlConnection connectionBD = new SqlConnection(@"Data Source=""))
                                {
                                    claimItems = connectionBD.Query<ClaimItem>(
                                        "USP_GUIA_INSUMOS_SEL",
                                        new { anoGuia = claimYear, numeroGuia = claimNumber },
                                        commandTimeout: 360,
                                        commandType: CommandType.StoredProcedure).ToList();

                                    //claimItems.RemoveAll(x => x.vl_real_pago == x.vl_cobrado);
                                    claimItems = claimItems.Where(x => x.vl_real_pago != x.vl_cobrado).ToList();

                                    if (claimItems.Count > 0)
                                    {
                                        HashSet<int> itensChecked = new HashSet<int>();

                                        foreach (XmlNode description in descriptions)
                                        {
                                            if (description.InnerText.Contains("a partir"))
                                            {
                                                var startIndex = description.InnerText.IndexOf('{');
                                                var endIndex = description.InnerText.IndexOf('}');

                                                var itensRules = description.InnerText.Substring(startIndex + 1, endIndex - startIndex - 1).Split(new[] { ',' }).ToList();

                                                var intItensRules = itensRules.Select(x => Convert.ToInt32(x)).ToList();

                                                foreach (var item in claimItems)
                                                {
                                                    if (!intItensRules.Contains(item.cd_insumo))
                                                    {
                                                        itensChecked.Add(item.cd_insumo);
                                                    }
                                                }
                                            }
                                        }

                                        if (itensChecked.Count > 0)
                                        {
                                            using (TextWriter txtWriter2 = new StreamWriter(@"C:\Users\ricar\Downloads\transacoes-completo\PRESTADOR " + codigoPrestador + @"\TRANSACOES_PAGAS\MES " + i + @"\glosados.txt", true))
                                            {
                                                txtWriter2.Write(transacao.Key);
                                                txtWriter2.Write(" ");
                                                foreach (var item in itensChecked)
                                                {
                                                    txtWriter2.Write(item + " ");
                                                }
                                                txtWriter2.WriteLine();
                                            }
                                        }                                        
                                    }
                                }
                            }
                        }
                    });
                }
            }

            Console.WriteLine($"{ sw.Elapsed } FIM");
            Console.ReadLine();
        }

        static public string SerializeToXML(BatchExecution batchExecution, int codigoPrestador, string id, int mes)
        {
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            XmlSerializer serializer = new XmlSerializer(typeof(BatchExecution), new Type[] { typeof(GuiaDrools), typeof(ControleDrools) });

            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (TextWriter txtWriter = new StreamWriter(@"C:\Users\ricar\Downloads\transacoes-completo\PRESTADOR " + codigoPrestador + @"\TRANSACOES_PAGAS\MES " + mes + @"\" + id + "_REQUEST.xml"))
            {
                serializer.Serialize(txtWriter, batchExecution);
                txtWriter.Close();
            }

            using (StringWriter textWriter = new StringWriter())
            {
                using (var writer = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(writer, batchExecution, emptyNamespaces);
                    return textWriter.ToString();
                }
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FpGrowthCore.Models
{
    public class EtlSistemasContext : DbContext
    {
        public DbSet<Guia> Guias { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guia>(entity => {
                entity.HasKey(e => new { e.NumeroGuia, e.CodigoProcedimento, e.CodigoTabela, e.DataExecucao, e.QuantidadeExecutada, e.ValorUnitario });
                entity.ToTable("vw_TMP_SERVICOS", "PortalPrestador");
                entity.Property(e => e.CodigoProcedimento);
                entity.Property(e => e.DescricaoProcedimento);
                entity.Property(e => e.QuantidadeExecutada);
                entity.Property(e => e.ValorUnitario);
                entity.Property(e => e.CodigoTabela);
                entity.Property(e => e.DataExecucao);
            });
        }

        public class Guia
        {
            public string NumeroGuia { get; set; }
            public string CodigoProcedimento { get; set; }
            public string DescricaoProcedimento { get; set; }
            public double QuantidadeExecutada { get; set; }
            public double ValorUnitario { get; set; }
            public string CodigoTabela { get; set; }
            public DateTime DataExecucao { get; set; }
        }

        [XmlRoot("batch-execution")]
        [XmlInclude(typeof(GuiaDrools))]
        public class BatchExecution
        {
            [XmlAttribute("lookup")]
            public string lookup = "MyStateless";
              
            [XmlElement(Order = 1, ElementName = "insert")]            
            public List<InsertCommand> inserts { get; set; }

            [XmlElement("fire-all-rules", Order = 2)]
            public object fireAllRules = new object();

            [XmlElement("query", Order = 3)]
            public QueryDrools queryDrools;
        }
               
        public class InsertCommand
        {
            [XmlElement(Type = typeof(GuiaDrools)), XmlElement(Type = typeof(ControleDrools))]
            public object command { get; set; }
        }

        [XmlType("query")]
        public class QueryDrools
        {
            [XmlAttribute("out-identifier")]
            public string outIdentifier { get; set; }

            [XmlAttribute("name")]
            public string name { get; set; }
        }

        [XmlType("com.ops.expertsystem.Guia")]
        public class GuiaDrools
        {
            [XmlElement("id")]
            public string id { get; set; }

            [XmlElement("codigoPrestador")]
            public int CodigoPrestador { get; set; }

            [XmlArray("movimentos")]
            [XmlArrayItem(Type = typeof(MovimentoDrools))]
            public List<MovimentoDrools> Movimentos { get; set; }
        }

        [XmlType("com.ops.expertsystem.Controle")]
        public class ControleDrools
        {
            [XmlElement("maxRegras")]
            public int maxRegras { get; set; }
        }

        [XmlType("com.ops.expertsystem.Movimento")]
        public class MovimentoDrools
        {
            [XmlElement("codigoTabela")]
            public string codigoTabela { get; set; }

            [XmlElement("codigoMovimento")]
            public string codigoMovimento { get; set; }

            [XmlElement("descricao")]
            public string descricao { get; set; }

            [XmlElement("valorUnitario")]
            public double valorUnitario { get; set; }

            [XmlElement("quantidadeExecutada")]
            public double quantidadeExecutada { get; set; }

            [XmlElement("dataExecucao")]
            public string dataExecucao {
                get { return dataExecucaoFormatado.ToString("yyyy-MM-dd HH:mm:ss") + ".0 UTC"; }
                set { dataExecucaoFormatado = DateTime.Parse(value); }
            }

            [XmlIgnore]
            public DateTime dataExecucaoFormatado { get; set; }            
        }
    }
}

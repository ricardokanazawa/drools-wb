using Microsoft.EntityFrameworkCore;
using System;

namespace Extraction.Models
{
    public class IntegracaoGestaoPlanosContext : DbContext
    {
        public DbSet<TransacaoBeneficiario> Transacoes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=SERVER\DESENV;Database=2017;Trusted_Connection=True;ConnectRetryCount=0");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransacaoBeneficiario>(entity => {
                entity.HasKey(e => new { e.NumeroGuia, e.Carteira, e.CodigoProcedimento, e.CodigoTabela, e.DescricaoProcedimento, e.ValorUnitario, e.DataExecucao });
                entity.Property(e => e.Carteira);
                entity.Property(e => e.CodigoProcedimento);
                entity.Property(e => e.DescricaoProcedimento);
                entity.Property(e => e.ValorUnitario);
                entity.Property(e => e.CodigoTabela);
                entity.Property(e => e.DataExecucao);
            });
        }

        public class TransacaoBeneficiario
        {
            public string NumeroGuia { get; set; }
            public string Carteira { get; set; }
            public string CodigoProcedimento { get; set; }
            public string DescricaoProcedimento { get; set; }
            public double ValorUnitario { get; set; }
            public int CodigoTabela { get; set; }
            public DateTime DataExecucao { get; set; }
        }
    }
}

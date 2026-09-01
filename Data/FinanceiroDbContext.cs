using Microsoft.EntityFrameworkCore;
using FinanceiroApi.Models;

/*
 DbContext: nossa classe herda de DbContext, a classe base do EF Core — 
isso é o que dá a ela todo o comportamento de conversar com o banco 
(sem precisarmos escrever esse código, ele já vem pronto na biblioteca).
Construtor recebendo DbContextOptions<FinanceiroDbContext> options: mesmo padrão de injeção de dependência que já usamos no RepositorioPagamentos 
 o ASP.NET Core vai entregar essas opções (que incluem a string de conexão) automaticamente, configuradas lá no Program.cs.
DbSet<PagamentoFinanceiro> Pagamentos: isso representa uma tabela no banco. O EF Core usa esse nome (Pagamentos) como nome da tabela por convenção, 
e cada propriedade pública da classe PagamentoFinanceiro vira uma coluna.
 */

namespace FinanceiroApi.Data
{
    public class FinanceiroDbContext : DbContext
    {
        public FinanceiroDbContext(DbContextOptions<FinanceiroDbContext> options) 
            : base(options) 
        { 
        }

        public DbSet<PagamentoFinanceiro> Pagamentos { get; set; }
    }
}

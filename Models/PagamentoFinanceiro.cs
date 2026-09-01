//cada get / set serve como uma gaveta, eles servem para guardar e modificar itens
namespace FinanceiroApi.Models
{
    public class PagamentoFinanceiro
    {
        public int Id { get; set; }
        public string RazaoSocialPagador { get; set; } = string.Empty;
        public string CnpjPagador { get; set; } = string.Empty;
        public string Fornecedor { get; set; } = string.Empty;
        public string CnpjFornecedor { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public DateTime DataVencimento { get; set; }
        public string Observacoes { get; set; } = string.Empty;
        public string CaminhoArquivoAnexo { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}
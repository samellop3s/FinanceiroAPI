namespace FinanceiroApi.Models
{
   /* <summary>
    //Representa os dados brutos recebidos do formulario em html,
    //antes de serem validados e convertidos.
    </summary>*/
    public class PagamentoRequest
    {
        public string RazaoSocialPagador { get; set; } = string.Empty;
        public string CnpjPagador { get; set; } = string.Empty;
        public string Fornecedor { get; set; } = string.Empty;
        public string CnpjFornecedor { get; set; } = string.Empty;
        public string ValorTotal { get; set; } = string.Empty;   // ainda como texto!
        public DateTime DataVencimento { get; set; }
        public string Observacoes { get; set; } = string.Empty;
        public IFormFile? Anexo { get; set; }                    // arquivo, pode não vir
    }
}

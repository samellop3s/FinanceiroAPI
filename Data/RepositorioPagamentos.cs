using FinanceiroApi.Models;
using Microsoft.EntityFrameworkCore;


/*FinanceiroDbContext _contexto: agora recebemos o DbContext via injeção de dependência, em vez de manipular arquivos diretamente.
_contexto.Pagamentos.Add(pagamento): adiciona o novo pagamento "na memória" do contexto — ainda não gravou no banco, só marcou como "pendente de inserção".
_contexto.SaveChanges(): aqui sim o EF Core monta o INSERT e executa de verdade no PostgreSQL.
_contexto.Pagamentos.OrderByDescending(p => p.DataCadastro).ToList(): consulta a tabela, ordenando pelos mais recentes primeiro, e traz tudo como uma lista C# 
— o EF Core traduz isso automaticamente para um SELECT ... ORDER BY em SQL.
A parte de anexos continua igual — o EF Core cuida dos dados estruturados (textos, números, datas), mas arquivos continuam sendo geridos manualmente em disco, 
prática comum mesmo em sistemas com banco de dados robusto.
*/
namespace FinanceiroApi.Data
{
    public class RepositorioPagamentos
    {
        private readonly FinanceiroDbContext _contexto;
        private readonly IWebHostEnvironment _ambiente;
        private readonly string _pastaAnexos;

        public RepositorioPagamentos(FinanceiroDbContext contexto, IWebHostEnvironment ambiente)
        {
            _contexto = contexto;
            _ambiente = ambiente;

            _pastaAnexos = Path.Combine(_ambiente.ContentRootPath, "DadosApp", "Anexos");
            Directory.CreateDirectory(_pastaAnexos);
        }

        public string CopiarAnexo(Stream conteudoArquivo, string nomeArquivoOriginal)
        {
            string nomeArquivo = Path.GetFileName(nomeArquivoOriginal);
            string destino = Path.Combine(_pastaAnexos, nomeArquivo);

            int contador = 1;
            string nomeSemExtensao = Path.GetFileNameWithoutExtension(nomeArquivo);
            string extensao = Path.GetExtension(nomeArquivo);

            while (File.Exists(destino))
            {
                destino = Path.Combine(_pastaAnexos, $"{nomeSemExtensao}_{contador}{extensao}");
                contador++;
            }

            using (var arquivoDestino = File.Create(destino))
            {
                conteudoArquivo.CopyTo(arquivoDestino);
            }

            return destino;
        }

        public string ObterCaminhoCompletoAnexo(string nomeArquivo)
        {
            return Path.Combine(_pastaAnexos, nomeArquivo);
        }

        public void Salvar(PagamentoFinanceiro pagamento)
        {
            _contexto.Pagamentos.Add(pagamento);
            _contexto.SaveChanges();
        }

        public PagamentoFinanceiro? ObterPorId(int id)
        {
            return _contexto.Pagamentos.FirstOrDefault(p => p.Id == id);
        }

        public void Atualizar(PagamentoFinanceiro pagamento)
        {
            _contexto.Pagamentos.Update(pagamento);
            _contexto.SaveChanges();
        }

        public void Excluir(int id)
        {
            var pagamento = _contexto.Pagamentos.FirstOrDefault(p => p.Id == id);

            if (pagamento == null)
                return;

            _contexto.Pagamentos.Remove(pagamento);
            _contexto.SaveChanges();

            if (!string.IsNullOrWhiteSpace(pagamento.CaminhoArquivoAnexo))
            {
                try
                {
                    if (File.Exists(pagamento.CaminhoArquivoAnexo))
                        File.Delete(pagamento.CaminhoArquivoAnexo);
                }
                catch
                {
                }
            }
        }

        public List<PagamentoFinanceiro> CarregarTodos() //fazer conexão do banco de dados para salvar pagamentos 
        {
            return _contexto.Pagamentos
                .OrderByDescending(p => p.DataCadastro)
                .ToList();
        }
    }
}
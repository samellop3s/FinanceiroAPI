using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FinanceiroApi.Data;
using FinanceiroApi.Helpers;
using FinanceiroApi.Models;
using System.Globalization;

namespace FinanceiroApi.Controllers
{
    /*ApiController: avisa o ASP.NET Core que essa classe é um controlador de 
     * API (ativa validações automáticas e outras conveniências).*/
    [ApiController]
    /*("api/[controller]"): define o "endereço" desse controller. O [controller] 
     * é substituído automaticamente pelo nome da classe sem o sufixo 
     * "Controller" — então esse controller responde em api/Pagamentos.*/
    [Route("api/[controller]")]

    /*ControllerBase: classe base do ASP.NET Core pra controllers de API (sem suporte a views HTML, que não precisamos aqui).*/
    public class PagamentosController : ControllerBase
    {
        /*RepositorioPagamentos: é aqui que a injeção de dependência que configuramos na Etapa 27 entra em ação 
         * — o ASP.NET Core vê que esse Controller "pede" um RepositorioPagamentos, 
         * lembra que você registrou ele com AddSingleton, e entrega uma instância pronta,
         * sem você precisar instanciar manualmente.*/
        private readonly RepositorioPagamentos _repositorio;

        public PagamentosController(RepositorioPagamentos repositorio)
        {
            _repositorio = repositorio;
        }
        [HttpGet]
        public ActionResult<List<PagamentoFinanceiro>> ListarTodos()
        {
            return _repositorio.CarregarTodos();
        }

        [HttpGet("{id}")]
        public IActionResult ObterPagamento(int id)
        {
            var pagamento = _repositorio.ObterPorId(id);

            if (pagamento == null)
                return NotFound();

            return Ok(pagamento);
        }

        [HttpPost]
        public IActionResult Salvar([FromForm] PagamentoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RazaoSocialPagador))
                return BadRequest("Informe a razão social do pagador.");
            if (string.IsNullOrWhiteSpace(request.Fornecedor))
                return BadRequest("Informe o nome do fornecedor.");
            if (!CnpjHelper.IsValid(request.CnpjPagador))
                return BadRequest("CNPJ do pagador invalido.");
            if (!CnpjHelper.IsValid(request.CnpjFornecedor))
                return BadRequest("CNPJ do fornecedor invalido.");

            if (!decimal.TryParse(
                request.ValorTotal,
                NumberStyles.Number,
                CultureInfo.GetCultureInfo("pt-BR"),
                out decimal valorTotal) || valorTotal <= 0)
            {
                return BadRequest("Informe um valor total válido, maior que zero");
            }

            string caminhoAnexoFinal = string.Empty;

            if (request.Anexo != null && request.Anexo.Length > 0)
            {
                using var streamArquivo = request.Anexo.OpenReadStream();
                caminhoAnexoFinal = _repositorio.CopiarAnexo(streamArquivo, request.Anexo.FileName);
            }
            var pagamento = new PagamentoFinanceiro
            {
                RazaoSocialPagador = request.RazaoSocialPagador.Trim(),
                CnpjPagador = CnpjHelper.SomenteNumeros(request.CnpjPagador),
                Fornecedor = request.Fornecedor.Trim(),
                CnpjFornecedor = CnpjHelper.SomenteNumeros(request.CnpjFornecedor),
                ValorTotal = valorTotal,
                DataVencimento = DateTime.SpecifyKind(request.DataVencimento.Date, DateTimeKind.Utc),
                Observacoes = request.Observacoes.Trim(),
                CaminhoArquivoAnexo = caminhoAnexoFinal
            };//teste anydesk

            _repositorio.Salvar(pagamento);

            return Ok(new { mensagem = "Pagamento cadastrado com sucesso!" });
        }

        [HttpPut("{id}")]
        public IActionResult Atualizar(int id, [FromForm] PagamentoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RazaoSocialPagador))
                return BadRequest("Informe a razão social do pagador.");
            if (string.IsNullOrWhiteSpace(request.Fornecedor))
                return BadRequest("Informe o nome do fornecedor.");
            if (!CnpjHelper.IsValid(request.CnpjPagador))
                return BadRequest("CNPJ do pagador invalido.");
            if (!CnpjHelper.IsValid(request.CnpjFornecedor))
                return BadRequest("CNPJ do fornecedor invalido.");

            if (!decimal.TryParse(
                request.ValorTotal,
                NumberStyles.Number,
                CultureInfo.GetCultureInfo("pt-BR"),
                out decimal valorTotal) || valorTotal <= 0)
            {
                return BadRequest("Informe um valor total válido, maior que zero");
            }

            var pagamento = _repositorio.ObterPorId(id);

            if (pagamento == null)
                return NotFound();

            string caminhoAnexoAntigo = pagamento.CaminhoArquivoAnexo;

            if (request.Anexo != null && request.Anexo.Length > 0)
            {
                using var streamArquivo = request.Anexo.OpenReadStream();
                pagamento.CaminhoArquivoAnexo = _repositorio.CopiarAnexo(streamArquivo, request.Anexo.FileName);
            }

            pagamento.RazaoSocialPagador = request.RazaoSocialPagador.Trim();
            pagamento.CnpjPagador = CnpjHelper.SomenteNumeros(request.CnpjPagador);
            pagamento.Fornecedor = request.Fornecedor.Trim();
            pagamento.CnpjFornecedor = CnpjHelper.SomenteNumeros(request.CnpjFornecedor);
            pagamento.ValorTotal = valorTotal;
            pagamento.DataVencimento = DateTime.SpecifyKind(request.DataVencimento.Date, DateTimeKind.Utc);
            pagamento.Observacoes = request.Observacoes.Trim();

            _repositorio.Atualizar(pagamento);

            if (request.Anexo != null && request.Anexo.Length > 0 &&
                !string.IsNullOrWhiteSpace(caminhoAnexoAntigo) &&
                System.IO.File.Exists(caminhoAnexoAntigo))
            {
                try
                {
                    System.IO.File.Delete(caminhoAnexoAntigo);
                }
                catch
                {
                }
            }

            return Ok(new { mensagem = "Pagamento atualizado com sucesso!" });
        }

        [HttpDelete("{id}")]
        public IActionResult ExcluirPagamento(int id)
        {
            var pagamento = _repositorio.ObterPorId(id);

            if (pagamento == null)
                return NotFound();

            _repositorio.Excluir(id);

            return Ok(new { mensagem = "Pagamento excluído com sucesso!" });
        }

        [HttpGet("anexo/{nomeArquivo}")]
        public IActionResult ObterAnexo(string nomeArquivo)
        {
            string caminhoCompleto = _repositorio.ObterCaminhoCompletoAnexo(nomeArquivo);

            if (!System.IO.File.Exists(caminhoCompleto))
                return NotFound("Arquivo não encontrado.");

            return PhysicalFile(caminhoCompleto, "application/octet-stream", nomeArquivo);
        }

    }
}
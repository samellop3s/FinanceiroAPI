namespace FinanceiroApi.Models
{
    public class CriarUsuarioRequest
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Senha {  get; set; } = string.Empty;
        public string ChaveMestra { get; set;  } = string.Empty;
    }
}

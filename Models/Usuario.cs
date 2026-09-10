namespace FinanceiroApi.Models {
    public class Usiario
    {
        public int Id { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
    } 
}
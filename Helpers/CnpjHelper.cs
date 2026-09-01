using System.Text.RegularExpressions;


namespace FinanceiroApi.Helpers
{
    public static class CnpjHelper
    {
        // Vai remover tudo oq não for numero(ponto, barra, traço)
        public static string SomenteNumeros(string cnpj)
        {
            return Regex.Replace(cnpj ?? string.Empty, "[^0-9]", "");
        }

        //Valida o cnpj usando o algoritmo oficial dos digitos verificadores
        public static bool IsValid(string cnpj)
        {
            string numeros = SomenteNumeros(cnpj);

            if (numeros.Length != 14)
                return false;

            //Rejeição caso a sequencia for igual a 00000000000000
            if (numeros.Distinct().Count() == 1)
                return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = numeros.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCnpj += digito1;
            soma = 0;

            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return numeros.EndsWith(digito1.ToString() + digito2.ToString());

        }
    }
}

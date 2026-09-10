using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceiroApi.Data;
using FinanceiroApi.Models;

namespace FinanceiroApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FinanceiroDbContext _contexto;
        private readonly IConfiguration _configuracao;

        public AuthController(FinanceiroDbContext contexto, IConfiguration configuracao)
        {
            _contexto = contexto;
            _configuracao = configuracao;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var usuario = _contexto.Usuarios.FirstOrDefault(u => u.NomeUsuario == request.NomeUsuario);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
                return Unauthorized("Usuário ou senha inválidos.");

            var token = GerarToken(usuario);

            return Ok(new { token = token, nomeUsuario = usuario.NomeUsuario });
        }

        [HttpPost("criar-usuario")]
        public IActionResult CriarUsuario([FromBody] CriarUsuarioRequest request)
        {
            var chaveMestraEsperada = _configuracao["ChaveMestraCriacaoUsuario"];

            if (request.ChaveMestra != chaveMestraEsperada)
                return Unauthorized("Chave mestra inválida.");

            if (string.IsNullOrWhiteSpace(request.NomeUsuario) || string.IsNullOrWhiteSpace(request.Senha))
                return BadRequest("Nome de usuário e senha são obrigatórios.");

            var usuarioExistente = _contexto.Usuarios.FirstOrDefault(u => u.NomeUsuario == request.NomeUsuario);
            if (usuarioExistente != null)
                return BadRequest("Já existe um usuário com esse nome.");

            var novoUsuario = new Usuario
            {
                NomeUsuario = request.NomeUsuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha)
            };

            _contexto.Usuarios.Add(novoUsuario);
            _contexto.SaveChanges();

            return Ok(new { mensagem = "Usuário criado com sucesso!" });
        }

        private string GerarToken(Usuario usuario)
        {
            var chaveBytes = Encoding.UTF8.GetBytes(_configuracao["Jwt:ChaveSecreta"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.NomeUsuario),
                new Claim("id", usuario.Id.ToString())
            };

            var credenciais = new SigningCredentials(
                new SymmetricSecurityKey(chaveBytes),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuracao["Jwt:Emissor"],
                audience: _configuracao["Jwt:Audiencia"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
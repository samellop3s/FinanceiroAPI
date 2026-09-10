using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<FinanceiroApi.Data.RepositorioPagamentos>();
builder.Services.AddCors(options => 
{ /*
   autorização para a API aceitar os pedidos vindos da pagina em HTML.
   */
    options.AddPolicy("PermitirFrontend", policy =>
    {
        /*
         AllowAnyOrigin(): por enquanto, liberamos qualquer origem chamar a API — é aceitável em desenvolvimento/estudo, 
        mas não é recomendado em produção 
        (lá você restringiria pra um domínio específico, tipo só https://meusite.com). 
        Guarda essa observação pra quando for pra produção de verdade.
         */
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
builder.Services.AddDbContext<FinanceiroApi.Data.FinanceiroDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FinanceiroDb")));

var app = builder.Build();
app.UseCors("PermitirFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var chaveJwt = builder.Configuration["Jwt:JoseCarlosDeJesusLopesZeliGomesFerreiraLopesSamuelFerreiraLopesRafaelFerreiraLopes"];
var chaveBytes = Encoding.UTF8.GetBytes(chaveJwt!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emissor"],
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            IssuerSigningKey = new SymmetricSecurityKey(chaveBytes)
        };
    });

builder.Services.AddAuthorization();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
    
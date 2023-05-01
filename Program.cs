using System.Text;
using System.Text.Json.Serialization;
using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

var app = builder.Build();
LoadConfiguration(app);
app.UseAuthentication(); // Primeiro a autenticação - Quêm você é.
app.UseAuthorization(); // Depois a autorização - O que pode fazer.
app.UseStaticFiles();
app.MapControllers();
app.Run();

void ConfigureAuthentication(WebApplicationBuilder wab)
{
    var valueKey = wab.Configuration.GetValue<string>("JwtKey") ?? ""; // Chave de autenticação definida
    var key = Encoding.ASCII.GetBytes(valueKey);

    wab
        .Services
        .AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Validar chave de assinatura? Sim
                IssuerSigningKey = new SymmetricSecurityKey(key), // Como validar a chave? DA mesma forma que encriptei
                ValidateIssuer = false,
                ValidateAudience = false
            };  
        });
}

void ConfigureMvc(WebApplicationBuilder wab)
{
    wab.Services.AddMemoryCache();
    // Adiciona dados em cahce para economizar acessos ao banco

    wab
        .Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        })
        .AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        });
    // SuppressModelStateInvalidFilter - Suprime a validação e envio de mensagem pelo próprio ASP.NET,
    // pois nós estamos fazendo este tratamento.
    // AddJsonOptions ReferenceHandler.IgnoreCycles - Trata os casos de referência cíclica entre entidades evitando loop.
    // AddJsonOptions JsonIgnoreCondition.WhenWritingDefault - Não carrega os objetos quando forem null.
}

void ConfigureServices(WebApplicationBuilder wab)
{
    wab.Services.AddDbContext<BlogDataContext>();
    // Adiciona o serviço de DbContext na aplicação

    wab.Services.AddTransient<TokenService>();
    wab.Services.AddTransient<EmailService>();
    // Transient - O tempo de vida da entidade é o tempo da requsição.
    // Se na mesma reauisição for chamado mais de uma vez, ele será reaproveitado

    // builder.Services.AddScoped<>();
    // Scoped O tempo de vida da entidade é o mais curto. A cada chamada cria um novo

    // builder.Services.AddSingleton<>();
    // O tempo de vida da entidade dura toda a vida da aplicação
}

void LoadConfiguration(WebApplication wapp)
{
    Configuration.JwtKey = wapp.Configuration.GetValue<string>("JwtKey") ?? ""; 
    Configuration.ApiKeyName = wapp.Configuration.GetValue<string>("ApiKeyName") ?? ""; 
    Configuration.ApiKey = wapp.Configuration.GetValue<string>("ApiKey") ?? ""; 

    var smtp = new Configuration.SmtpConfiguration();
    wapp.Configuration.GetSection("Smtp").Bind(smtp);
    Configuration.Smtp = smtp;

    app.Configuration.GetConnectionString("Hostgator");
}
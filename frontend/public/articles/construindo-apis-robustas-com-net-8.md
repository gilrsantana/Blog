# Construindo APIs Robustas com .NET 8

O .NET 8 traz melhorias de desempenho fantásticas e novos recursos para desenvolvimento web. Neste artigo, exploramos as melhores práticas para desenhar APIs que escalam.

## Estrutura do Projeto

Para manter o código limpo, é importante separar as responsabilidades:

- **Controllers:** Tratam as requisições HTTP, validações básicas de entrada e retornam os códigos de status adequados.
- **Services (Business Logic):** Camada dedicada a implementar as regras de negócio da aplicação.
- **Repository (Data Access):** Gerencia a comunicação direta com o banco de dados usando o EF Core.

## Configuração Básica em C#

Aqui está um exemplo de como registramos os serviços de forma limpa no `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adicionar banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

## Dicas de Performance
- Use `AsNoTracking()` no EF Core para consultas de apenas leitura.
- Habilite compressão de resposta para reduzir o tráfego de rede.
- Adicione cache de forma estratégica para endpoints muito acessados.

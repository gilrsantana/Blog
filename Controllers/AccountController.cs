using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace Blog.Controllers;

[Authorize]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly TokenService _tokenService;
    public AccountController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    [HttpPost("v1/accounts")]
    public async Task<IActionResult> Post([FromBody]RegisterViewModel model, [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace('@', '-').Replace('.', '-')
        };
        var password = PasswordGenerator.Generate(25, true, true);
        user.PasswordHash = PasswordHasher.Hash(password); // Gera um hash diferente pra mesma senha a cada solicitação

        try
        {
            await context.AddAsync(user);
            await context.SaveChangesAsync();
            return Ok(new ResultViewModel<dynamic>(new
            {
                user = user.Email, password
            }));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>(
                "Este E-mail já está cadastrado"));
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>(
                "Erro interno no servidor ao processar a requisição"));
        }
        
    }
    
    [AllowAnonymous]
    [HttpPost("v1/accounts/login")]
    public async Task<IActionResult> Login([FromBody]LoginViewModel model, 
        [FromServices] TokenService tokenService, 
        [FromServices]BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == model.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido"));
        
        if (user.PasswordHash != null && !PasswordHasher.Verify(user.PasswordHash, model.Password) )
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválido"));

        try
        {
            var token = _tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>(
                "Erro interno no servidor ao processar a requisição"));
        }
    }

    // [Authorize(Roles = "user")]
    // [HttpGet("v1/user")]
    // public IActionResult GetUser() => Ok(User.Identity.Name);
    //
    // [Authorize(Roles = "author")]
    // [HttpGet("v1/author")]
    // public IActionResult GetAuthor() => Ok(User.Identity.Name);
    //
    // [Authorize(Roles = "admin")]
    // [HttpGet("v1/admin")]
    // public IActionResult GetAdmin() => Ok(User.Identity.Name);
    
}
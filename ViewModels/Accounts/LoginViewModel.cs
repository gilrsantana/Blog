using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Senha é obrigatório")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; }
}
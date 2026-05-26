namespace Blog.Shared;

public record ValidationError(string PropertyName, string ErrorMessage) 
    : Error("Validation.Error", ErrorMessage);

using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions;

public static class ModesStateExtension
{
    public static List<string> GetErrors(this ModelStateDictionary modelState)
    {
        return (from item in modelState.Values 
                    from error in item.Errors 
                        select error.ErrorMessage)
                            .ToList();
    }
    
}
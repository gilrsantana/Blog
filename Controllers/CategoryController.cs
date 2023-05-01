using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers;

[ApiController]
[Route("v1/categories/")]
public class CategoryController : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetAsync(
        [FromServices]BlogDataContext context,
        [FromServices]IMemoryCache cache)
    {
        try
        {
            var categories = cache.GetOrCreate("CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return GetCategories(context);
            });
            //var categories = await context.Categories.ToListAsync();
            return Ok(new ResultViewModel<List<Category>>(categories));
        }
        catch 
        {
            return StatusCode(500, new ResultViewModel<List<Category>>(
                "Erro interno no servidor ao processar a requisição."));
        }
    }

    private static List<Category> GetCategories(BlogDataContext context)
    {
        return context.Categories.ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromServices]BlogDataContext context, [FromRoute]int id)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category is null)
                return NotFound(new ResultViewModel<Category>("Categoria não encontrada."));
            return Ok(new ResultViewModel<Category>(category));
        }
        catch 
        {
            return StatusCode(500, new ResultViewModel<Category>(
                "Erro interno no servidor ao processar a requisição."));
        }
    }
    
    [HttpPost("")]
    public async Task<IActionResult> PostAsync([FromServices]BlogDataContext context, 
        [FromBody] EditorCategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
        try
        {
            var category = new Category
            {
                Name = model.Name,
                Slug = model.Slug.ToLower(),
            };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
            return Created($"{category.Id}", new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, new ResultViewModel<Category>(
                "Erro interno no servidor ao processar nova categoria."));
        }
        catch 
        {
            return StatusCode(500, new ResultViewModel<Category>(
                "Erro interno no servidor ao processar a requisição."));
        }
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PostAsync([FromServices]BlogDataContext context, 
                                                [FromRoute] int id, 
                                                [FromBody] EditorCategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category is null)
                return NotFound(new ResultViewModel<Category>("Categoria não encontrada."));

            category.Name = model.Name;
            category.Slug = model.Slug.ToLower();
        
            context.Categories.Update(category);
            await context.SaveChangesAsync();
            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, new ResultViewModel<Category>(
                "Não foi possível atualizar a categoria."));
        }
        catch (Exception e)
        {
            return StatusCode(500,new ResultViewModel<Category>(
                "Erro interno no servidor ao processar a requisição."));
        }
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromServices]BlogDataContext context, [FromRoute] int id)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category is null)
                return NotFound(new ResultViewModel<Category>("Categoria não encontrada."));
        
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return Ok(new ResultViewModel<Category>(category));
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500,  new ResultViewModel<Category>(
                "Não foi possível excluir a categoria."));
        }
        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<Category>(
                "Erro interno no servidor ao processar a requisição."));
        }
    }
}
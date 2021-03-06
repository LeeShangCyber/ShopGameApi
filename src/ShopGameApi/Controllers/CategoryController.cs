using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ShopGameApi.Data;
using Microsoft.Extensions.Configuration;
using ShopGameApi.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopGameApi.Objects;

namespace ShopGameApi.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {

        private readonly ShopGameApiDBContext _context;
        private readonly IConfiguration _config;

        public CategoryController(ShopGameApiDBContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        [HttpGet]
        public List<CategoryObjectJson> GetCategories() 
        {
            List<Category> caterogies = _context.Categories.Include(c => c.CategoryGame).ThenInclude(cg => cg.Game).ToList();
            List<CategoryObjectJson> categoryObjectJsons = new List<CategoryObjectJson>();
            foreach (Category category in caterogies)
            {
                categoryObjectJsons.Add(category.Convert());
            }
            return categoryObjectJsons;
        }

        [HttpPost("AddCategory")]
        public async Task<IActionResult> PostAddCategory(Category category)
        {
            Category result = _context.Categories.FirstOrDefault<Category>( s => s.Name == category.Name.ToLower() );
            if (result == null)
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return Ok(category);
            }
            return BadRequest(new { error = "Cannot Create Category" });
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> PostAddGameToCategory(int id, [FromBody]Game game)
        {
            _context.Categories.Include(c => c.CategoryGame).ThenInclude(cg => cg.Game);
            Category category = await _context.Categories.FindAsync(id);
            Game game48 = await _context.Games.FindAsync(game.GameId);
            
            if (category == null || game48 == null)
            {
                return BadRequest(new {error = "Category or Game is not Established!"} );
            }

            CategoryGame categoryGame = await _context.CategoryGame.FirstOrDefaultAsync<CategoryGame>(cg => (cg.GameId == game48.GameId && cg.GameId == category.CategoryId));

            if (categoryGame == null)
            {
                categoryGame = new CategoryGame
                {
                    GameId = game48.GameId,
                    CategoryId = category.CategoryId
                };

                await _context.CategoryGame.AddAsync(categoryGame);
                if (game48.CategoryGame == null) game48.CategoryGame = new List<CategoryGame>();
                if (category.CategoryGame == null) category.CategoryGame = new List<CategoryGame>();
                game48.CategoryGame.Add(categoryGame);
                category.CategoryGame.Add(categoryGame);
                await _context.SaveChangesAsync();
                return Ok(categoryGame);
            }

            return BadRequest(new { error = "Category had game" });
        }

    }
}
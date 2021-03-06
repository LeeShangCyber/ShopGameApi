using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ShopGameApi.Data;
using Microsoft.Extensions.Configuration;
using ShopGameApi.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShopGameApi.Objects;

namespace ShopGameApi.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly ShopGameApiDBContext _context;
        private readonly IConfiguration _config;

        public GameController(ShopGameApiDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public List<GameObjectJson> GetListGames()
        {          
            var games = _context.Games
            .Include(g => g.Company)
            .Include(g => g.Rating)
            .Include(g => g.Rating)
            .Include(g => g.CategoryGame)
            .ThenInclude(cg => cg.Category);

            List<GameObjectJson> gameObjectJsons = new List<GameObjectJson>();

            foreach (Game game in games)
            {
                gameObjectJsons.Add(game.Covert());
            }
            
            return gameObjectJsons;
        }

        [HttpPost("AddGame")]
        public async Task<IActionResult> PostAddGame(Game game)
        {

            Game result = _context.Games.FirstOrDefault<Game>(g => g.Name == game.Name.ToLower());

            if (result == null)
            {
                Company company = new Company();
                if (game.Company != null)
                    company = _context.Companies.FirstOrDefault<Company>(c => c.Name.ToLower() == game.Company.Name.ToLower());
                else
                {
                    return BadRequest(new { error = "Lack of Company of Game" });
                }

                if (company == null)
                {
                    await _context.Companies.AddAsync(game.Company);
                    await _context.SaveChangesAsync();
                    company = _context.Companies.FirstOrDefault<Company>(c => c.Name.ToLower() == game.Company.Name.ToLower());
                }       

            
                Rating rating = new Rating();

                rating.Quantity = 0;
                rating.Score = 0.0;

                game.Company = null;
                
                await _context.Ratings.AddAsync(rating);
                await _context.Games.AddAsync(game);
                game.Company = company;
                game.Rating = rating;
                await _context.SaveChangesAsync();

                return Ok(game);
            }

            return BadRequest(new { error = "Cannot create Game" });
        }

    }
}
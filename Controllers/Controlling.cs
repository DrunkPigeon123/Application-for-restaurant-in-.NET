using Microsoft.AspNetCore.Mvc;
using restaurant.Models;
using restaurant.Data; // Make sure this is the correct namespace for your DbContext
using restaurant.DTO;


namespace restaurant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllingController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ControllingController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet("recepie/{name}")]
        public IActionResult GetRecepie(string name)
        {
            var recipe = _context.MenuItems.FirstOrDefault(r => r.Name == name);
            if (recipe == null)
                return NotFound();

            var recipeDto = CreateRecipeDtoFactory.FromMenuItem(recipe);

            return Ok(recipeDto);
        }
    }

}
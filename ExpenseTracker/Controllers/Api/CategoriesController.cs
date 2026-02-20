using ExpenseTracker.Models;
using ExpenseTracker.Models.Dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/categories
        /// Returns all categories.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Title = c.Title,
                    Icon = c.Icon,
                    Type = c.Type,
                    IsRecurring = c.IsRecurring,
                    RecurringAmount = c.RecurringAmount
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// GET /api/categories/{id}
        /// Returns a single category.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(new CategoryDto
            {
                CategoryId = category.CategoryId,
                Title = category.Title,
                Icon = category.Icon,
                Type = category.Type,
                IsRecurring = category.IsRecurring,
                RecurringAmount = category.RecurringAmount
            });
        }

        /// <summary>
        /// POST /api/categories
        /// Create a new category.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = new Category
            {
                Title = dto.Title,
                Icon = dto.Icon,
                Type = dto.Type,
                IsRecurring = dto.IsRecurring,
                RecurringAmount = dto.RecurringAmount
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            dto.CategoryId = category.CategoryId;
            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, dto);
        }

        /// <summary>
        /// PUT /api/categories/{id}
        /// Update an existing category.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            category.Title = dto.Title;
            category.Icon = dto.Icon;
            category.Type = dto.Type;
            category.IsRecurring = dto.IsRecurring;
            category.RecurringAmount = dto.RecurringAmount;

            _context.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new CategoryDto
            {
                CategoryId = category.CategoryId,
                Title = category.Title,
                Icon = category.Icon,
                Type = category.Type,
                IsRecurring = category.IsRecurring,
                RecurringAmount = category.RecurringAmount
            });
        }

        /// <summary>
        /// DELETE /api/categories/{id}
        /// Delete a category.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

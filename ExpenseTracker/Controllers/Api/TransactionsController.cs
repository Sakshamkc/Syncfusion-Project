using ExpenseTracker.Helpers;
using ExpenseTracker.Models;
using ExpenseTracker.Models.Dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/transactions
        /// Returns all transactions (newest first).
        /// Optional query params: ?days=7 (filter last N days), ?categoryId=1
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TransactionResponseDto>>> GetAll(
            [FromQuery] int? days,
            [FromQuery] int? categoryId)
        {
            IQueryable<Transaction> query = _context.Transactions.Include(t => t.Category);

            if (days.HasValue && days.Value > 0)
            {
                var startDate = DateTime.Today.AddDays(-(days.Value - 1));
                query = query.Where(t => t.Date >= startDate && t.Date <= DateTime.Today);
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return Ok(transactions.Select(MapToDto).ToList());
        }

        /// <summary>
        /// GET /api/transactions/{id}
        /// Returns a single transaction.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> GetById(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null)
                return NotFound(new { message = "Transaction not found." });

            return Ok(MapToDto(transaction));
        }

        /// <summary>
        /// POST /api/transactions
        /// Create a new transaction.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TransactionResponseDto>> Create([FromBody] TransactionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify category exists
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Invalid CategoryId." });

            var transaction = new Transaction
            {
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                Note = dto.Note,
                Date = dto.Date ?? DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Reload with category
            var saved = await _context.Transactions
                .Include(t => t.Category)
                .FirstAsync(t => t.TransactionId == transaction.TransactionId);

            return CreatedAtAction(nameof(GetById),
                new { id = saved.TransactionId },
                MapToDto(saved));
        }

        /// <summary>
        /// PUT /api/transactions/{id}
        /// Update an existing transaction.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> Update(int id, [FromBody] TransactionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound(new { message = "Transaction not found." });

            // Verify category exists
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Invalid CategoryId." });

            transaction.CategoryId = dto.CategoryId;
            transaction.Amount = dto.Amount;
            transaction.Note = dto.Note;
            transaction.Date = dto.Date ?? transaction.Date;

            _context.Update(transaction);
            await _context.SaveChangesAsync();

            // Reload with category
            var saved = await _context.Transactions
                .Include(t => t.Category)
                .FirstAsync(t => t.TransactionId == id);

            return Ok(MapToDto(saved));
        }

        /// <summary>
        /// DELETE /api/transactions/{id}
        /// Delete a transaction.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound(new { message = "Transaction not found." });

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static TransactionResponseDto MapToDto(Transaction t)
        {
            var culture = CultureInfo.CreateSpecificCulture("ne-NP");
            culture.NumberFormat.CurrencySymbol = "Rs";
            culture.NumberFormat.CurrencyPositivePattern = 2;

            var sign = (t.Category == null || t.Category.Type == "Expense") ? "- " : "+ ";

            string nepaliDate, nepaliDateShort;
            try
            {
                nepaliDate = NepaliDateHelper.FormatBsDate(t.Date);
                nepaliDateShort = NepaliDateHelper.FormatBsDateShort(t.Date);
            }
            catch
            {
                nepaliDate = t.Date.ToString("yyyy-MM-dd");
                nepaliDateShort = t.Date.ToString("MMM-dd-yy");
            }

            return new TransactionResponseDto
            {
                TransactionId = t.TransactionId,
                CategoryId = t.CategoryId,
                CategoryTitle = t.Category?.Title ?? "",
                CategoryIcon = t.Category?.Icon ?? "",
                CategoryType = t.Category?.Type ?? "",
                Amount = t.Amount,
                Note = t.Note,
                Date = t.Date,
                NepaliDate = nepaliDate,
                NepaliDateShort = nepaliDateShort,
                FormattedAmount = sign + t.Amount.ToString("C0", culture)
            };
        }
    }
}

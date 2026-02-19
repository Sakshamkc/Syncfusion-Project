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
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/dashboard/summary?days=7
        /// Returns income, expense, balance, chart data, and recent transactions.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary([FromQuery] int days = 7)
        {
            var culture = CultureInfo.CreateSpecificCulture("ne-NP");
            culture.NumberFormat.CurrencySymbol = "Rs";
            culture.NumberFormat.CurrencyPositivePattern = 2;
            culture.NumberFormat.CurrencyNegativePattern = 8;

            var startDate = DateTime.Today.AddDays(-(days - 1));
            var endDate = DateTime.Today;

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            int totalIncome = transactions
                .Where(t => t.Category?.Type == "Income")
                .Sum(t => t.Amount);

            int totalExpense = transactions
                .Where(t => t.Category?.Type == "Expense")
                .Sum(t => t.Amount);

            int balance = totalIncome - totalExpense;

            // Doughnut chart — expense by category
            var expenseByCategory = transactions
                .Where(t => t.Category?.Type == "Expense")
                .GroupBy(t => t.Category!.CategoryId)
                .Select(g => new DoughnutChartItem
                {
                    CategoryTitleWithIcon = g.First().Category!.Icon + " " + g.First().Category!.Title,
                    Amount = g.Sum(t => t.Amount),
                    FormattedAmount = g.Sum(t => t.Amount).ToString("C0", culture)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Spline chart — income vs expense per day
            var incomeSummary = transactions
                .Where(t => t.Category?.Type == "Income")
                .GroupBy(t => t.Date.Date)
                .ToDictionary(
                    g => NepaliDateHelper.FormatBsDateShort(g.Key),
                    g => g.Sum(t => t.Amount));

            var expenseSummary = transactions
                .Where(t => t.Category?.Type == "Expense")
                .GroupBy(t => t.Date.Date)
                .ToDictionary(
                    g => NepaliDateHelper.FormatBsDateShort(g.Key),
                    g => g.Sum(t => t.Amount));

            var dayLabels = Enumerable.Range(0, days)
                .Select(i => NepaliDateHelper.FormatBsDateShort(startDate.AddDays(i)))
                .ToList();

            var splineData = dayLabels.Select(day => new SplineChartItem
            {
                Day = day,
                Income = incomeSummary.GetValueOrDefault(day, 0),
                Expense = expenseSummary.GetValueOrDefault(day, 0)
            }).ToList();

            // Recent transactions (last 5)
            var recentTransactions = await _context.Transactions
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            var recentDtos = recentTransactions.Select(t =>
            {
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
            }).ToList();

            return Ok(new DashboardSummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = balance,
                TotalIncomeFormatted = totalIncome.ToString("N0", culture),
                TotalExpenseFormatted = totalExpense.ToString("N0", culture),
                BalanceFormatted = balance.ToString("N0", culture),
                ExpenseByCategory = expenseByCategory,
                IncomeVsExpense = splineData,
                RecentTransactions = recentDtos
            });
        }

        /// <summary>
        /// GET /api/dashboard/calendar?bsYear=2081&bsMonth=10
        /// Returns Nepali calendar data for a specific BS month.
        /// </summary>
        [HttpGet("calendar")]
        public async Task<ActionResult<CalendarDataDto>> GetCalendarData(
            [FromQuery] int? bsYear,
            [FromQuery] int? bsMonth)
        {
            try
            {
                // Default to current BS month
                if (!bsYear.HasValue || !bsMonth.HasValue)
                {
                    var todayBs = NepaliDateHelper.AdToBs(DateTime.Today);
                    bsYear = todayBs.Year;
                    bsMonth = todayBs.Month;
                }

                int daysInMonth = NepaliDateHelper.GetBsMonthDays(bsYear.Value, bsMonth.Value);
                var monthStartAd = NepaliDateHelper.BsToAd(bsYear.Value, bsMonth.Value, 1);
                var monthEndAd = NepaliDateHelper.BsToAd(bsYear.Value, bsMonth.Value, daysInMonth);
                int firstDow = (int)monthStartAd.DayOfWeek;

                var txns = await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.Date >= monthStartAd && t.Date <= monthEndAd)
                    .ToListAsync();

                var days = new Dictionary<int, CalendarDayData>();
                foreach (var t in txns)
                {
                    var bs = NepaliDateHelper.AdToBs(t.Date);
                    if (bs.Year == bsYear.Value && bs.Month == bsMonth.Value)
                    {
                        if (!days.ContainsKey(bs.Day))
                            days[bs.Day] = new CalendarDayData();

                        if (t.Category?.Type == "Income")
                            days[bs.Day].Income += t.Amount;
                        else
                            days[bs.Day].Expense += t.Amount;
                    }
                }

                return Ok(new CalendarDataDto
                {
                    BsYear = bsYear.Value,
                    BsMonth = bsMonth.Value,
                    BsMonthName = NepaliDateHelper.GetMonthName(bsMonth.Value),
                    DaysInMonth = daysInMonth,
                    FirstDayOfWeek = firstDow,
                    Days = days
                });
            }
            catch
            {
                return BadRequest(new { message = "Invalid BS year/month." });
            }
        }
    }
}

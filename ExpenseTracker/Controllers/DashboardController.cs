using ExpenseTracker.Models;
using ExpenseTracker.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    [Authorize]

    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var culture = CultureInfo.CreateSpecificCulture("ne-NP");
            culture.NumberFormat.CurrencySymbol = "Rs";
            culture.NumberFormat.CurrencyPositivePattern = 2;
            culture.NumberFormat.CurrencyNegativePattern = 8;

            // Current BS month date range
            var todayBs = NepaliDateHelper.AdToBs(DateTime.Today);
            int bsDaysInMonth = NepaliDateHelper.GetBsMonthDays(todayBs.Year, todayBs.Month);
            DateTime monthStartAd = NepaliDateHelper.BsToAd(todayBs.Year, todayBs.Month, 1);
            DateTime monthEndAd = NepaliDateHelper.BsToAd(todayBs.Year, todayBs.Month, bsDaysInMonth);

            // Auto-generate recurring transactions for this month if not already present
            await GenerateRecurringTransactions(todayBs.Year, todayBs.Month, monthStartAd, monthEndAd);

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= monthStartAd && y.Date <= monthEndAd)
                .ToListAsync();

            int TotalIncome = SelectedTransactions.Where(i => i.Category.Type == "Income").Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("N0", culture);

            int TotalExpense = SelectedTransactions.Where(i => i.Category.Type == "Expense").Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("N0", culture);

            int Balance = TotalIncome - TotalExpense;

            ViewBag.Balance = Balance.ToString("N0", culture);
            //Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0", culture),
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            // Spline chart: show daily data for the current BS month
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = NepaliDateHelper.FormatBsDateShort(k.First().Date),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            List<SplineChartData> ExpenseSummary = SelectedTransactions
               .Where(i => i.Category.Type == "Expense")
               .GroupBy(j => j.Date)
               .Select(k => new SplineChartData()
               {
                   day = NepaliDateHelper.FormatBsDateShort(k.First().Date),
                   expense = k.Sum(l => l.Amount)
               })
               .ToList();

            // Use last 7 days for the spline chart x-axis
            DateTime StartDate = DateTime.Today.AddDays(-6);
            string[] last7Days = Enumerable.Range(0, 7)
                .Select(i => NepaliDateHelper.FormatBsDateShort(StartDate.AddDays(i))).ToArray();

            ViewBag.SplineChartData = from day in last7Days
                                      join income in IncomeSummary on day equals income.day
                                      into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day
                                      into ExpenseJoined
                                      from expense in ExpenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            // Nepali Calendar Data — reuse already-computed BS month info
            ViewBag.BsYear = todayBs.Year;
            ViewBag.BsMonth = todayBs.Month;
            ViewBag.BsDay = todayBs.Day;
            ViewBag.BsMonthName = NepaliDateHelper.GetMonthName(todayBs.Month);
            ViewBag.BsDaysInMonth = bsDaysInMonth;

            // Build a dictionary: BS day number → { income, expense }
            var calendarData = new Dictionary<int, int[]>(); // [income, expense]
            foreach (var t in SelectedTransactions)
            {
                var bs = NepaliDateHelper.AdToBs(t.Date);
                if (bs.Year == todayBs.Year && bs.Month == todayBs.Month)
                {
                    if (!calendarData.ContainsKey(bs.Day))
                        calendarData[bs.Day] = new int[] { 0, 0 };

                    if (t.Category?.Type == "Income")
                        calendarData[bs.Day][0] += t.Amount;
                    else
                        calendarData[bs.Day][1] += t.Amount;
                }
            }
            ViewBag.CalendarData = calendarData;

            // Day of week for the 1st of BS month (0=Sun)
            ViewBag.BsFirstDayOfWeek = (int)monthStartAd.DayOfWeek;

            // Today's Events & Reminders
            var todayEvents = await _context.CalendarEvents
                .Where(e => e.Date == DateTime.Today)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
            ViewBag.TodayEvents = todayEvents;

            return View();
        }

        /// <summary>
        /// AJAX endpoint: returns calendar data for a given BS year/month.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CalendarData(int bsYear, int bsMonth)
        {
            try
            {
                int daysInMonth = NepaliDateHelper.GetBsMonthDays(bsYear, bsMonth);
                var monthStartAd = NepaliDateHelper.BsToAd(bsYear, bsMonth, 1);
                var monthEndAd = NepaliDateHelper.BsToAd(bsYear, bsMonth, daysInMonth);
                int firstDow = (int)monthStartAd.DayOfWeek;

                var txns = await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.Date >= monthStartAd && t.Date <= monthEndAd)
                    .ToListAsync();

                // Also fetch calendar events for this month
                var calEvents = await _context.CalendarEvents
                    .Where(e => e.Date >= monthStartAd && e.Date <= monthEndAd)
                    .OrderBy(e => e.StartTime)
                    .ToListAsync();

                var result = new Dictionary<string, int[]>();
                foreach (var t in txns)
                {
                    var bs = NepaliDateHelper.AdToBs(t.Date);
                    if (bs.Year == bsYear && bs.Month == bsMonth)
                    {
                        var key = bs.Day.ToString();
                        if (!result.ContainsKey(key))
                            result[key] = new int[] { 0, 0 };

                        if (t.Category?.Type == "Income")
                            result[key][0] += t.Amount;
                        else
                            result[key][1] += t.Amount;
                    }
                }

                // Convert to anonymous objects for JSON
                var txnJson = new Dictionary<string, object>();
                foreach (var kv in result)
                    txnJson[kv.Key] = new { income = kv.Value[0], expense = kv.Value[1] };

                // Group events by BS day
                var evtJson = new Dictionary<string, object>();
                foreach (var ev in calEvents)
                {
                    var bs = NepaliDateHelper.AdToBs(ev.Date);
                    if (bs.Year == bsYear && bs.Month == bsMonth)
                    {
                        var key = bs.Day.ToString();
                        if (!evtJson.ContainsKey(key))
                            evtJson[key] = new List<object>();
                        ((List<object>)evtJson[key]).Add(new
                        {
                            ev.Title,
                            ev.Color,
                            ev.Type
                        });
                    }
                }

                return Json(new { daysInMonth, firstDow, transactions = txnJson, events = evtJson });
            }
            catch
            {
                return Json(new { daysInMonth = 30, firstDow = 0, transactions = new { } });
            }
        }

        /// <summary>
        /// Automatically copies transactions from recurring categories  
        /// from the previous BS month into the current month if not already present.
        /// </summary>
        private async Task GenerateRecurringTransactions(int bsYear, int bsMonth, DateTime monthStartAd, DateTime monthEndAd)
        {
            // Check if we already have transactions from recurring categories this month
            bool alreadyGenerated = await _context.Transactions
                .Include(t => t.Category)
                .AnyAsync(t => t.Category.IsRecurring && t.Date >= monthStartAd && t.Date <= monthEndAd);

            if (alreadyGenerated) return;

            // Get previous BS month
            int prevMonth = bsMonth - 1;
            int prevYear = bsYear;
            if (prevMonth < 1)
            {
                prevMonth = 12;
                prevYear--;
            }

            int prevDaysInMonth = NepaliDateHelper.GetBsMonthDays(prevYear, prevMonth);
            var prevStartAd = NepaliDateHelper.BsToAd(prevYear, prevMonth, 1);
            var prevEndAd = NepaliDateHelper.BsToAd(prevYear, prevMonth, prevDaysInMonth);

            // Get all transactions from recurring categories in the previous month
            var recurringTxns = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Category.IsRecurring && t.Date >= prevStartAd && t.Date <= prevEndAd)
                .ToListAsync();

            if (!recurringTxns.Any()) return;

            // Copy each to the 1st of the current BS month
            foreach (var txn in recurringTxns)
            {
                var newTxn = new Transaction
                {
                    CategoryId = txn.CategoryId,
                    Amount = txn.Amount,
                    Note = txn.Note,
                    Date = monthStartAd // 1st of the new BS month
                };
                _context.Transactions.Add(newTxn);
            }

            await _context.SaveChangesAsync();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}

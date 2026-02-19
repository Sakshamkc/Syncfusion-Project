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
        //Last 7 Days
        DateTime StartDate = DateTime.Today.AddDays(-6);
        DateTime EndDate = DateTime.Today;


        public async Task<IActionResult> Index()
        {
            var culture = CultureInfo.CreateSpecificCulture("ne-NP");
            culture.NumberFormat.CurrencySymbol = "Rs";
            culture.NumberFormat.CurrencyPositivePattern = 2;
            culture.NumberFormat.CurrencyNegativePattern = 8;
            List<Transaction> SelectedTransactions = await _context.Transactions.Include(x => x.Category).Where(y => y.Date >= StartDate && y.Date <= EndDate).ToListAsync();
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

            // Nepali Calendar Data — current BS month
            var todayBs = NepaliDateHelper.AdToBs(DateTime.Today);
            ViewBag.BsYear = todayBs.Year;
            ViewBag.BsMonth = todayBs.Month;
            ViewBag.BsDay = todayBs.Day;
            ViewBag.BsMonthName = NepaliDateHelper.GetMonthName(todayBs.Month);
            ViewBag.BsDaysInMonth = NepaliDateHelper.GetBsMonthDays(todayBs.Year, todayBs.Month);

            // Get AD range for this BS month (1st to last day)
            int bsDaysInMonth = NepaliDateHelper.GetBsMonthDays(todayBs.Year, todayBs.Month);
            ViewBag.BsDaysInMonth = bsDaysInMonth;
            var bsMonthStartAd = NepaliDateHelper.BsToAd(todayBs.Year, todayBs.Month, 1);
            var bsMonthEndAd = NepaliDateHelper.BsToAd(todayBs.Year, todayBs.Month, bsDaysInMonth);

            // Expense totals per day in this BS month
            var monthTransactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date >= bsMonthStartAd && t.Date <= bsMonthEndAd)
                .ToListAsync();

            // Build a dictionary: BS day number → { income, expense }
            var calendarData = new Dictionary<int, int[]>(); // [income, expense]
            foreach (var t in monthTransactions)
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
            ViewBag.BsFirstDayOfWeek = (int)bsMonthStartAd.DayOfWeek;

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

                return Json(new { daysInMonth, firstDow, transactions = txnJson });
            }
            catch
            {
                return Json(new { daysInMonth = 30, firstDow = 0, transactions = new { } });
            }
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}

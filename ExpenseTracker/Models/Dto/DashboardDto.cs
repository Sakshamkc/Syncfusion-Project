using System;
using System.Collections.Generic;

namespace ExpenseTracker.Models.Dto
{
    public class DashboardSummaryDto
    {
        public int TotalIncome { get; set; }
        public int TotalExpense { get; set; }
        public int Balance { get; set; }
        public string TotalIncomeFormatted { get; set; } = "";
        public string TotalExpenseFormatted { get; set; } = "";
        public string BalanceFormatted { get; set; } = "";
        public List<DoughnutChartItem> ExpenseByCategory { get; set; } = new();
        public List<SplineChartItem> IncomeVsExpense { get; set; } = new();
        public List<TransactionResponseDto> RecentTransactions { get; set; } = new();
    }

    public class DoughnutChartItem
    {
        public string CategoryTitleWithIcon { get; set; } = "";
        public int Amount { get; set; }
        public string FormattedAmount { get; set; } = "";
    }

    public class SplineChartItem
    {
        public string Day { get; set; } = "";
        public int Income { get; set; }
        public int Expense { get; set; }
    }

    public class CalendarDataDto
    {
        public int BsYear { get; set; }
        public int BsMonth { get; set; }
        public string BsMonthName { get; set; } = "";
        public int DaysInMonth { get; set; }
        public int FirstDayOfWeek { get; set; }
        public Dictionary<int, CalendarDayData> Days { get; set; } = new();
    }

    public class CalendarDayData
    {
        public int Income { get; set; }
        public int Expense { get; set; }
    }
}

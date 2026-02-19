using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Helpers;

namespace ExpenseTracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public Category? Category { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        public int Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Category == null ? "" : Category.Icon + "" + Category.Title;
            }

        }

        [NotMapped]
        public string? FormattedAccount
        {
            get
            {
                var culture = CultureInfo.CreateSpecificCulture("ne-NP");
                culture.NumberFormat.CurrencySymbol = "Rs";
                culture.NumberFormat.CurrencyPositivePattern = 2;
                var sign = (Category == null || Category.Type == "Expense") ? "- " : "+ ";

                return sign + Amount.ToString("C0", culture);
            }

        }

        /// <summary>
        /// Nepali (Bikram Sambat) date string, e.g. "2081-10-06"
        /// </summary>
        [NotMapped]
        public string? NepaliDate
        {
            get
            {
                try { return NepaliDateHelper.FormatBsDate(Date); }
                catch { return Date.ToString("yyyy-MM-dd"); }
            }
        }

        /// <summary>
        /// Short Nepali date, e.g. "Mag-06-81"
        /// </summary>
        [NotMapped]
        public string? NepaliDateShort
        {
            get
            {
                try { return NepaliDateHelper.FormatBsDateShort(Date); }
                catch { return Date.ToString("MMM-dd-yy"); }
            }
        }
    }
}

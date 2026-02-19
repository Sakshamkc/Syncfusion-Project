using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dto
{
    /// <summary>
    /// Used for creating/updating a transaction.
    /// </summary>
    public class TransactionCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        public int Amount { get; set; }

        [MaxLength(75)]
        public string? Note { get; set; }

        /// <summary>
        /// AD date (ISO 8601). If omitted, defaults to today.
        /// </summary>
        public DateTime? Date { get; set; }
    }

    /// <summary>
    /// Returned when reading a transaction.
    /// </summary>
    public class TransactionResponseDto
    {
        public int TransactionId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; } = "";
        public string CategoryIcon { get; set; } = "";
        public string CategoryType { get; set; } = "";
        public int Amount { get; set; }
        public string? Note { get; set; }
        public DateTime Date { get; set; }
        public string NepaliDate { get; set; } = "";
        public string NepaliDateShort { get; set; } = "";
        public string FormattedAmount { get; set; } = "";
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "nvarchar(10)")]
        public string Type { get; set; } = "Expense";

        /// <summary>
        /// If true, transactions under this category auto-copy to the next month
        /// (e.g. SIP, Salary, Pocket Money, Rent).
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// Fixed monthly amount for recurring categories (e.g. 5000 for SIP).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RecurringAmount { get; set; }

        [NotMapped]
        public string? TitleWithIcon
        {
            get
            {
                return this.Icon + " " + this.Title;
            }
        }
    }
}

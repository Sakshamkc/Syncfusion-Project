using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models.Dto
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = "";

        [MaxLength(100)]
        public string Icon { get; set; } = "";

        [Required]
        [RegularExpression("^(Income|Expense)$", ErrorMessage = "Type must be 'Income' or 'Expense'.")]
        public string Type { get; set; } = "Expense";
    }
}

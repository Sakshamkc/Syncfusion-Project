using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Helpers;

namespace ExpenseTracker.Models
{
    public class CalendarEvent
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// "Reminder" or "Event"
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "Reminder";

        /// <summary>
        /// Date in AD (stored in DB)
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; } = DateTime.Today;

        /// <summary>
        /// Optional start time (for Events)
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Optional end time (for Events)
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Color label: "green", "blue", "red", "orange", "purple"
        /// </summary>
        [StringLength(20)]
        public string Color { get; set; } = "blue";

        /// <summary>
        /// Nepali date (computed, not stored)
        /// </summary>
        [NotMapped]
        public string NepaliDate => NepaliDateHelper.FormatBsDate(Date);
    }
}

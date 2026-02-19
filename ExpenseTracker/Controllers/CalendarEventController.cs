using ExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class CalendarEventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarEventController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all events for a date range (AD dates as query params).
        /// Used by calendar AJAX for showing event dots.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEvents(string startDate, string endDate)
        {
            if (!DateTime.TryParse(startDate, out var start) || !DateTime.TryParse(endDate, out var end))
                return BadRequest("Invalid dates");

            var events = await _context.CalendarEvents
                .Where(e => e.Date >= start && e.Date <= end)
                .OrderBy(e => e.Date).ThenBy(e => e.StartTime)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Description,
                    e.Type,
                    date = e.Date.ToString("yyyy-MM-dd"),
                    startTime = e.StartTime.HasValue ? e.StartTime.Value.ToString(@"hh\:mm") : null,
                    endTime = e.EndTime.HasValue ? e.EndTime.Value.ToString(@"hh\:mm") : null,
                    e.Color
                })
                .ToListAsync();

            return Json(events);
        }

        /// <summary>
        /// Get events for a specific day (AD date).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDayEvents(string date)
        {
            if (!DateTime.TryParse(date, out var d))
                return BadRequest("Invalid date");

            var events = await _context.CalendarEvents
                .Where(e => e.Date == d)
                .OrderBy(e => e.StartTime)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Description,
                    e.Type,
                    date = e.Date.ToString("yyyy-MM-dd"),
                    startTime = e.StartTime.HasValue ? e.StartTime.Value.ToString(@"hh\:mm") : null,
                    endTime = e.EndTime.HasValue ? e.EndTime.Value.ToString(@"hh\:mm") : null,
                    e.Color
                })
                .ToListAsync();

            return Json(events);
        }

        /// <summary>
        /// Create a new event/reminder (AJAX POST).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CalendarEventDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { error = "Title is required" });

            if (!DateTime.TryParse(dto.Date, out var date))
                return BadRequest(new { error = "Invalid date" });

            var ev = new CalendarEvent
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Type = dto.Type == "Event" ? "Event" : "Reminder",
                Date = date,
                Color = dto.Color ?? "blue"
            };

            if (!string.IsNullOrEmpty(dto.StartTime) && TimeSpan.TryParse(dto.StartTime, out var st))
                ev.StartTime = st;
            if (!string.IsNullOrEmpty(dto.EndTime) && TimeSpan.TryParse(dto.EndTime, out var et))
                ev.EndTime = et;

            _context.CalendarEvents.Add(ev);
            await _context.SaveChangesAsync();

            return Json(new
            {
                ev.EventId,
                ev.Title,
                ev.Description,
                ev.Type,
                date = ev.Date.ToString("yyyy-MM-dd"),
                startTime = ev.StartTime?.ToString(@"hh\:mm"),
                endTime = ev.EndTime?.ToString(@"hh\:mm"),
                ev.Color
            });
        }

        /// <summary>
        /// Delete an event/reminder (AJAX POST).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.CalendarEvents.FindAsync(id);
            if (ev == null) return NotFound();
            _context.CalendarEvents.Remove(ev);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class CalendarEventDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Type { get; set; } = "Reminder";
        public string Date { get; set; } = "";
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Color { get; set; }
    }
}

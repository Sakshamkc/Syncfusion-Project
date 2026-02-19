using System;
using System.Collections.Generic;

namespace ExpenseTracker.Helpers
{
    /// <summary>
    /// Converts between Gregorian (AD) and Bikram Sambat (BS) dates.
    /// Covers BS years 2000–2090 (approx. 1943-04-14 to 2033-04-13 AD).
    /// </summary>
    public static class NepaliDateHelper
    {
        // Month-day lookup: each entry is an array of 12 ints (days per month for that BS year)
        private static readonly Dictionary<int, int[]> BsMonthDays = new()
        {
            { 2000, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2001, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2002, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2003, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2004, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2005, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2006, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2007, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2008, new[] { 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31 } },
            { 2009, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2010, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2011, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2012, new[] { 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30 } },
            { 2013, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2014, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2015, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2016, new[] { 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30 } },
            { 2017, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2018, new[] { 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2019, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2020, new[] { 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2021, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2022, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30 } },
            { 2023, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2024, new[] { 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2025, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2026, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2027, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2028, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2029, new[] { 31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30 } },
            { 2030, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2031, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2032, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2033, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2034, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2035, new[] { 30, 32, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31 } },
            { 2036, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2037, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2038, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2039, new[] { 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30 } },
            { 2040, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2041, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2042, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2043, new[] { 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30 } },
            { 2044, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2045, new[] { 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2046, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2047, new[] { 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2048, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2049, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30 } },
            { 2050, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2051, new[] { 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2052, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2053, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30 } },
            { 2054, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2055, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2056, new[] { 31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30 } },
            { 2057, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2058, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2059, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2060, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2061, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2062, new[] { 30, 32, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30 } },
            { 2063, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2064, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2065, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2066, new[] { 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30 } },
            { 2067, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2068, new[] { 31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2069, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2070, new[] { 31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30 } },
            { 2071, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2072, new[] { 31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30 } },
            { 2073, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31 } },
            { 2074, new[] { 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2075, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2076, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30 } },
            { 2077, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31 } },
            { 2078, new[] { 31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2079, new[] { 31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30 } },
            { 2080, new[] { 31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30 } },
            { 2081, new[] { 31, 31, 32, 32, 31, 30, 30, 30, 29, 30, 30, 30 } },
            { 2082, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30 } },
            { 2083, new[] { 31, 31, 32, 31, 31, 30, 30, 30, 29, 30, 30, 30 } },
            { 2084, new[] { 31, 31, 32, 31, 31, 30, 30, 30, 29, 30, 30, 30 } },
            { 2085, new[] { 31, 32, 31, 32, 30, 31, 30, 30, 29, 30, 30, 30 } },
            { 2086, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30 } },
            { 2087, new[] { 31, 31, 32, 31, 31, 31, 30, 30, 29, 30, 30, 30 } },
            { 2088, new[] { 30, 31, 32, 32, 30, 31, 30, 30, 29, 30, 30, 30 } },
            { 2089, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30 } },
            { 2090, new[] { 30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30 } },
        };

        // Nepali month names in English
        private static readonly string[] NepaliMonths =
        {
            "Baisakh", "Jestha", "Ashadh", "Shrawan", "Bhadra", "Ashwin",
            "Kartik", "Mangsir", "Poush", "Magh", "Falgun", "Chaitra"
        };

        // Nepali month names in Devanagari
        private static readonly string[] NepaliMonthsNp =
        {
            "बैशाख", "जेठ", "असार", "श्रावण", "भदौ", "आश्विन",
            "कार्तिक", "मंसिर", "पौष", "माघ", "फाल्गुन", "चैत्र"
        };

        // Reference point: BS 2000/01/01 = AD 1943/04/14
        private static readonly DateTime ReferenceAdDate = new(1943, 4, 14);
        private const int ReferenceBsYear = 2000;
        private const int ReferenceBsMonth = 1;
        private const int ReferenceBsDay = 1;

        /// <summary>
        /// Get total days in a BS year.
        /// </summary>
        public static int GetBsYearDays(int bsYear)
        {
            if (!BsMonthDays.ContainsKey(bsYear))
                throw new ArgumentOutOfRangeException(nameof(bsYear), $"BS year {bsYear} is not supported (2000-2090).");

            int total = 0;
            foreach (var d in BsMonthDays[bsYear]) total += d;
            return total;
        }

        /// <summary>
        /// Get total days in a BS month.
        /// </summary>
        public static int GetBsMonthDays(int bsYear, int bsMonth)
        {
            if (!BsMonthDays.ContainsKey(bsYear))
                throw new ArgumentOutOfRangeException(nameof(bsYear), $"BS year {bsYear} is not supported.");
            if (bsMonth < 1 || bsMonth > 12)
                throw new ArgumentOutOfRangeException(nameof(bsMonth), "Month must be 1-12.");

            return BsMonthDays[bsYear][bsMonth - 1];
        }

        /// <summary>
        /// Convert AD (Gregorian) DateTime to BS date tuple (year, month, day).
        /// </summary>
        public static (int Year, int Month, int Day) AdToBs(DateTime adDate)
        {
            // Calculate total days from reference AD date
            int totalDays = (int)(adDate.Date - ReferenceAdDate).TotalDays;

            if (totalDays < 0)
                throw new ArgumentOutOfRangeException(nameof(adDate), "Date is before supported range.");

            int bsYear = ReferenceBsYear;
            int bsMonth = ReferenceBsMonth;
            int bsDay = ReferenceBsDay;

            // Move forward through days
            while (totalDays > 0)
            {
                int daysInMonth = GetBsMonthDays(bsYear, bsMonth);
                int daysLeftInMonth = daysInMonth - bsDay;

                if (totalDays <= daysLeftInMonth)
                {
                    bsDay += totalDays;
                    totalDays = 0;
                }
                else
                {
                    totalDays -= (daysLeftInMonth + 1);
                    bsMonth++;
                    if (bsMonth > 12)
                    {
                        bsMonth = 1;
                        bsYear++;
                    }
                    bsDay = 1;
                }
            }

            return (bsYear, bsMonth, bsDay);
        }

        /// <summary>
        /// Convert BS date to AD (Gregorian) DateTime.
        /// </summary>
        public static DateTime BsToAd(int bsYear, int bsMonth, int bsDay)
        {
            if (!BsMonthDays.ContainsKey(bsYear))
                throw new ArgumentOutOfRangeException(nameof(bsYear), $"BS year {bsYear} is not supported.");

            // Count total days from reference BS date to the target BS date
            int totalDays = 0;

            // Add full years
            for (int y = ReferenceBsYear; y < bsYear; y++)
            {
                totalDays += GetBsYearDays(y);
            }

            // Add full months of the target year
            for (int m = 1; m < bsMonth; m++)
            {
                totalDays += GetBsMonthDays(bsYear, m);
            }

            // Add remaining days (minus 1 because reference starts at day 1)
            totalDays += bsDay - 1;

            return ReferenceAdDate.AddDays(totalDays);
        }

        /// <summary>
        /// Format BS date as "YYYY-MM-DD" (e.g., "2081-10-06").
        /// </summary>
        public static string FormatBsDate(DateTime adDate, string format = "yyyy-MM-dd")
        {
            var (year, month, day) = AdToBs(adDate);
            return format
                .Replace("yyyy", year.ToString("D4"))
                .Replace("MM", month.ToString("D2"))
                .Replace("dd", day.ToString("D2"))
                .Replace("MMMM", NepaliMonths[month - 1])
                .Replace("MMM", NepaliMonths[month - 1][..3]);
        }

        /// <summary>
        /// Format BS date with Nepali month name (e.g., "06 Magh 2081").
        /// </summary>
        public static string FormatBsDateWithMonth(DateTime adDate)
        {
            var (year, month, day) = AdToBs(adDate);
            return $"{day:D2} {NepaliMonths[month - 1]} {year}";
        }

        /// <summary>
        /// Format BS date short (e.g., "Magh-06-81").
        /// </summary>
        public static string FormatBsDateShort(DateTime adDate)
        {
            var (year, month, day) = AdToBs(adDate);
            return $"{NepaliMonths[month - 1][..3]}-{day:D2}-{(year % 100):D2}";
        }

        /// <summary>
        /// Get the Nepali month name (English) for a 1-based month index.
        /// </summary>
        public static string GetMonthName(int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));
            return NepaliMonths[month - 1];
        }

        /// <summary>
        /// Get the Nepali month name (Devanagari) for a 1-based month index.
        /// </summary>
        public static string GetMonthNameNp(int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));
            return NepaliMonthsNp[month - 1];
        }

        /// <summary>
        /// Parse a BS date string "YYYY-MM-DD" to (year, month, day).
        /// </summary>
        public static (int Year, int Month, int Day) ParseBsDate(string bsDateStr)
        {
            var parts = bsDateStr.Split('-');
            if (parts.Length != 3)
                throw new FormatException("BS date must be in YYYY-MM-DD format.");

            return (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        /// <summary>
        /// Parse a BS date string "YYYY-MM-DD" and convert to AD DateTime.
        /// </summary>
        public static DateTime ParseBsToAd(string bsDateStr)
        {
            var (year, month, day) = ParseBsDate(bsDateStr);
            return BsToAd(year, month, day);
        }
    }
}

/**
 * Nepali (Bikram Sambat) Date Utilities
 * Handles BS↔AD conversion and provides a simple calendar picker widget.
 */
var NepaliDate = (function () {
    // BS month days lookup (2000-2090)
    var bsMonthDays = {
        2000: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2001: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2002: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2003: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2004: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2005: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2006: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2007: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2008: [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31],
        2009: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2010: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2011: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2012: [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
        2013: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2014: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2015: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2016: [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
        2017: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2018: [31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2019: [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2020: [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
        2021: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2022: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
        2023: [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2024: [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
        2025: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2026: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2027: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2028: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2029: [31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30],
        2030: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2031: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2032: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2033: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2034: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2035: [30, 32, 31, 32, 31, 31, 29, 30, 30, 29, 29, 31],
        2036: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2037: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2038: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2039: [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
        2040: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2041: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2042: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2043: [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
        2044: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2045: [31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2046: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2047: [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
        2048: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2049: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
        2050: [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2051: [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
        2052: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2053: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
        2054: [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2055: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2056: [31, 31, 32, 31, 32, 30, 30, 29, 30, 29, 30, 30],
        2057: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2058: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2059: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2060: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2061: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2062: [30, 32, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
        2063: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2064: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2065: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2066: [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
        2067: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2068: [31, 31, 32, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2069: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2070: [31, 31, 31, 32, 31, 31, 29, 30, 30, 29, 30, 30],
        2071: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2072: [31, 32, 31, 32, 31, 30, 30, 29, 30, 29, 30, 30],
        2073: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 31],
        2074: [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
        2075: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2076: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
        2077: [31, 32, 31, 32, 31, 30, 30, 30, 29, 30, 29, 31],
        2078: [31, 31, 31, 32, 31, 31, 30, 29, 30, 29, 30, 30],
        2079: [31, 31, 32, 31, 31, 31, 30, 29, 30, 29, 30, 30],
        2080: [31, 32, 31, 32, 31, 30, 30, 30, 29, 29, 30, 30],
        2081: [31, 31, 32, 32, 31, 30, 30, 30, 29, 30, 30, 30],
        2082: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
        2083: [31, 31, 32, 31, 31, 30, 30, 30, 29, 30, 30, 30],
        2084: [31, 31, 32, 31, 31, 30, 30, 30, 29, 30, 30, 30],
        2085: [31, 32, 31, 32, 30, 31, 30, 30, 29, 30, 30, 30],
        2086: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
        2087: [31, 31, 32, 31, 31, 31, 30, 30, 29, 30, 30, 30],
        2088: [30, 31, 32, 32, 30, 31, 30, 30, 29, 30, 30, 30],
        2089: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30],
        2090: [30, 32, 31, 32, 31, 30, 30, 30, 29, 30, 30, 30]
    };

    var monthNames = [
        'Baisakh', 'Jestha', 'Ashadh', 'Shrawan', 'Bhadra', 'Ashwin',
        'Kartik', 'Mangsir', 'Poush', 'Magh', 'Falgun', 'Chaitra'
    ];

    var monthNamesShort = [
        'Bai', 'Jes', 'Ash', 'Shr', 'Bhd', 'Asw',
        'Kar', 'Man', 'Pou', 'Mag', 'Fal', 'Cha'
    ];

    var dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

    // Reference: BS 2000/01/01 = AD 1943/04/14
    var refAdDate = new Date(1943, 3, 14); // month is 0-based in JS

    function getDaysInBsMonth(year, month) {
        if (!bsMonthDays[year]) return 30;
        return bsMonthDays[year][month - 1];
    }

    function getDaysInBsYear(year) {
        if (!bsMonthDays[year]) return 365;
        var total = 0;
        for (var i = 0; i < 12; i++) total += bsMonthDays[year][i];
        return total;
    }

    // Convert AD to BS
    function adToBs(adDate) {
        var d = new Date(adDate.getFullYear(), adDate.getMonth(), adDate.getDate());
        var totalDays = Math.floor((d - refAdDate) / (1000 * 60 * 60 * 24));

        if (totalDays < 0) return null;

        var bsY = 2000, bsM = 1, bsD = 1;
        while (totalDays > 0) {
            var dim = getDaysInBsMonth(bsY, bsM);
            var left = dim - bsD;
            if (totalDays <= left) {
                bsD += totalDays;
                totalDays = 0;
            } else {
                totalDays -= (left + 1);
                bsM++;
                if (bsM > 12) { bsM = 1; bsY++; }
                bsD = 1;
            }
        }
        return { year: bsY, month: bsM, day: bsD };
    }

    // Convert BS to AD
    function bsToAd(bsYear, bsMonth, bsDay) {
        var totalDays = 0;
        for (var y = 2000; y < bsYear; y++) {
            totalDays += getDaysInBsYear(y);
        }
        for (var m = 1; m < bsMonth; m++) {
            totalDays += getDaysInBsMonth(bsYear, m);
        }
        totalDays += bsDay - 1;
        var result = new Date(refAdDate.getTime());
        result.setDate(result.getDate() + totalDays);
        return result;
    }

    // Format BS date as YYYY-MM-DD
    function formatBs(bs) {
        if (!bs) return '';
        var y = bs.year.toString();
        var m = bs.month < 10 ? '0' + bs.month : '' + bs.month;
        var d = bs.day < 10 ? '0' + bs.day : '' + bs.day;
        return y + '-' + m + '-' + d;
    }

    // Format AD date as YYYY-MM-DD
    function formatAd(ad) {
        var y = ad.getFullYear();
        var m = ad.getMonth() + 1;
        var d = ad.getDate();
        return y + '-' + (m < 10 ? '0' + m : m) + '-' + (d < 10 ? '0' + d : d);
    }

    // Parse BS date string
    function parseBs(str) {
        if (!str) return null;
        var parts = str.split('-');
        if (parts.length !== 3) return null;
        var y = parseInt(parts[0], 10);
        var m = parseInt(parts[1], 10);
        var d = parseInt(parts[2], 10);
        if (isNaN(y) || isNaN(m) || isNaN(d)) return null;
        if (m < 1 || m > 12) return null;
        if (d < 1 || d > getDaysInBsMonth(y, m)) return null;
        return { year: y, month: m, day: d };
    }

    // Get day of week (0=Sun) for first day of a BS month
    function getFirstDayOfBsMonth(bsYear, bsMonth) {
        var ad = bsToAd(bsYear, bsMonth, 1);
        return ad.getDay();
    }

    // ==================== Calendar Picker Widget ====================

    function createPicker(inputId, hiddenFieldId) {
        var input = document.getElementById(inputId);
        var hiddenField = document.getElementById(hiddenFieldId);
        if (!input) return;

        var currentBs = null;
        var viewYear, viewMonth;

        // Parse initial value
        var initialVal = input.value;
        if (initialVal) {
            currentBs = parseBs(initialVal);
        }
        if (!currentBs) {
            currentBs = adToBs(new Date());
        }
        viewYear = currentBs.year;
        viewMonth = currentBs.month;

        // Set input value
        input.value = formatBs(currentBs);
        syncHiddenField(currentBs);

        // Create calendar container
        var calendarWrap = document.createElement('div');
        calendarWrap.className = 'np-calendar-wrap';
        calendarWrap.style.display = 'none';

        // Position relative to input
        var wrapper = document.createElement('div');
        wrapper.className = 'np-datepicker-wrapper';
        wrapper.style.position = 'relative';
        input.parentNode.insertBefore(wrapper, input);
        wrapper.appendChild(input);
        wrapper.appendChild(calendarWrap);

        function syncHiddenField(bs) {
            if (hiddenField && bs) {
                var ad = bsToAd(bs.year, bs.month, bs.day);
                hiddenField.value = formatAd(ad);
            }
        }

        function renderCalendar() {
            var daysInMonth = getDaysInBsMonth(viewYear, viewMonth);
            var firstDay = getFirstDayOfBsMonth(viewYear, viewMonth);

            var html = '<div class="np-cal-header">';
            html += '<button type="button" class="np-cal-nav np-cal-prev" title="Previous month">&laquo;</button>';
            html += '<span class="np-cal-title">' + monthNames[viewMonth - 1] + ' ' + viewYear + '</span>';
            html += '<button type="button" class="np-cal-nav np-cal-next" title="Next month">&raquo;</button>';
            html += '</div>';

            html += '<table class="np-cal-table"><thead><tr>';
            for (var i = 0; i < 7; i++) {
                html += '<th>' + dayNames[i] + '</th>';
            }
            html += '</tr></thead><tbody>';

            var dayCount = 1;
            for (var row = 0; row < 6; row++) {
                if (dayCount > daysInMonth) break;
                html += '<tr>';
                for (var col = 0; col < 7; col++) {
                    if (row === 0 && col < firstDay) {
                        html += '<td></td>';
                    } else if (dayCount > daysInMonth) {
                        html += '<td></td>';
                    } else {
                        var isSelected = currentBs &&
                            currentBs.year === viewYear &&
                            currentBs.month === viewMonth &&
                            currentBs.day === dayCount;
                        var todayBs = adToBs(new Date());
                        var isToday = todayBs &&
                            todayBs.year === viewYear &&
                            todayBs.month === viewMonth &&
                            todayBs.day === dayCount;
                        var cls = 'np-cal-day';
                        if (isSelected) cls += ' np-cal-selected';
                        if (isToday) cls += ' np-cal-today';
                        html += '<td><button type="button" class="' + cls + '" data-day="' + dayCount + '">' + dayCount + '</button></td>';
                        dayCount++;
                    }
                }
                html += '</tr>';
            }
            html += '</tbody></table>';
            calendarWrap.innerHTML = html;

            // Attach events
            var prevBtn = calendarWrap.querySelector('.np-cal-prev');
            var nextBtn = calendarWrap.querySelector('.np-cal-next');
            if (prevBtn) prevBtn.addEventListener('click', function (e) {
                e.preventDefault();
                viewMonth--;
                if (viewMonth < 1) { viewMonth = 12; viewYear--; }
                renderCalendar();
            });
            if (nextBtn) nextBtn.addEventListener('click', function (e) {
                e.preventDefault();
                viewMonth++;
                if (viewMonth > 12) { viewMonth = 1; viewYear++; }
                renderCalendar();
            });

            var dayBtns = calendarWrap.querySelectorAll('.np-cal-day');
            dayBtns.forEach(function (btn) {
                btn.addEventListener('click', function (e) {
                    e.preventDefault();
                    var day = parseInt(btn.getAttribute('data-day'), 10);
                    currentBs = { year: viewYear, month: viewMonth, day: day };
                    input.value = formatBs(currentBs);
                    syncHiddenField(currentBs);
                    calendarWrap.style.display = 'none';
                    renderCalendar();
                });
            });
        }

        // Toggle calendar on input focus/click
        input.addEventListener('focus', function () {
            calendarWrap.style.display = 'block';
            renderCalendar();
        });

        input.addEventListener('click', function () {
            calendarWrap.style.display = 'block';
            renderCalendar();
        });

        // Close when clicking outside
        document.addEventListener('click', function (e) {
            if (!wrapper.contains(e.target)) {
                calendarWrap.style.display = 'none';
            }
        });

        // Manual input change
        input.addEventListener('change', function () {
            var bs = parseBs(input.value);
            if (bs) {
                currentBs = bs;
                viewYear = bs.year;
                viewMonth = bs.month;
                syncHiddenField(bs);
                renderCalendar();
            }
        });
    }

    return {
        adToBs: adToBs,
        bsToAd: bsToAd,
        format: formatBs,
        formatAd: formatAd,
        parse: parseBs,
        monthNames: monthNames,
        monthNamesShort: monthNamesShort,
        getDaysInMonth: getDaysInBsMonth,
        createPicker: createPicker
    };
})();

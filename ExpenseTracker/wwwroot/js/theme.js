/**
 * Theme switcher — persists preference in localStorage.
 * Dark is the default theme.
 */
(function () {
    'use strict';

    var STORAGE_KEY = 'expense-tracker-theme';
    var DARK = 'dark';
    var LIGHT = 'light';

    // Syncfusion CDN theme URLs
    var SF_DARK = 'https://cdn.syncfusion.com/ej2/20.3.47/bootstrap5-dark.css';
    var SF_LIGHT = 'https://cdn.syncfusion.com/ej2/20.3.47/bootstrap5.css';

    /**
     * Apply theme immediately (before DOMContentLoaded to avoid FOUC).
     */
    function getStoredTheme() {
        try { return localStorage.getItem(STORAGE_KEY); } catch (e) { return null; }
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);

        // Swap Syncfusion stylesheet
        var sfLink = document.getElementById('sf-theme-css');
        if (sfLink) {
            sfLink.href = theme === LIGHT ? SF_LIGHT : SF_DARK;
        }

        // Update toggle icon
        var icon = document.getElementById('themeToggleIcon');
        if (icon) {
            if (theme === LIGHT) {
                icon.classList.remove('fa-sun');
                icon.classList.add('fa-moon');
                icon.title = 'Switch to Dark Mode';
            } else {
                icon.classList.remove('fa-moon');
                icon.classList.add('fa-sun');
                icon.title = 'Switch to Light Mode';
            }
        }

        // Update chart backgrounds if they exist
        updateChartTheme(theme);
    }

    function toggleTheme() {
        var current = document.documentElement.getAttribute('data-theme') || DARK;
        var next = current === DARK ? LIGHT : DARK;
        try { localStorage.setItem(STORAGE_KEY, next); } catch (e) { }
        applyTheme(next);
    }

    function updateChartTheme(theme) {
        var bgColor = theme === LIGHT ? '#ffffff' : '#1a222b';
        var legendColor = theme === LIGHT ? '#212b36' : '#ffffff';
        var gridColor = theme === LIGHT ? '#dfe3e8' : '#32414d';

        // Spline chart
        var splineEl = document.getElementById('spline-chart');
        if (splineEl && splineEl.ej2_instances && splineEl.ej2_instances[0]) {
            var chart = splineEl.ej2_instances[0];
            chart.background = bgColor;
            if (chart.legendSettings && chart.legendSettings.textStyle) {
                chart.legendSettings.textStyle.color = legendColor;
            }
            if (chart.primaryYAxis && chart.primaryYAxis.majorGridLines) {
                chart.primaryYAxis.majorGridLines.color = gridColor;
            }
            if (chart.primaryXAxis && chart.primaryXAxis.labelStyle) {
                chart.primaryXAxis.labelStyle.color = legendColor;
            }
            if (chart.primaryYAxis && chart.primaryYAxis.labelStyle) {
                chart.primaryYAxis.labelStyle.color = legendColor;
            }
            chart.refresh();
        }

        // Doughnut chart
        var doughnutEl = document.getElementById('doughnutchart');
        if (doughnutEl && doughnutEl.ej2_instances && doughnutEl.ej2_instances[0]) {
            var accChart = doughnutEl.ej2_instances[0];
            accChart.background = bgColor;
            if (accChart.legendSettings && accChart.legendSettings.textStyle) {
                accChart.legendSettings.textStyle.color = legendColor;
            }
            accChart.refresh();
        }
    }

    // Apply theme immediately from localStorage (before paint)
    var stored = getStoredTheme();
    if (stored === LIGHT) {
        document.documentElement.setAttribute('data-theme', LIGHT);
    }

    // Wire up toggle after DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        var theme = getStoredTheme() || DARK;
        applyTheme(theme);

        var btn = document.getElementById('themeToggleBtn');
        if (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleTheme();
            });
        }
    });
})();

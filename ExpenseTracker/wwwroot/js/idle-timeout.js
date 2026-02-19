/**
 * Session Idle Timeout — warns the user after inactivity,
 * then logs them out if they don't respond.
 *
 * Idle timeout : 15 minutes (matches server cookie expiry)
 * Warning shown : 2 minutes before timeout (at 13 min)
 * Countdown     : 120 seconds visible in the popup
 */
(function () {
    'use strict';

    var IDLE_LIMIT_MS   = 15 * 60 * 1000;   // 15 minutes total
    var WARN_BEFORE_MS  =  2 * 60 * 1000;   // show warning 2 min before
    var WARN_AT_MS      = IDLE_LIMIT_MS - WARN_BEFORE_MS; // 13 min

    var idleTimer    = null;
    var warnTimer    = null;
    var countdownId  = null;
    var navbarTickId = null;
    var lastActivity = Date.now();
    var overlay, modal, countdownEl, stayBtn, logoutBtn;
    var navBadge, navText;

    // Activity events to track (only deliberate interactions reset timer)
    var EVENTS = ['mousedown', 'keydown', 'touchstart', 'click'];

    function init() {
        // Build popup DOM
        buildPopup();

        // Get references
        overlay     = document.getElementById('idleOverlay');
        modal       = document.getElementById('idleModal');
        countdownEl = document.getElementById('idleCountdown');
        stayBtn     = document.getElementById('idleStayBtn');
        logoutBtn   = document.getElementById('idleLogoutBtn');
        navBadge    = document.getElementById('sessionTimerBadge');
        navText     = document.getElementById('sessionTimerText');

        // Button handlers
        stayBtn.addEventListener('click', function () {
            hideWarning();
            resetTimers();
            // Ping server to refresh the sliding cookie
            fetch(window.location.href, { method: 'HEAD', credentials: 'same-origin' }).catch(function(){});
        });

        logoutBtn.addEventListener('click', function () {
            doLogout();
        });

        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                hideWarning();
                resetTimers();
            }
        });

        // Start tracking
        EVENTS.forEach(function (evt) {
            document.addEventListener(evt, onActivity, { passive: true });
        });

        resetTimers();
    }

    function onActivity() {
        // Only reset if the warning isn't showing
        if (!overlay || overlay.style.display !== 'flex') {
            lastActivity = Date.now();
            resetTimers();
        }
    }

    function resetTimers() {
        clearTimeout(idleTimer);
        clearTimeout(warnTimer);
        clearInterval(countdownId);
        clearInterval(navbarTickId);

        lastActivity = Date.now();

        // Update navbar timer every second
        updateNavbarTimer();
        navbarTickId = setInterval(updateNavbarTimer, 1000);

        // After 13 min of idle → show warning
        warnTimer = setTimeout(function () {
            showWarning();
        }, WARN_AT_MS);

        // After 15 min of idle → force logout
        idleTimer = setTimeout(function () {
            doLogout();
        }, IDLE_LIMIT_MS);
    }

    function updateNavbarTimer() {
        if (!navText || !navBadge) return;
        var elapsed = Date.now() - lastActivity;
        var remaining = Math.max(0, Math.floor((IDLE_LIMIT_MS - elapsed) / 1000));
        navText.textContent = formatTime(remaining);

        // Change badge color based on remaining time
        navBadge.classList.remove('timer-warning', 'timer-danger');
        if (remaining <= 120) {
            navBadge.classList.add('timer-danger');
        } else if (remaining <= 300) {
            navBadge.classList.add('timer-warning');
        }
    }

    function showWarning() {
        var secondsLeft = Math.floor(WARN_BEFORE_MS / 1000);
        countdownEl.textContent = formatTime(secondsLeft);
        overlay.style.display = 'flex';

        countdownId = setInterval(function () {
            secondsLeft--;
            if (secondsLeft <= 0) {
                clearInterval(countdownId);
                doLogout();
                return;
            }
            countdownEl.textContent = formatTime(secondsLeft);
            // Pulse effect when under 30s
            if (secondsLeft <= 30) {
                countdownEl.classList.add('idle-urgent');
            }
        }, 1000);
    }

    function hideWarning() {
        clearInterval(countdownId);
        overlay.style.display = 'none';
        countdownEl.classList.remove('idle-urgent');
    }

    function doLogout() {
        hideWarning();
        window.location.href = '/Account/Logout?expired=true';
    }

    function formatTime(sec) {
        var m = Math.floor(sec / 60);
        var s = sec % 60;
        return (m < 10 ? '0' : '') + m + ':' + (s < 10 ? '0' : '') + s;
    }

    function buildPopup() {
        // Don't add twice
        if (document.getElementById('idleOverlay')) return;

        var html =
            '<div id="idleOverlay" class="idle-overlay" style="display:none;">' +
                '<div id="idleModal" class="idle-modal">' +
                    '<div class="idle-icon"><i class="fa-solid fa-clock"></i></div>' +
                    '<h4 class="idle-title">Session Timeout Warning</h4>' +
                    '<p class="idle-text">You\'ve been inactive for a while. Your session will expire in</p>' +
                    '<div id="idleCountdown" class="idle-countdown">02:00</div>' +
                    '<p class="idle-text idle-sub">Click "Stay Logged In" to continue your session.</p>' +
                    '<div class="idle-actions">' +
                        '<button id="idleStayBtn" class="idle-btn idle-btn-primary">' +
                            '<i class="fa-solid fa-rotate-right me-1"></i> Stay Logged In' +
                        '</button>' +
                        '<button id="idleLogoutBtn" class="idle-btn idle-btn-outline">' +
                            '<i class="fa-solid fa-right-from-bracket me-1"></i> Logout Now' +
                        '</button>' +
                    '</div>' +
                '</div>' +
            '</div>';

        var wrapper = document.createElement('div');
        wrapper.innerHTML = html;
        document.body.appendChild(wrapper.firstChild);
    }

    // Wait for DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

// Accessibility helper for screen reader announcements
window.accessibilityHelper = (function () {
    let liveRegionPolite = null;
    let liveRegionAssertive = null;
    let announceTimeout = null;

    // Initialize ARIA live regions
    function initialize() {
        // Create polite live region if it doesn't exist
        if (!liveRegionPolite) {
            liveRegionPolite = document.createElement('div');
            liveRegionPolite.setAttribute('role', 'status');
            liveRegionPolite.setAttribute('aria-live', 'polite');
            liveRegionPolite.setAttribute('aria-atomic', 'true');
            liveRegionPolite.className = 'sr-only';
            liveRegionPolite.style.position = 'absolute';
            liveRegionPolite.style.left = '-10000px';
            liveRegionPolite.style.width = '1px';
            liveRegionPolite.style.height = '1px';
            liveRegionPolite.style.overflow = 'hidden';
            document.body.appendChild(liveRegionPolite);
        }

        // Create assertive live region if it doesn't exist
        if (!liveRegionAssertive) {
            liveRegionAssertive = document.createElement('div');
            liveRegionAssertive.setAttribute('role', 'alert');
            liveRegionAssertive.setAttribute('aria-live', 'assertive');
            liveRegionAssertive.setAttribute('aria-atomic', 'true');
            liveRegionAssertive.className = 'sr-only';
            liveRegionAssertive.style.position = 'absolute';
            liveRegionAssertive.style.left = '-10000px';
            liveRegionAssertive.style.width = '1px';
            liveRegionAssertive.style.height = '1px';
            liveRegionAssertive.style.overflow = 'hidden';
            document.body.appendChild(liveRegionAssertive);
        }
    }

    // Announce a message to screen readers
    function announce(message, priority = 'polite') {
        // Initialize if not already done
        if (!liveRegionPolite || !liveRegionAssertive) {
            initialize();
        }

        // Clear any pending announcements
        if (announceTimeout) {
            clearTimeout(announceTimeout);
        }

        // Choose the appropriate live region
        const liveRegion = priority === 'assertive' ? liveRegionAssertive : liveRegionPolite;

        // Clear the live region first (helps with repeated messages)
        liveRegion.textContent = '';

        // Set the message after a brief delay to ensure screen readers pick it up
        announceTimeout = setTimeout(() => {
            liveRegion.textContent = message;
            console.log(`[A11y Announce] [${priority}]: ${message}`);
        }, 100);
    }

    // Auto-initialize when the script loads
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

    return {
        initialize,
        announce
    };
})();

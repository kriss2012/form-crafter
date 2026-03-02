// Rotation handler for canvas elements
window.rotationHandler = {
    dotNetHelper: null,
    canvasId: null,
    isRotating: false,
    elementId: null,
    centerX: 0,
    centerY: 0,

    initialize: function (canvasId, dotNetHelper) {
        this.canvasId = canvasId;
        this.dotNetHelper = dotNetHelper;
    },

    startRotate: function (elementId, centerX, centerY, clientX, clientY) {
        this.isRotating = true;
        this.elementId = elementId;
        this.centerX = centerX;
        this.centerY = centerY;

        // Calculate initial angle
        const initialAngle = this.calculateAngle(clientX, clientY);

        // Add event listeners
        document.addEventListener('mousemove', this.handleMouseMove.bind(this));
        document.addEventListener('mouseup', this.handleMouseUp.bind(this));

        return initialAngle;
    },

    handleMouseMove: function (e) {
        if (!this.isRotating) return;

        const angle = this.calculateAngle(e.clientX, e.clientY);

        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnRotateMove', angle);
        }
    },

    handleMouseUp: function (e) {
        if (!this.isRotating) return;

        const angle = this.calculateAngle(e.clientX, e.clientY);

        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnRotateEnd', angle);
        }

        this.cleanup();
    },

    calculateAngle: function (clientX, clientY) {
        // Calculate angle from center point
        const dx = clientX - this.centerX;
        const dy = clientY - this.centerY;

        // Convert to degrees (atan2 returns radians)
        let angle = Math.atan2(dy, dx) * (180 / Math.PI);

        // Adjust to match CSS rotation (0 degrees = top, positive = clockwise)
        angle = angle + 90;

        return angle;
    },

    cleanup: function () {
        document.removeEventListener('mousemove', this.handleMouseMove.bind(this));
        document.removeEventListener('mouseup', this.handleMouseUp.bind(this));
        this.isRotating = false;
        this.elementId = null;
    }
};

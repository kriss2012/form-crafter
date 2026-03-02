// Smooth Canvas Drag Handler
window.canvasDragHandler = {
    activeElement: null,
    canvasElement: null,
    dragOffsetX: 0,
    dragOffsetY: 0,
    isDragging: false,
    dotNetHelper: null,

    initialize: function (canvasId, dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.canvasElement = document.querySelector(`[data-canvas-id="${canvasId}"]`);

        if (!this.canvasElement) {
            console.error('Canvas element not found');
            return;
        }

        // Add global mouse move and up listeners for smooth dragging
        document.addEventListener('mousemove', this.handleGlobalMouseMove.bind(this));
        document.addEventListener('mouseup', this.handleGlobalMouseUp.bind(this));
    },

    startDrag: function (elementId, offsetX, offsetY) {
        this.isDragging = true;
        this.activeElement = elementId;
        this.dragOffsetX = offsetX;
        this.dragOffsetY = offsetY;
    },

    handleGlobalMouseMove: function (e) {
        if (!this.isDragging || !this.canvasElement) {
            return;
        }

        // Get canvas bounding rect for accurate positioning
        const canvasRect = this.canvasElement.getBoundingClientRect();

        // Calculate position relative to canvas
        const x = e.clientX - canvasRect.left - this.dragOffsetX;
        const y = e.clientY - canvasRect.top - this.dragOffsetY;

        // Send position update to Blazor
        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnDragMove', x, y);
        }
    },

    handleGlobalMouseUp: function (e) {
        if (!this.isDragging || !this.canvasElement) {
            return;
        }

        // Get canvas bounding rect for accurate positioning
        const canvasRect = this.canvasElement.getBoundingClientRect();

        // Calculate final position relative to canvas
        const x = e.clientX - canvasRect.left - this.dragOffsetX;
        const y = e.clientY - canvasRect.top - this.dragOffsetY;

        // Send drop event to Blazor
        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnDragEnd', x, y);
        }

        this.isDragging = false;
        this.activeElement = null;
    },

    cleanup: function () {
        document.removeEventListener('mousemove', this.handleGlobalMouseMove);
        document.removeEventListener('mouseup', this.handleGlobalMouseUp);
        this.dotNetHelper = null;
    }
};

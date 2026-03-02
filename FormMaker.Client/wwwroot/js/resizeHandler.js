// Element resize handler for the form editor
window.resizeHandler = {
    activeElement: null,
    activeHandle: null,
    canvasElement: null,
    startX: 0,
    startY: 0,
    startWidth: 0,
    startHeight: 0,
    startLeft: 0,
    startTop: 0,
    isResizing: false,
    dotNetHelper: null,
    minWidth: 50,
    minHeight: 30,

    initialize: function (canvasId, dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.canvasElement = document.querySelector(`[data-canvas-id="${canvasId}"]`);
        document.addEventListener('mousemove', this.handleGlobalMouseMove.bind(this));
        document.addEventListener('mouseup', this.handleGlobalMouseUp.bind(this));
    },

    startResize: function (elementId, handleType, clientX, clientY, width, height, left, top) {
        this.isResizing = true;
        this.activeElement = elementId;
        this.activeHandle = handleType;

        // Convert client coordinates to canvas-relative coordinates
        if (this.canvasElement) {
            const canvasRect = this.canvasElement.getBoundingClientRect();
            this.startX = clientX - canvasRect.left;
            this.startY = clientY - canvasRect.top;
        } else {
            this.startX = clientX;
            this.startY = clientY;
        }

        this.startWidth = width;
        this.startHeight = height;
        this.startLeft = left;
        this.startTop = top;
    },

    handleGlobalMouseMove: function (e) {
        if (!this.isResizing || !this.canvasElement) return;

        const canvasRect = this.canvasElement.getBoundingClientRect();
        const currentX = e.clientX - canvasRect.left;
        const currentY = e.clientY - canvasRect.top;
        const deltaX = currentX - this.startX;
        const deltaY = currentY - this.startY;

        let newWidth = this.startWidth;
        let newHeight = this.startHeight;
        let newLeft = this.startLeft;
        let newTop = this.startTop;

        // Calculate new dimensions based on handle type
        switch (this.activeHandle) {
            case 'nw': // Top-left corner
                newWidth = Math.max(this.minWidth, this.startWidth - deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight - deltaY);
                newLeft = this.startLeft + (this.startWidth - newWidth);
                newTop = this.startTop + (this.startHeight - newHeight);
                break;
            case 'ne': // Top-right corner
                newWidth = Math.max(this.minWidth, this.startWidth + deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight - deltaY);
                newTop = this.startTop + (this.startHeight - newHeight);
                break;
            case 'sw': // Bottom-left corner
                newWidth = Math.max(this.minWidth, this.startWidth - deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight + deltaY);
                newLeft = this.startLeft + (this.startWidth - newWidth);
                break;
            case 'se': // Bottom-right corner
                newWidth = Math.max(this.minWidth, this.startWidth + deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight + deltaY);
                break;
            case 'n': // Top edge
                newHeight = Math.max(this.minHeight, this.startHeight - deltaY);
                newTop = this.startTop + (this.startHeight - newHeight);
                break;
            case 's': // Bottom edge
                newHeight = Math.max(this.minHeight, this.startHeight + deltaY);
                break;
            case 'w': // Left edge
                newWidth = Math.max(this.minWidth, this.startWidth - deltaX);
                newLeft = this.startLeft + (this.startWidth - newWidth);
                break;
            case 'e': // Right edge
                newWidth = Math.max(this.minWidth, this.startWidth + deltaX);
                break;
        }

        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnResize', newWidth, newHeight, newLeft, newTop);
        }
    },

    handleGlobalMouseUp: function (e) {
        if (!this.isResizing || !this.canvasElement) return;

        const canvasRect = this.canvasElement.getBoundingClientRect();
        const currentX = e.clientX - canvasRect.left;
        const currentY = e.clientY - canvasRect.top;
        const deltaX = currentX - this.startX;
        const deltaY = currentY - this.startY;

        let newWidth = this.startWidth;
        let newHeight = this.startHeight;
        let newLeft = this.startLeft;
        let newTop = this.startTop;

        // Calculate final dimensions
        switch (this.activeHandle) {
            case 'nw':
                newWidth = Math.max(this.minWidth, this.startWidth - deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight - deltaY);
                newLeft = this.startLeft + (this.startWidth - newWidth);
                newTop = this.startTop + (this.startHeight - newHeight);
                break;
            case 'ne':
                newWidth = Math.max(this.minWidth, this.startWidth + deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight - deltaY);
                newTop = this.startTop + (this.startHeight - newHeight);
                break;
            case 'sw':
                newWidth = Math.max(this.minWidth, this.startWidth - deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight + deltaY);
                newLeft = this.startLeft + (this.startWidth - newWidth);
                break;
            case 'se':
                newWidth = Math.max(this.minWidth, this.startWidth + deltaX);
                newHeight = Math.max(this.minHeight, this.startHeight + deltaY);
                break;
            case 'n':
                newHeight = Math.max(this.minHeight, this.startHeight - deltaY);
                newTop = this.startTop + (this.startHeight - newHeight);
                break;
            case 's':
                newHeight = Math.max(this.minHeight, this.startHeight + deltaY);
                break;
            case 'w':
                newWidth = Math.max(this.minWidth, this.startWidth - deltaX);
                newLeft = this.startLeft + (this.startWidth - newWidth);
                break;
            case 'e':
                newWidth = Math.max(this.minWidth, this.startWidth + deltaX);
                break;
        }

        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnResizeEnd', newWidth, newHeight, newLeft, newTop);
        }

        this.isResizing = false;
        this.activeElement = null;
        this.activeHandle = null;
    },

    cleanup: function () {
        document.removeEventListener('mousemove', this.handleGlobalMouseMove.bind(this));
        document.removeEventListener('mouseup', this.handleGlobalMouseUp.bind(this));
        this.dotNetHelper = null;
    }
};

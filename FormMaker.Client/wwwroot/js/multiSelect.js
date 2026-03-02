// Multi-Select Handler for Canvas Elements
window.multiSelectHandler = {
    dotNetHelper: null,
    canvasElement: null,

    initialize: function (canvasId, dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.canvasElement = document.querySelector(`[data-canvas-id="${canvasId}"]`);

        if (!this.canvasElement) {
            console.error('Canvas element not found for multi-select');
            return;
        }

        // Add click listener to canvas elements
        this.canvasElement.addEventListener('click', this.handleElementClick.bind(this), true);
    },

    handleElementClick: function (e) {
        // Check if the click is on a canvas element
        const elementDiv = e.target.closest('.canvas-element');

        if (!elementDiv) {
            return; // Not clicking on an element
        }

        // Get the element ID from the data attribute or find it another way
        const elementId = this.getElementId(elementDiv);

        if (elementId && this.dotNetHelper) {
            // Prevent default click behavior
            e.stopPropagation();

            // Check for modifier keys
            const ctrlKey = e.ctrlKey || e.metaKey; // metaKey for Mac
            const shiftKey = e.shiftKey;

            // Call Blazor method with modifier keys
            this.dotNetHelper.invokeMethodAsync('OnElementClickWithModifiers', elementId, ctrlKey, shiftKey);
        }
    },

    getElementId: function (elementDiv) {
        // Try to find element ID from data attribute or other means
        // For now, we'll need to add data-element-id to canvas elements
        return elementDiv.getAttribute('data-element-id');
    },

    cleanup: function () {
        if (this.canvasElement) {
            this.canvasElement.removeEventListener('click', this.handleElementClick);
        }
        this.dotNetHelper = null;
    }
};

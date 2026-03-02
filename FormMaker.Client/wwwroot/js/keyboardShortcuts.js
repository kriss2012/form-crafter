// Global keyboard shortcut handler for the form editor
window.keyboardShortcuts = {
    dotNetHelper: null,

    initialize: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
    },

    handleKeyDown: function (e) {
        // Only handle shortcuts when not typing in input fields
        const isInputField = e.target.tagName === 'INPUT' ||
                           e.target.tagName === 'TEXTAREA' ||
                           e.target.isContentEditable;

        if (isInputField) {
            return; // Don't handle shortcuts when typing
        }

        const isCtrl = e.ctrlKey || e.metaKey; // Support both Ctrl (Windows/Linux) and Cmd (Mac)

        // Ctrl+Z: Undo
        if (isCtrl && e.key === 'z' && !e.shiftKey) {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnUndo');
            }
        }
        // Ctrl+Y or Ctrl+Shift+Z: Redo
        else if (isCtrl && (e.key === 'y' || (e.key === 'z' && e.shiftKey))) {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnRedo');
            }
        }
        // Ctrl+D: Duplicate
        else if (isCtrl && e.key === 'd') {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnDuplicate');
            }
        }
        // Ctrl+C: Copy
        else if (isCtrl && e.key === 'c') {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnCopy');
            }
        }
        // Ctrl+V: Paste
        else if (isCtrl && e.key === 'v') {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnPaste');
            }
        }
        // Delete or Backspace: Delete selected element
        else if (e.key === 'Delete' || e.key === 'Backspace') {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnDelete');
            }
        }
        // Ctrl+/ or F1: Show keyboard shortcuts help
        else if ((isCtrl && e.key === '/') || e.key === 'F1') {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnShowKeyboardShortcuts');
            }
        }
        // Ctrl+A: Select all elements on canvas
        else if (isCtrl && e.key === 'a') {
            e.preventDefault();
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnSelectAll');
            }
        }
    },

    cleanup: function () {
        document.removeEventListener('keydown', this.handleKeyDown.bind(this));
        this.dotNetHelper = null;
    }
};

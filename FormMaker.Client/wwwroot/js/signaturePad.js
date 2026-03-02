// Signature Pad Handler for FormMaker
window.signaturePad = {
    pads: {},

    initialize: function (elementId, lineWidth, strokeColor) {
        const canvas = document.getElementById(`signature-${elementId}`);
        if (!canvas) {
            console.error('Canvas element not found:', elementId);
            return;
        }

        const ctx = canvas.getContext('2d');
        let isDrawing = false;
        let lastX = 0;
        let lastY = 0;

        // Set canvas dimensions to match display size
        const rect = canvas.getBoundingClientRect();
        canvas.width = rect.width;
        canvas.height = rect.height;

        // Configure drawing style
        ctx.strokeStyle = strokeColor || '#000000';
        ctx.lineWidth = lineWidth || 2;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';

        // Mouse events
        canvas.addEventListener('mousedown', (e) => {
            isDrawing = true;
            const rect = canvas.getBoundingClientRect();
            lastX = e.clientX - rect.left;
            lastY = e.clientY - rect.top;
        });

        canvas.addEventListener('mousemove', (e) => {
            if (!isDrawing) return;

            const rect = canvas.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            ctx.beginPath();
            ctx.moveTo(lastX, lastY);
            ctx.lineTo(x, y);
            ctx.stroke();

            lastX = x;
            lastY = y;
        });

        canvas.addEventListener('mouseup', () => {
            isDrawing = false;
        });

        canvas.addEventListener('mouseleave', () => {
            isDrawing = false;
        });

        // Touch events for mobile
        canvas.addEventListener('touchstart', (e) => {
            e.preventDefault();
            isDrawing = true;
            const rect = canvas.getBoundingClientRect();
            const touch = e.touches[0];
            lastX = touch.clientX - rect.left;
            lastY = touch.clientY - rect.top;
        });

        canvas.addEventListener('touchmove', (e) => {
            e.preventDefault();
            if (!isDrawing) return;

            const rect = canvas.getBoundingClientRect();
            const touch = e.touches[0];
            const x = touch.clientX - rect.left;
            const y = touch.clientY - rect.top;

            ctx.beginPath();
            ctx.moveTo(lastX, lastY);
            ctx.lineTo(x, y);
            ctx.stroke();

            lastX = x;
            lastY = y;
        });

        canvas.addEventListener('touchend', () => {
            isDrawing = false;
        });

        // Store pad reference
        this.pads[elementId] = {
            canvas: canvas,
            ctx: ctx
        };
    },

    clear: function (elementId) {
        const pad = this.pads[elementId];
        if (pad) {
            const ctx = pad.ctx;
            const canvas = pad.canvas;
            ctx.clearRect(0, 0, canvas.width, canvas.height);
        }
    },

    getSignatureData: function (elementId) {
        const pad = this.pads[elementId];
        if (pad) {
            return pad.canvas.toDataURL('image/png');
        }
        return null;
    },

    cleanup: function (elementId) {
        if (this.pads[elementId]) {
            delete this.pads[elementId];
        }
    }
};

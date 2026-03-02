// PDF Export Handler using jsPDF
window.pdfExport = {
    /**
     * Export form template as PDF
     * @param {Object} template - The form template object
     * @param {boolean} includeData - Whether to include filled data (for filled forms)
     * @param {Object} formData - Dictionary of form field values (optional)
     */
    exportToPdf: async function (template, includeData = false, formData = null) {
        try {
            // Access jsPDF from window
            const { jsPDF } = window.jspdf;

            if (!jsPDF) {
                console.error('jsPDF library not loaded');
                return false;
            }

            // Handle property name casing (C# uses PascalCase, might be camelCase after serialization)
            const formName = String(template.name || template.Name || 'Untitled Form');
            const widthPx = template.widthInPixels || template.WidthInPixels || 612;
            const heightPx = template.heightInPixels || template.HeightInPixels || 792;

            // Create PDF document - use template page size
            const doc = new jsPDF({
                orientation: widthPx > heightPx ? 'landscape' : 'portrait',
                unit: 'px',
                format: [widthPx, heightPx]
            });

            // Set document properties
            doc.setProperties({
                title: formName,
                subject: 'Form Builder Export',
                author: 'Form Maker',
                creator: 'Form Maker - Claude Code'
            });

            // Add form title
            doc.setFontSize(20);
            doc.setFont(undefined, 'bold');
            doc.text(formName, 40, 40);

            // Process each element - handle both camelCase and PascalCase
            const isMultiPage = template.isMultiPage || template.IsMultiPage;
            const pages = template.pages || template.Pages;

            // Collect all elements from all pages or from legacy elements list
            let allElements = [];
            if (isMultiPage && pages && pages.length > 0) {
                // Collect elements from all pages
                for (const page of pages) {
                    const pageElements = page.elements || page.Elements || [];
                    allElements = allElements.concat(pageElements);
                }
            } else {
                // Use legacy elements list
                allElements = template.elements || template.Elements || [];
            }

            // Render all elements
            for (const element of allElements) {
                await this.renderElement(doc, element, includeData, formData);
            }

            // Generate filename
            const filename = `${formName.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().split('T')[0]}.pdf`;

            // Save the PDF
            doc.save(filename);

            return true;
        } catch (error) {
            console.error('PDF export error:', error);
            return false;
        }
    },

    /**
     * Render a single form element to PDF
     */
    renderElement: async function (doc, element, includeData, formData) {
        // Handle property name casing
        const x = element.x || element.X || 0;
        const y = element.y || element.Y || 0;
        const width = element.width || element.Width || 100;
        const height = element.height || element.Height || 30;
        const properties = element.properties || element.Properties;
        const elementType = element.type || element.Type;

        // Apply element styling
        if (properties) {
            this.applyElementStyle(doc, properties);
        }

        // Render based on element type
        switch (elementType) {
            case 'Label':
                this.renderLabel(doc, element, x, y, width, height);
                break;
            case 'TextInput':
                this.renderTextInput(doc, element, x, y, width, height, includeData, formData);
                break;
            case 'TextArea':
                this.renderTextArea(doc, element, x, y, width, height, includeData, formData);
                break;
            case 'Checkbox':
                this.renderCheckbox(doc, element, x, y, width, height, includeData, formData);
                break;
            case 'RadioGroup':
                this.renderRadioGroup(doc, element, x, y, width, height, includeData, formData);
                break;
            case 'Dropdown':
                this.renderDropdown(doc, element, x, y, width, height, includeData, formData);
                break;
            case 'DatePicker':
                this.renderDatePicker(doc, element, x, y, width, height, includeData, formData);
                break;
            case 'Image':
                await this.renderImage(doc, element, x, y, width, height);
                break;
            case 'Signature':
                this.renderSignature(doc, element, x, y, width, height, includeData, formData);
                break;
            case 'Table':
                this.renderTable(doc, element, x, y, width, height);
                break;
            case 'Divider':
                this.renderDivider(doc, element, x, y, width, height);
                break;
            case 'FileUpload':
                this.renderFileUpload(doc, element, x, y, width, height);
                break;
            default:
                console.warn(`Unknown element type: ${element.Type}`);
        }
    },

    applyElementStyle: function (doc, properties) {
        // Apply text color
        if (properties.TextColor) {
            doc.setTextColor(properties.TextColor);
        }

        // Apply font size
        if (properties.FontSize) {
            doc.setFontSize(properties.FontSize);
        }

        // Apply font weight
        if (properties.FontWeight === 'bold') {
            doc.setFont(undefined, 'bold');
        } else if (properties.FontStyle === 'italic') {
            doc.setFont(undefined, 'italic');
        } else {
            doc.setFont(undefined, 'normal');
        }
    },

    renderLabel: function (doc, element, x, y, width, height) {
        const label = element.Label || element.Text || '';
        doc.setFontSize(element.Properties?.FontSize || 16);
        doc.text(label, x + 5, y + height / 2 + 5);
    },

    renderTextInput: function (doc, element, x, y, width, height, includeData, formData) {
        // Draw border
        doc.setDrawColor(150);
        doc.rect(x, y, width, height);

        // Add label
        if (element.Label) {
            doc.setFontSize(12);
            doc.text(element.Label, x, y - 5);
        }

        // Add filled value if includeData
        if (includeData && formData && formData[element.Id]) {
            doc.setFontSize(14);
            doc.text(formData[element.Id], x + 5, y + height / 2 + 5);
        } else if (element.Placeholder) {
            doc.setTextColor(150);
            doc.setFontSize(12);
            doc.text(element.Placeholder, x + 5, y + height / 2 + 5);
            doc.setTextColor(0);
        }
    },

    renderTextArea: function (doc, element, x, y, width, height, includeData, formData) {
        // Draw border
        doc.setDrawColor(150);
        doc.rect(x, y, width, height);

        // Add label
        if (element.Label) {
            doc.setFontSize(12);
            doc.text(element.Label, x, y - 5);
        }

        // Add filled value if includeData
        if (includeData && formData && formData[element.Id]) {
            doc.setFontSize(12);
            const lines = doc.splitTextToSize(formData[element.Id], width - 10);
            doc.text(lines, x + 5, y + 15);
        }
    },

    renderCheckbox: function (doc, element, x, y, width, height, includeData, formData) {
        const boxSize = Math.min(20, height);

        // Draw checkbox box
        doc.setDrawColor(0);
        doc.rect(x, y, boxSize, boxSize);

        // Add check if checked
        if (includeData && formData && formData[element.Id]) {
            doc.setFontSize(16);
            doc.text('✓', x + 3, y + boxSize - 3);
        }

        // Add label
        if (element.Label) {
            doc.setFontSize(14);
            doc.text(element.Label, x + boxSize + 10, y + boxSize / 2 + 5);
        }
    },

    renderRadioGroup: function (doc, element, x, y, width, height, includeData, formData) {
        // Add label
        if (element.Label) {
            doc.setFontSize(14);
            doc.setFont(undefined, 'bold');
            doc.text(element.Label, x, y);
            doc.setFont(undefined, 'normal');
        }

        // Render radio options
        const options = element.Options || [];
        const spacing = 25;
        let currentY = y + 20;

        options.forEach((option, index) => {
            // Draw radio circle
            doc.circle(x + 10, currentY, 6);

            // Fill if selected
            if (includeData && formData && formData[element.Id] === option) {
                doc.circle(x + 10, currentY, 3, 'F');
            }

            // Add option text
            doc.setFontSize(12);
            doc.text(option, x + 25, currentY + 4);

            currentY += spacing;
        });
    },

    renderDropdown: function (doc, element, x, y, width, height, includeData, formData) {
        // Draw border
        doc.setDrawColor(150);
        doc.rect(x, y, width, height);

        // Add label
        if (element.Label) {
            doc.setFontSize(12);
            doc.text(element.Label, x, y - 5);
        }

        // Add selected value or placeholder
        if (includeData && formData && formData[element.Id]) {
            doc.setFontSize(14);
            doc.text(formData[element.Id], x + 5, y + height / 2 + 5);
        } else if (element.Placeholder) {
            doc.setTextColor(150);
            doc.setFontSize(12);
            doc.text(element.Placeholder, x + 5, y + height / 2 + 5);
            doc.setTextColor(0);
        }

        // Draw dropdown arrow
        const arrowX = x + width - 20;
        const arrowY = y + height / 2;
        doc.line(arrowX, arrowY - 3, arrowX + 5, arrowY + 3);
        doc.line(arrowX + 5, arrowY + 3, arrowX + 10, arrowY - 3);
    },

    renderDatePicker: function (doc, element, x, y, width, height, includeData, formData) {
        // Similar to text input but with date icon
        this.renderTextInput(doc, element, x, y, width, height, includeData, formData);

        // Draw calendar icon
        const iconX = x + width - 25;
        const iconY = y + height / 2 - 8;
        doc.setDrawColor(100);
        doc.rect(iconX, iconY, 16, 16);
        doc.setFontSize(10);
        doc.text('📅', iconX + 2, iconY + 12);
    },

    renderImage: async function (doc, element, x, y, width, height) {
        if (element.ImageUrl) {
            try {
                // If it's a data URI (base64), we can embed it directly
                if (element.ImageUrl.startsWith('data:image')) {
                    doc.addImage(element.ImageUrl, 'PNG', x, y, width, height);
                } else {
                    // For external URLs, draw placeholder
                    doc.setDrawColor(200);
                    doc.rect(x, y, width, height);
                    doc.setFontSize(10);
                    doc.setTextColor(150);
                    doc.text('[Image]', x + width / 2 - 15, y + height / 2);
                    doc.setTextColor(0);
                }
            } catch (error) {
                console.warn('Error adding image to PDF:', error);
                // Draw placeholder on error
                doc.setDrawColor(200);
                doc.rect(x, y, width, height);
            }
        } else {
            // Draw placeholder
            doc.setDrawColor(200);
            doc.rect(x, y, width, height);
            doc.setFontSize(10);
            doc.setTextColor(150);
            doc.text('[Image Placeholder]', x + 10, y + height / 2);
            doc.setTextColor(0);
        }
    },

    renderSignature: function (doc, element, x, y, width, height, includeData, formData) {
        // Draw signature line
        doc.setDrawColor(0);
        doc.line(x, y + height - 5, x + width, y + height - 5);

        // Add label
        if (element.Label) {
            doc.setFontSize(12);
            doc.text(element.Label, x, y - 5);
        }

        // If signed (has signature data), add it
        if (includeData && element.SignatureData) {
            try {
                doc.addImage(element.SignatureData, 'PNG', x, y, width, height - 10);
            } catch (error) {
                console.warn('Error adding signature:', error);
            }
        }
    },

    renderTable: function (doc, element, x, y, width, height) {
        const rows = element.Rows || 3;
        const cols = element.Columns || 3;
        const cellWidth = width / cols;
        const cellHeight = height / rows;

        // Draw table grid
        doc.setDrawColor(0);

        // Vertical lines
        for (let i = 0; i <= cols; i++) {
            doc.line(x + i * cellWidth, y, x + i * cellWidth, y + height);
        }

        // Horizontal lines
        for (let i = 0; i <= rows; i++) {
            doc.line(x, y + i * cellHeight, x + width, y + i * cellHeight);
        }

        // Add headers if they exist
        if (element.ShowHeaders && element.Headers) {
            doc.setFont(undefined, 'bold');
            doc.setFontSize(10);
            element.Headers.forEach((header, col) => {
                doc.text(header || '', x + col * cellWidth + 5, y + 15);
            });
            doc.setFont(undefined, 'normal');
        }

        // Add cell data if exists
        if (element.CellData) {
            doc.setFontSize(10);
            element.CellData.forEach((row, rowIndex) => {
                row.forEach((cell, colIndex) => {
                    const startRow = element.ShowHeaders ? rowIndex + 1 : rowIndex;
                    doc.text(cell || '', x + colIndex * cellWidth + 5, y + startRow * cellHeight + 15);
                });
            });
        }
    },

    renderDivider: function (doc, element, x, y, width, height) {
        doc.setDrawColor(element.Properties?.BorderColor || '#000000');
        doc.setLineWidth(element.Properties?.BorderWidth || 1);

        // Draw horizontal line in the middle
        doc.line(x, y + height / 2, x + width, y + height / 2);

        doc.setLineWidth(1); // Reset
    },

    renderFileUpload: function (doc, element, x, y, width, height) {
        // Draw border
        doc.setDrawColor(150);
        doc.setLineDash([5, 5]);
        doc.rect(x, y, width, height);
        doc.setLineDash([]);

        // Add label
        if (element.Label) {
            doc.setFontSize(12);
            doc.text(element.Label, x, y - 5);
        }

        // Add button text
        doc.setFontSize(12);
        doc.setTextColor(100);
        const buttonText = element.ButtonText || 'Choose File';
        doc.text(buttonText, x + width / 2 - 20, y + height / 2 + 5);
        doc.setTextColor(0);
    }
};

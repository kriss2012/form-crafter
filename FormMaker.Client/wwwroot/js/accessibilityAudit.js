// Accessibility Audit using axe-core
window.accessibilityAudit = {
    // Run axe-core audit on the entire page
    runAudit: async function () {
        try {
            if (typeof axe === 'undefined') {
                console.error('axe-core library not loaded');
                return {
                    success: false,
                    error: 'axe-core library not loaded'
                };
            }

            // Configure axe to test against WCAG 2.1 AA
            const results = await axe.run(document, {
                runOnly: {
                    type: 'tag',
                    values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']
                }
            });

            // Format results for easier consumption
            const formattedResults = {
                success: true,
                timestamp: new Date().toISOString(),
                url: window.location.href,
                violations: results.violations.map(violation => ({
                    id: violation.id,
                    impact: violation.impact,
                    description: violation.description,
                    help: violation.help,
                    helpUrl: violation.helpUrl,
                    tags: violation.tags,
                    nodes: violation.nodes.map(node => ({
                        html: node.html,
                        target: node.target,
                        failureSummary: node.failureSummary,
                        impact: node.impact
                    }))
                })),
                passes: results.passes.length,
                incomplete: results.incomplete.map(item => ({
                    id: item.id,
                    impact: item.impact,
                    description: item.description,
                    help: item.help,
                    nodes: item.nodes.length
                })),
                inapplicable: results.inapplicable.length
            };

            console.log('Accessibility Audit Results:', formattedResults);
            return formattedResults;
        } catch (error) {
            console.error('Error running accessibility audit:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    // Run audit on a specific element
    runAuditOnElement: async function (selector) {
        try {
            if (typeof axe === 'undefined') {
                console.error('axe-core library not loaded');
                return {
                    success: false,
                    error: 'axe-core library not loaded'
                };
            }

            const element = document.querySelector(selector);
            if (!element) {
                return {
                    success: false,
                    error: `Element not found: ${selector}`
                };
            }

            const results = await axe.run(element, {
                runOnly: {
                    type: 'tag',
                    values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']
                }
            });

            const formattedResults = {
                success: true,
                timestamp: new Date().toISOString(),
                selector: selector,
                violations: results.violations.map(violation => ({
                    id: violation.id,
                    impact: violation.impact,
                    description: violation.description,
                    help: violation.help,
                    helpUrl: violation.helpUrl,
                    tags: violation.tags,
                    nodes: violation.nodes.map(node => ({
                        html: node.html,
                        target: node.target,
                        failureSummary: node.failureSummary
                    }))
                })),
                passes: results.passes.length,
                incomplete: results.incomplete.length,
                inapplicable: results.inapplicable.length
            };

            console.log('Accessibility Audit Results (Element):', formattedResults);
            return formattedResults;
        } catch (error) {
            console.error('Error running accessibility audit on element:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    // Generate a summary report
    generateSummary: function (results) {
        if (!results.success) {
            return `Audit failed: ${results.error}`;
        }

        const violationsByImpact = {
            critical: results.violations.filter(v => v.impact === 'critical').length,
            serious: results.violations.filter(v => v.impact === 'serious').length,
            moderate: results.violations.filter(v => v.impact === 'moderate').length,
            minor: results.violations.filter(v => v.impact === 'minor').length
        };

        let summary = `
=== Accessibility Audit Summary ===
Timestamp: ${results.timestamp}
URL: ${results.url || results.selector}

Violations: ${results.violations.length}
  - Critical: ${violationsByImpact.critical}
  - Serious: ${violationsByImpact.serious}
  - Moderate: ${violationsByImpact.moderate}
  - Minor: ${violationsByImpact.minor}

Passes: ${results.passes}
Incomplete: ${results.incomplete.length}
Inapplicable: ${results.inapplicable}
`;

        if (results.violations.length > 0) {
            summary += '\n=== Violations Detail ===\n';
            results.violations.forEach((violation, index) => {
                summary += `\n${index + 1}. [${violation.impact.toUpperCase()}] ${violation.help}\n`;
                summary += `   ID: ${violation.id}\n`;
                summary += `   Description: ${violation.description}\n`;
                summary += `   Affected nodes: ${violation.nodes.length}\n`;
                summary += `   Help URL: ${violation.helpUrl}\n`;
            });
        }

        return summary;
    }
};

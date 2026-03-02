using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FormMaker.Client.Services;

public class AccessibilityAuditService
{
    private readonly IJSRuntime _jsRuntime;

    public AccessibilityAuditService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<AuditResult> RunAuditAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<AuditResult>("accessibilityAudit.runAudit");
            return result;
        }
        catch (Exception ex)
        {
            return new AuditResult
            {
                Success = false,
                Error = $"Failed to run audit: {ex.Message}"
            };
        }
    }

    public async Task<AuditResult> RunAuditOnElementAsync(string selector)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<AuditResult>("accessibilityAudit.runAuditOnElement", selector);
            return result;
        }
        catch (Exception ex)
        {
            return new AuditResult
            {
                Success = false,
                Error = $"Failed to run audit on element: {ex.Message}"
            };
        }
    }

    public string GenerateReport(AuditResult result)
    {
        if (!result.Success)
        {
            return $"Audit failed: {result.Error}";
        }

        var violationsByImpact = new Dictionary<string, int>
        {
            ["critical"] = result.Violations?.Count(v => v.Impact == "critical") ?? 0,
            ["serious"] = result.Violations?.Count(v => v.Impact == "serious") ?? 0,
            ["moderate"] = result.Violations?.Count(v => v.Impact == "moderate") ?? 0,
            ["minor"] = result.Violations?.Count(v => v.Impact == "minor") ?? 0
        };

        var report = $@"
=== Accessibility Audit Report ===
Timestamp: {result.Timestamp}
URL: {result.Url ?? result.Selector}

Total Violations: {result.Violations?.Count ?? 0}
  - Critical: {violationsByImpact["critical"]}
  - Serious: {violationsByImpact["serious"]}
  - Moderate: {violationsByImpact["moderate"]}
  - Minor: {violationsByImpact["minor"]}

Passes: {result.Passes}
Incomplete: {result.Incomplete?.Count ?? 0}
Inapplicable: {result.Inapplicable}

";

        if (result.Violations != null && result.Violations.Count > 0)
        {
            report += "=== Violations Detail ===\n\n";
            for (int i = 0; i < result.Violations.Count; i++)
            {
                var violation = result.Violations[i];
                report += $"{i + 1}. [{violation.Impact?.ToUpper()}] {violation.Help}\n";
                report += $"   ID: {violation.Id}\n";
                report += $"   Description: {violation.Description}\n";
                report += $"   Affected elements: {violation.Nodes?.Count ?? 0}\n";
                report += $"   WCAG Tags: {string.Join(", ", violation.Tags ?? new List<string>())}\n";
                report += $"   Help: {violation.HelpUrl}\n\n";

                if (violation.Nodes != null && violation.Nodes.Count > 0)
                {
                    report += "   Affected HTML:\n";
                    foreach (var node in violation.Nodes.Take(3)) // Show first 3 nodes
                    {
                        report += $"   - {node.Html}\n";
                        if (node.Target != null && node.Target.Count > 0)
                        {
                            report += $"     Selector: {string.Join(" > ", node.Target)}\n";
                        }
                    }
                    if (violation.Nodes.Count > 3)
                    {
                        report += $"   ... and {violation.Nodes.Count - 3} more\n";
                    }
                    report += "\n";
                }
            }
        }

        if (result.Incomplete != null && result.Incomplete.Count > 0)
        {
            report += "=== Incomplete Checks (Need Manual Review) ===\n\n";
            foreach (var item in result.Incomplete)
            {
                report += $"- [{item.Impact?.ToUpper()}] {item.Help}\n";
                report += $"  ID: {item.Id}\n";
                report += $"  Elements to review: {item.Nodes}\n\n";
            }
        }

        return report;
    }
}

public class AuditResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("selector")]
    public string? Selector { get; set; }

    [JsonPropertyName("violations")]
    public List<Violation>? Violations { get; set; }

    [JsonPropertyName("passes")]
    public int Passes { get; set; }

    [JsonPropertyName("incomplete")]
    public List<IncompleteItem>? Incomplete { get; set; }

    [JsonPropertyName("inapplicable")]
    public int Inapplicable { get; set; }
}

public class Violation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("impact")]
    public string? Impact { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("help")]
    public string? Help { get; set; }

    [JsonPropertyName("helpUrl")]
    public string? HelpUrl { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("nodes")]
    public List<ViolationNode>? Nodes { get; set; }
}

public class ViolationNode
{
    [JsonPropertyName("html")]
    public string? Html { get; set; }

    [JsonPropertyName("target")]
    public List<string>? Target { get; set; }

    [JsonPropertyName("failureSummary")]
    public string? FailureSummary { get; set; }

    [JsonPropertyName("impact")]
    public string? Impact { get; set; }
}

public class IncompleteItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("impact")]
    public string? Impact { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("help")]
    public string? Help { get; set; }

    [JsonPropertyName("nodes")]
    public int Nodes { get; set; }
}

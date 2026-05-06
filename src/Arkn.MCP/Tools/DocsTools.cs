using Arkn.MCP.Docs;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Arkn.MCP.Tools;

[McpServerToolType]
public static class DocsTools
{
    [McpServerTool, Description("Searches Arkn documentation by keyword. Available topics: result, error, iarknjob, iarknlogger, addarknhttp, analyzers, sourcegen, templates.")]
    public static string DocsLookup(
        [Description("Search query or topic name, e.g. 'result', 'error code', 'job registration', 'http client'")] string query)
    {
        return DocsIndex.Lookup(query);
    }
}

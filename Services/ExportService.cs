using System.Text;

namespace ISMSponsor.Services;

public class ExportService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data, string[] headers, Func<T, string[]> rowMapper)
    {
        var sb = new StringBuilder();
        
        // Write headers
        sb.AppendLine(string.Join(",", headers.Select(h => EscapeCsvField(h))));
        
        // Write data rows
        foreach (var item in data)
        {
            var fields = rowMapper(item);
            sb.AppendLine(string.Join(",", fields.Select(f => EscapeCsvField(f))));
        }
        
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
    
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "\"\"";
            
        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        
        return field;
    }
}

using System.Text.Json.Serialization;

namespace Core.Dto;

public class EnvironmentReportDto
{
    public string Student { get; set; } = "";
    public string OsDescription { get; set; } = "";
    public string Architecture { get; set; } = "";
    public string Runtime { get; set; } = "";
    public string DetectedRid { get; set; } = "";
    public string ReportedRid { get; set; } = "";
    public string BaseDirectory { get; set; } = "";
    public string CurrentDirectory { get; set; } = "";
    public string BuildNote { get; set; } = "";
    public string Domain { get; set; } = "";
}

[JsonSerializable(typeof(EnvironmentReportDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}
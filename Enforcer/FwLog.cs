using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enforcer;

public static class Extensions
{
	public static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
	{
		WriteIndented = true,
		AllowTrailingCommas = true,

	};
}
public class FwLog
{
	public FwLog()
	{

	}

	public FwLog(string date, string time, string action, string protocol, string sourceIp, string sourcePort, string destinationIp, string destinationPort)
	{
		Date = date;
		Time = time;
		Action = action;
		Protocol = protocol;
		SourceIp = sourceIp;
		SourcePort = sourcePort;
		DestinationIp = destinationIp;
		DestinationPort = destinationPort;
	}
	public string Date { get; set; } = string.Empty;
	public string Time { get; set; } = string.Empty;
	public string Action { get; set; } = string.Empty;
	public string Protocol { get; set; } = string.Empty;
	public string SourceIp { get; set; } = string.Empty;
	public string SourcePort { get; set; } = string.Empty;
	public string DestinationIp { get; set; } = string.Empty;
	public string DestinationPort { get; set; } = string.Empty;


	[JsonIgnore]
	public bool IsDeny => Action.Equals("Deny", StringComparison.OrdinalIgnoreCase);

	public override string ToString() => JsonSerializer.Serialize(this, Extensions.JsonSerializerOptions);

}

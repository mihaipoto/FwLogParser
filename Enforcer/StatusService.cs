using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enforcer;


public enum EnforcerStatusType
{
	InternalError,
	PolicyError,
	Ok
}


public class StatusService
{
	public EnforcerStatusType Status => (CurrentException, FwLogs.Any(l => l.IsDeny)) switch
	{
		(null, true) => EnforcerStatusType.PolicyError,
		(not null, _) => EnforcerStatusType.InternalError,
		_ => EnforcerStatusType.Ok
	};



	public List<FwLog> FwLogs { get; set; } = [];
	public string? ErrorMessage => CurrentException?.Message;

	[JsonIgnore]
	public Exception? CurrentException { get; set; }
	public override string ToString() => JsonSerializer.Serialize(this, Extensions.JsonSerializerOptions);


}

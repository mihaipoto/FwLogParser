using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FwLogParser;

public enum FwActionEnum
{
	Allow,
	Deny
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

	public override string ToString() => JsonSerializer.Serialize(this);

}

public class FirewallLogMonitorService(ILogger<FirewallLogMonitorService> logger) : IHostedService, IDisposable
{
	private readonly ILogger<FirewallLogMonitorService> _logger = logger;
	private FileStream? _fileStream;
	private StreamReader? _reader;
	private static string _logFilePath = @"C:\FwLogs\pfirewall.log";
	private Timer? _timer;


	public Queue<FwLog> FwLogs { get; set; } = new(1000);

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation($"Monitoring file: {_logFilePath}");
			// Open the log file
			_fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			_fileStream.Seek(0, SeekOrigin.End);
			_reader = new StreamReader(_fileStream);
			_timer = new Timer(async (t) => { await DoWork(cancellationToken); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

		}
		catch (Exception ex)
		{
			_logger.LogError(exception: ex, message: "Error starting service");
			await StopAsync(cancellationToken);
		}

	}

	private async Task DoWork(CancellationToken cancellationToken)
	{
		string? line;
		while ((line = await _reader!.ReadLineAsync(cancellationToken)) is not null)
		{
			var fwLog = ProcessLogEntry(line);
			if (fwLog != null)
			{
				_logger.LogInformation("{Log}", fwLog);
				FwLogs.Enqueue(fwLog);
			}
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Service is stopping...");
		_timer?.Change(Timeout.Infinite, 0);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		_reader?.Close();
		_reader?.Dispose();
		_fileStream?.Close();
		_fileStream?.Dispose();
		_timer?.Dispose();
	}




	private FwLog? ProcessLogEntry(string line)
	{
		// Skip comment lines (starting with #) or empty lines
		if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
		{
			return null;
		}
		// Split the log entry into fields
		string[] fields = line.Split(' ');

		// Ensure there are enough fields to parse
		if (fields.Length < 10)
		{
			_logger.LogWarning($"Malformed log entry: {line}");
			return null;
		}

		// Log and display the parsed information
		//_logger.LogInformation("Date: {date}, Time: {time}, Action: {action}, Protocol: {protocol}, Source: {sourceIP}:{sourcePort}, Destination: {destinationIP}:{destinationPort}",
		//				 fields[0],
		//				 fields[1],
		//				 fields[2],
		//				 fields[3],
		//				 fields[4],
		//				 fields[5],
		//				 fields[6],
		//				 fields[7]);


		return new FwLog(date: fields[0],
						time: fields[1],
						action: fields[2],
						protocol: fields[3],
						sourceIp: fields[4],
						sourcePort: fields[5],
						destinationIp: fields[6],
						destinationPort: fields[7]);

	}
}

namespace Enforcer;

public enum FwActionEnum
{
	Allow,
	Deny
}

public class FirewallLogMonitorService(ILogger<FirewallLogMonitorService> logger,
	StatusService statusService) : IHostedService, IDisposable
{
	private readonly ILogger<FirewallLogMonitorService> _logger = logger;
	private readonly StatusService _statusService = statusService;
	private FileStream? _fileStream;
	private StreamReader? _reader;
	private static string _logFilePath = @"C:\FwLogs\pfirewall.log";
	private Timer? _timer;




	public async Task StartAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation($"Monitoring file: {_logFilePath}");
			// Open the log file
			_fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			_fileStream.Seek(0, SeekOrigin.End);
			_reader = new StreamReader(_fileStream);
			_timer = new Timer(async (t) => { await DoWork(cancellationToken); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

		}
		catch (Exception ex)
		{
			_logger.LogError(exception: ex, message: "Error starting service");
			_statusService.CurrentException = ex;
			await StopAsync(cancellationToken);
		}

	}

	private async Task DoWork(CancellationToken cancellationToken)
	{
		try
		{
			string? line;
			while ((line = await _reader!.ReadLineAsync(cancellationToken)) is not null)
			{
				var fwLog = ProcessLogEntry(line);
				if (fwLog != null)
				{
					_logger.LogInformation("{Log}", fwLog);
					_statusService.FwLogs.Add(fwLog);
				}
			}
		}
		catch (Exception ex)
		{
			_statusService.CurrentException = ex;
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

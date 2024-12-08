using FwLogParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;


//var isService = !(Debugger.IsAttached || args.Contains("--console"));

//var builder = Host.CreateDefaultBuilder(args);
//if (isService)
//{
//	builder.UseWindowsService();

//}
//else
//{
//	builder.ConfigureLogging(logging =>
//	{
//		logging.ClearProviders();
//		logging.AddConsole();
//	});
//}

//builder.ConfigureServices(services =>
//{
//	services.AddHostedService<FirewallLogMonitorService>();
//});




//Console.WriteLine("Starting the application...");
//await builder.Build().RunAsync();
//Console.WriteLine("Application stopped.");

Log.Logger = new LoggerConfiguration()
		.WriteTo.Console() // Console logging for debugging
		.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // File-based logs
		.CreateLogger();



try
{
	var builder = Host.CreateDefaultBuilder(args)
		.UseSerilog() // Use Serilog
		.UseWindowsService()
		.ConfigureLogging(logging =>
		{


		})
		.ConfigureServices(services =>
		{
			services.AddHostedService<FirewallLogMonitorService>();
		});



	await builder.Build().RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "The application failed to start.");
}
finally
{
	Log.CloseAndFlush();
}
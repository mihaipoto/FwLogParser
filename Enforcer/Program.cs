using Enforcer;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<FirewallLogMonitorService>();
builder.Services.AddSingleton<StatusService>();
builder.Services.AddWindowsService();

var app = builder.Build();

app.MapGet("/status", ([FromServices] StatusService statusService)
	=> statusService.ToString());


app.Urls.Add("http://localhost:4001");

app.Run();



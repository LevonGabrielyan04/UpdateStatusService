using Microsoft.Extensions.Configuration;
using UpdateStatusService;
using Microsoft.Extensions.Hosting.WindowsServices;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() 
                                     ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);
Routes.TTL = builder.Configuration.GetValue<int>("TTL");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseWindowsService();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("/updatestatuses", Routes.UpdateStatuses)
.WithName("UpdateStatuses")
.WithOpenApi();
app.MapGet("/updatestatusesimidently", Routes.UpdateStatusImidiantly)
.WithName("UpdateStatusesImidently")
.WithOpenApi();
app.Run();
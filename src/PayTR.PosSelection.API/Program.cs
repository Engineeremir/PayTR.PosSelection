using Hangfire;
using PayTR.PosSelection.API.SeedWork.ExceptionHandling;
using PayTR.PosSelection.Application;
using PayTR.PosSelection.Infrastructure.EFCore;
using PayTR.PosSelection.Infrastructure.Schduler;
using PayTR.PosSelection.Shared;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.EnableDiscardEvents", false);

var builder = WebApplication.CreateBuilder(args);

#region Application settings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

//var vaultService = new VaultService(builder.Configuration);
//vaultService.SetSecrets();

builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);

#endregion
builder.Services.AddControllers();

builder.Services.AddInfrastructureEfCore(builder.Configuration);
builder.Services.AddShared();
builder.Services.AddApplication(builder.Configuration);

#region Problem Details
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<FluentValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandnler>();
builder.Services.AddProblemDetails();
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfireSchduler(builder.Configuration);

var app = builder.Build();
app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireSchduler();
app.UseHangfireDashboard();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

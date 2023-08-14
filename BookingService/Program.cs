using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

#region setup OpenTelemetry provider

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(cfg =>
                //collect all requests except the requests to the swagger UI
                cfg.Filter = httpContext => httpContext.Request.Path.Value != null
                            && !httpContext.Request.Path.Value.Contains("swagger")
                            && !httpContext.Request.Path.Value.Contains("_framework")
            )
            .AddConsoleExporter()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(conf =>
            {
                conf.Endpoint = new Uri(builder.Configuration.GetValue<string>("OtlpExporterGrpcUri") ?? "http://localhost:4317");
                conf.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
                conf.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            })
        );

#endregion

#region Configure Serilog

builder.Host.UseSerilog((hostContext, services, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(builder.Configuration);
});

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSerilogRequestLogging();
app.UseHttpLogging();

ActivitySource MyActivitySource = new("Quack.BookService");

app.MapGet("/bookWithDetailTelemetry", async (IConfiguration conf, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("G");
    logger.LogInformation("Start bookWithDetailTelemetry");
    var carRentalServiceUri = conf.GetValue<string>("CarRentalServiceUri");
    var client = new HttpClient();
    using (MyActivitySource.StartActivity("CarRentalService"))
    {
        await client.GetStringAsync(carRentalServiceUri + "/rentCarWithDetailTelemetry");
    }
    using (MyActivitySource.StartActivity("Book"))
    {
        await Task.Delay(200);
    }
    return "Book OK";
});

app.MapGet("/book", async (IConfiguration conf, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("G");
    logger.LogInformation("Start Book");
    var carRentalServiceUri = conf.GetValue<string>("CarRentalServiceUri");
    var client = new HttpClient();
    await client.GetStringAsync(carRentalServiceUri + "/rentCar");
    await Task.Delay(200);
    return "Book OK";
});

app.Run();

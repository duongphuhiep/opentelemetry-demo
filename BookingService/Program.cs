using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// download https://www.jaegertracing.io/download/
// run the server and activate the otlp collector
// .\jaeger-all-in-one.exe --collector.otlp.enabled=true --collector.otlp.grpc.host-port=:4317  --collector.otlp.http.host-port=:4318

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
                conf.Endpoint = new Uri(builder.Configuration.GetValue<string>("OtlpExporterGrpcUri"));
                conf.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
                conf.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            })
        );

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

ActivitySource MyActivitySource = new("Quack.BookService");

app.MapGet("/bookWithDetailTelemetry", async (IConfiguration conf) =>
{
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

app.MapGet("/book", async (IConfiguration conf) =>
{
    var carRentalServiceUri = conf.GetValue<string>("CarRentalServiceUri");
    var client = new HttpClient();
    await client.GetStringAsync(carRentalServiceUri + "/rentCar");
    await Task.Delay(200);
    return "Book OK";
});

app.Run();

# Tracing sample with Open Telemetry

## 0. Run the jaeger-all-in-one server

[Download here](https://www.jaegertracing.io/download/)

```
.\jaeger-all-in-one --collector.otlp.enabled=true --collector.otlp.grpc.host-port=:4317  --collector.otlp.http.host-port=:4318
```

Note: the command line means that

  * `--collector.otlp.enabled=true`: the jaeger server is enabled to collect instruments in the Open Telemetry Protocol (OTLP) => **this format is more standard than the jaeger format or zipkins format**
  * `--collector.otlp.grpc.host-port=:4317` applications can send instruments via GRPC `localhost:4317`
  * `--collector.otlp.http.host-port=:4318` applications can send instruments via HTTP `localhost:4318`

The jaeger UI `http://localhost:16686/`

## 1. Run the micro-service

```
dotnet run
```

Note: All the codes is in `Program.cs`. 
  * Our micro-service will send instruments to GRPC `localhost:4317`.
  * HttpClient usage is automaticly instrumented.
    * It means that all Http call to outside services is traced
    * In our example we also call a service which redirect our call to other service to get the response and we are able to trace all of these communications.
  * in C# we don't directly use the [Trace API](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/api.md) to build Span and Tracer, instead [we use `System.Diagnostics.Activity` and "Tag" to create the corresponding Span and its Attribute](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/docs/trace/getting-started/README.md#opentelemetry-net-and-relation-with-net-activity-api). (We will have to use the Trace API to instrument Non .NET micro-service.)

## 2. Make some request (via Postman or Swagger)

```
GET /createPayment
GET /createPaymentWithDetailTelemetry
```

## 3. Go to the jaeger UI `http://localhost:16686/`

 * Trace for `/createPayment`

![image](https://user-images.githubusercontent.com/1638594/179763257-71208eda-6a15-4c10-b52d-b7aae576d858.png)

 * Trace for `/createPaymentWithDetailTelemetry`

![image](https://user-images.githubusercontent.com/1638594/179763115-5e715940-be5c-41b0-8bdb-e7c621d99d59.png)


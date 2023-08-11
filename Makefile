all: Jaeger CarRental Booking

CarRental: ./CarRentalService/CarRentalService.csproj
	dotnet run --project ./CarRentalService/CarRentalService.csproj 

Booking: ./BookingService/BookingService.csproj
	dotnet run --project ./BookingService/BookingService.csproj

Jaeger:	./jaeger-all-in-one.exe
	./jaeger-all-in-one --collector.otlp.enabled=true --collector.otlp.grpc.host-port=:4317  --collector.otlp.http.host-port=:4318
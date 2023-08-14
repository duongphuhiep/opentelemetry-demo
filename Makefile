all: test

test: Infra CarRental Booking
	curl https://localhost:7217/book

CarRental: ./CarRentalService/CarRentalService.csproj
	dotnet run --project ./CarRentalService/CarRentalService.csproj 

Booking: ./BookingService/BookingService.csproj
	dotnet run --project ./BookingService/BookingService.csproj

Infra: ./docker-compose.yaml
	docker compose up -d
	
clean:
	docker compose down
# Slot Booking API

The Slot Booking API is a C# application designed to manage and book slots for various facilities. It allows users to view available slots based on specific date and book them accordingly. The API is configured to handle slots based on a weekly schedule. Ensure that the date inputs are correct and follow the expected format. This document provides instructions on how to set up and run the application.

# What is used

- [ASP NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- [Docker](https://www.docker.com/) for running things locally 
- [FluentValidation](https://fluentvalidation.net/) for building strongly-typed validation rules
- Unit and integration tests using [Moq](https://github.com/moq/moq4) and [nUnit](https://nunit.org/)
- [Swagger UI](https://github.com/swagger-api/swagger-ui)
- [ASP NET Core Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=linux) for secret managing locally

# How to run locally using docker

0. Ensure Docker is installed.
1. Build the Docker image: open a terminal and navigate to the root directory of the project. Run the following command to build the Docker image:
```sh
docker build -t slotbooking-api .
```
2. Run the Docker container:
> [!NOTE]
> Provide your credentials instead of USERNAME and PASSWORD for accessing external service
To run your application, execute:
```sh
docker run -e SlotService__Username=USERNAME -e SlotService__Password=PASSWORD -p 8080:8080 slotbooking-api
```
3. You can access swagger at http://localhost:8080/swagger/index.html

# How to run locally using CLI



0. Ensure .NET 8 is installed.
1. Set up secrets:
> [!NOTE]
> Provide your credentials instead of USERNAME and PASSWORD for accessing external service
Open directory yourpath/SlotBookingAPI/SlotBooking.API
```sh
dotnet user-secrets init 
dotnet user-secrets set "SlotService:Password" "PASSWORD"
dotnet user-secrets set "SlotService:Username" "USERNAME"
```
2. Run the following `dotnet` commands:

```sh
dotnet build
dotnet run 
```
3. Open browser to: `https://localhost:7088/swagger/index.html`.
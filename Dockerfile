# Use the ASP.NET base image for the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Expose the ports your application uses
EXPOSE 8080
EXPOSE 443

# Use the SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Copy csproj files and restore any dependencies (via NuGet)
COPY ["SlotBooking.API/SlotBooking.API.csproj", "SlotBooking.API/"]
COPY ["SlotBooking.Domain/SlotBooking.Domain.csproj", "SlotBooking.Domain/"]
COPY ["SlotBooking.Data/SlotBooking.Data.csproj", "SlotBooking.Data/"]
RUN dotnet restore "SlotBooking.API/SlotBooking.API.csproj"
# Copy the rest of your source code
COPY . .
WORKDIR "/src/SlotBooking.API"
RUN dotnet build "SlotBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SlotBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Set the entry point for the container
ENTRYPOINT ["dotnet", "SlotBooking.API.dll"]
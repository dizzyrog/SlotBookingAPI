FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SlotBooking.API/SlotBooking.API.csproj", "SlotBooking.API/"]
COPY ["SlotBooking.Domain/SlotBooking.Domain.csproj", "SlotBooking.Domain/"]
COPY ["SlotBooking.Data/SlotBooking.Data.csproj", "SlotBooking.Data/"]
RUN dotnet restore "SlotBooking.API/SlotBooking.API.csproj"

COPY . .
WORKDIR "/src/SlotBooking.API"
RUN dotnet build "SlotBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "SlotBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SlotBooking.API.dll"]
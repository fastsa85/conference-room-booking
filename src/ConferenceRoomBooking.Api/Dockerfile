FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ConferenceRoomBooking.Api/ConferenceRoomBooking.Api.csproj", "src/ConferenceRoomBooking.Api/"]
COPY ["src/ConferenceRoomBooking.Application/ConferenceRoomBooking.Application.csproj", "src/ConferenceRoomBooking.Application/"]
COPY ["src/ConferenceRoomBooking.Domain/ConferenceRoomBooking.Domain.csproj", "src/ConferenceRoomBooking.Domain/"]
COPY ["src/ConferenceRoomBooking.Infrastructure/ConferenceRoomBooking.Infrastructure.csproj", "src/ConferenceRoomBooking.Infrastructure/"]

RUN dotnet restore "src/ConferenceRoomBooking.Api/ConferenceRoomBooking.Api.csproj"

COPY . .

RUN dotnet publish \
    "src/ConferenceRoomBooking.Api/ConferenceRoomBooking.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConferenceRoomBooking.Api.dll"]
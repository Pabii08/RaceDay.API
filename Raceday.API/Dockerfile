FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Raceday.API/Raceday.API.csproj", "Raceday.API/"]
RUN dotnet restore "Raceday.API/Raceday.API.csproj"
COPY . .
WORKDIR "/src/Raceday.API"
RUN dotnet build "Raceday.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Raceday.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Raceday.API.dll"]
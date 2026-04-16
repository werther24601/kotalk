FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PhysOn.sln", "./"]
COPY ["global.json", "./"]
COPY ["src/PhysOn.Api/PhysOn.Api.csproj", "src/PhysOn.Api/"]
COPY ["src/PhysOn.Application/PhysOn.Application.csproj", "src/PhysOn.Application/"]
COPY ["src/PhysOn.Contracts/PhysOn.Contracts.csproj", "src/PhysOn.Contracts/"]
COPY ["src/PhysOn.Domain/PhysOn.Domain.csproj", "src/PhysOn.Domain/"]
COPY ["src/PhysOn.Infrastructure/PhysOn.Infrastructure.csproj", "src/PhysOn.Infrastructure/"]

RUN dotnet restore "src/PhysOn.Api/PhysOn.Api.csproj"

COPY . .
RUN dotnet publish "src/PhysOn.Api/PhysOn.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PhysOn.Api.dll"]

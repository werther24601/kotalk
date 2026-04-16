FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PhysOn.sln", "./"]
COPY ["global.json", "./"]
COPY ["src/PhysOn.Worker/PhysOn.Worker.csproj", "src/PhysOn.Worker/"]
COPY ["src/PhysOn.Application/PhysOn.Application.csproj", "src/PhysOn.Application/"]
COPY ["src/PhysOn.Contracts/PhysOn.Contracts.csproj", "src/PhysOn.Contracts/"]
COPY ["src/PhysOn.Domain/PhysOn.Domain.csproj", "src/PhysOn.Domain/"]
COPY ["src/PhysOn.Infrastructure/PhysOn.Infrastructure.csproj", "src/PhysOn.Infrastructure/"]

RUN dotnet restore "src/PhysOn.Worker/PhysOn.Worker.csproj"

COPY . .
RUN dotnet publish "src/PhysOn.Worker/PhysOn.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PhysOn.Worker.dll"]

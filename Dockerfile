FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DotNetKafkaRabbitMQExample/DotNetKafkaRabbitMQExample.csproj", "DotNetKafkaRabbitMQExample/"]
COPY ["DotNetKafkaRabbitMQExample.Tests/DotNetKafkaRabbitMQExample.Tests.csproj", "DotNetKafkaRabbitMQExample.Tests/"]

RUN dotnet restore "DotNetKafkaRabbitMQExample/DotNetKafkaRabbitMQExample.csproj"

COPY . .

RUN dotnet publish "DotNetKafkaRabbitMQExample/DotNetKafkaRabbitMQExample.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/wwwroot

EXPOSE 8080

ENTRYPOINT ["dotnet", "DotNetKafkaRabbitMQExample.dll"]
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CsvProcessing.Api/CsvProcessing.Api.csproj src/CsvProcessing.Api/
COPY src/CsvProcessing.Application/CsvProcessing.Application.csproj src/CsvProcessing.Application/
COPY tests/CsvProcessing.Api.Tests/CsvProcessing.Api.Tests.csproj tests/CsvProcessing.Api.Tests/
RUN dotnet restore tests/CsvProcessing.Api.Tests/CsvProcessing.Api.Tests.csproj

COPY . .
RUN dotnet test tests/CsvProcessing.Api.Tests/CsvProcessing.Api.Tests.csproj \
    --configuration Release --no-restore
RUN dotnet publish src/CsvProcessing.Api/CsvProcessing.Api.csproj \
    --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "CsvProcessing.Api.dll"]
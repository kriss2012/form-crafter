# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY FormMaker.Api/FormMaker.Api.csproj FormMaker.Api/
COPY FormMaker.Shared/FormMaker.Shared.csproj FormMaker.Shared/
RUN dotnet restore "FormMaker.Api/FormMaker.Api.csproj"

# Copy everything else and build
COPY FormMaker.Api/ FormMaker.Api/
COPY FormMaker.Shared/ FormMaker.Shared/
WORKDIR /src/FormMaker.Api
RUN dotnet publish "FormMaker.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
WORKDIR /home/site/wwwroot

# Copy the published files from build stage
COPY --from=build /app/publish .

# Set environment variables for Azure Functions
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

# Expose port 80
EXPOSE 80

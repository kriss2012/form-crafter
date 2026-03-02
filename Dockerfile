# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire workspace to get all csproj and shared folders
COPY . .

# Restore and publish
RUN dotnet restore "FormMaker.Api/FormMaker.Api.csproj"
RUN dotnet publish "FormMaker.Api/FormMaker.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0
WORKDIR /home/site/wwwroot

# Copy from build
COPY --from=build /app/publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

EXPOSE 80

ENTRYPOINT ["/azure-functions-host/Microsoft.Azure.WebJobs.Script.WebHost"]

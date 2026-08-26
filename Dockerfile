FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY TasksApi.csproj .
RUN dotnet restore TasksApi.csproj
COPY . .
RUN dotnet publish TasksApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TasksApi.dll"]

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY RSPeer.Api/RSPeer.Api.csproj RSPeer.Api/
COPY RSPeer.Application/RSPeer.Application.csproj RSPeer.Application/
COPY RSPeer.Domain/RSPeer.Domain.csproj RSPeer.Domain/
COPY RSPeer.Persistence/RSPeer.Persistence.csproj RSPeer.Persistence/
COPY RSPeer.ForumsMigration/RSPeer.ForumsMigration.csproj RSPeer.ForumsMigration/
COPY RSPeer.Infrastructure/RSPeer.Infrastructure.csproj RSPeer.Infrastructure/
COPY RSPeer.Common/RSPeer.Common.csproj RSPeer.Common/
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "RSPeer.Api.dll"]

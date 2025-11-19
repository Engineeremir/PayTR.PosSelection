# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PayTR.PosSelection.API/PayTR.PosSelection.API.csproj", "src/PayTR.PosSelection.API/"]
COPY ["src/Infrastructure/PayTR.PosSelection.Infrastructure.EFCore/PayTR.PosSelection.Infrastructure.EFCore.csproj", "src/Infrastructure/PayTR.PosSelection.Infrastructure.EFCore/"]
COPY ["src/PayTR.PosSelection.Domain/PayTR.PosSelection.Domain.csproj", "src/PayTR.PosSelection.Domain/"]
COPY ["src/PayTR.PosSelection.Shared/PayTR.PosSelection.Shared.csproj", "src/PayTR.PosSelection.Shared/"]
COPY ["src/Infrastructure/PayTR.PosSelection.Infrastructure.Scheduler/PayTR.PosSelection.Infrastructure.Scheduler.csproj", "src/Infrastructure/PayTR.PosSelection.Infrastructure.Scheduler/"]
COPY ["src/PayTR.PosSelection.Application/PayTR.PosSelection.Application.csproj", "src/PayTR.PosSelection.Application/"]
RUN dotnet restore "./src/PayTR.PosSelection.API/PayTR.PosSelection.API.csproj"
COPY . .
WORKDIR "/src/src/PayTR.PosSelection.API"
RUN dotnet build "./PayTR.PosSelection.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PayTR.PosSelection.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PayTR.PosSelection.API.dll"]
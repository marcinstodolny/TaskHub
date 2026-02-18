# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Web/TaskHub.Web/TaskHub.Web.csproj", "src/Web/TaskHub.Web/"]
COPY ["src/API/TaskHub.Api/TaskHub.Api.csproj", "src/API/TaskHub.Api/"]
COPY ["src/Application/TaskHub.Application/TaskHub.Application.csproj", "src/Application/TaskHub.Application/"]
COPY ["src/Domains/TaskHub.Domain/TaskHub.Domain.csproj", "src/Domains/TaskHub.Domain/"]
COPY ["src/Infrastructure/TaskHub.Infrastructure/TaskHub.Infrastructure.csproj", "src/Infrastructure/TaskHub.Infrastructure/"]
RUN dotnet restore "./src/Web/TaskHub.Web/TaskHub.Web.csproj"
COPY . .
WORKDIR "/src/src/Web/TaskHub.Web"
RUN dotnet build "./TaskHub.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskHub.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskHub.Web.dll"]
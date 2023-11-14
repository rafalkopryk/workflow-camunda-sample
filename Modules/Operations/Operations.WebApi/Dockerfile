#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Modules/Operations/Operations.WebApi/Operations.WebApi.csproj", "Modules/Operations/Operations.WebApi/"]
COPY ["Modules/Operations/Operations.Application/Operations.Application.csproj", "Modules/Operations/Operations.Application/"]
COPY ["Common/Common.Application/Common.Application.csproj", "Common/Common.Application/"]
COPY ["Camunda/Camunda.Client/Camunda.Client.csproj", "Camunda/Camunda.Client/"]
COPY ["Common/Common.Kafka/Common.Kafka.csproj", "Common/Common.Kafka/"]
RUN dotnet restore "./Modules/Operations/Operations.WebApi/./Operations.WebApi.csproj"
COPY . .
WORKDIR "/src/Modules/Operations/Operations.WebApi"
RUN dotnet build "./Operations.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Operations.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Operations.WebApi.dll"]
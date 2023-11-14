#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Modules/BackOffice/Operations.BackOffice/Server/Operations.BackOffice.Server.csproj", "Modules/BackOffice/Operations.BackOffice/Server/"]
COPY ["Modules/BackOffice/Operations.BackOffice/Client/Operations.BackOffice.Client.csproj", "Modules/BackOffice/Operations.BackOffice/Client/"]
COPY ["Modules/BackOffice/Operations.BackOffice/Shared/Operations.BackOffice.Shared.csproj", "Modules/BackOffice/Operations.BackOffice/Shared/"]
RUN dotnet restore "./Modules/BackOffice/Operations.BackOffice/Server/./Operations.BackOffice.Server.csproj"
COPY . .
WORKDIR "/src/Modules/BackOffice/Operations.BackOffice/Server"
RUN dotnet build "./Operations.BackOffice.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Operations.BackOffice.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Operations.BackOffice.Server.dll"]
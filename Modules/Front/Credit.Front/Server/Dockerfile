#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Modules/Front/Credit.Front/Server/Credit.Front.Server.csproj", "Modules/Front/Credit.Front/Server/"]
COPY ["Modules/Front/Credit.Front/Client/Credit.Front.Client.csproj", "Modules/Front/Credit.Front/Client/"]
COPY ["Modules/Front/Credit.Front/Shared/Credit.Front.Shared.csproj", "Modules/Front/Credit.Front/Shared/"]
RUN dotnet restore "./Modules/Front/Credit.Front/Server/./Credit.Front.Server.csproj"
COPY . .
WORKDIR "/src/Modules/Front/Credit.Front/Server"
RUN dotnet build "./Credit.Front.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Credit.Front.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Credit.Front.Server.dll"]
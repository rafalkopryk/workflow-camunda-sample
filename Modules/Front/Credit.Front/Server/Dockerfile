# Zobacz https://aka.ms/customizecontainer, aby dowiedzieć się, jak dostosować kontener debugowania i jak program Visual Studio używa tego pliku Dockerfile do kompilowania obrazów w celu szybszego debugowania.

# Ten etap jest używany podczas uruchamiania z programu VS w trybie szybkim (domyślnie dla konfiguracji debugowania)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Ten etap służy do kompilowania projektu usługi
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Modules/Front/Credit.Front/Server/Credit.Front.Server.csproj", "Modules/Front/Credit.Front/Server/"]
COPY ["Modules/Front/Credit.Front/Client/Credit.Front.Client.csproj", "Modules/Front/Credit.Front/Client/"]
COPY ["Modules/Front/Credit.Front/Shared/Credit.Front.Shared.csproj", "Modules/Front/Credit.Front/Shared/"]
RUN dotnet restore "./Modules/Front/Credit.Front/Server/Credit.Front.Server.csproj"
COPY . .
WORKDIR "/src/Modules/Front/Credit.Front/Server"
RUN dotnet build "./Credit.Front.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Ten etap służy do publikowania projektu usługi do skopiowania do etapu końcowego
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Credit.Front.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Ten etap jest używany w środowisku produkcyjnym lub w przypadku uruchamiania z programu VS w trybie regularnym (domyślnie, gdy nie jest używana konfiguracja debugowania)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Credit.Front.Server.dll"]
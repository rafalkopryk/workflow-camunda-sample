#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Modules/BackOffice/Operations.BackOffice/Server/Operations.BackOffice.Server.csproj", "Modules/BackOffice/Operations.BackOffice/Server/"]
COPY ["Modules/BackOffice/Operations.BackOffice/Client/Operations.BackOffice.Client.csproj", "Modules/BackOffice/Operations.BackOffice/Client/"]
COPY ["Modules/BackOffice/Operations.BackOffice/Shared/Operations.BackOffice.Shared.csproj", "Modules/BackOffice/Operations.BackOffice/Shared/"]
RUN dotnet restore "Modules/BackOffice/Operations.BackOffice/Server/Operations.BackOffice.Server.csproj"
COPY . .
WORKDIR "/src/Modules/BackOffice/Operations.BackOffice/Server"
RUN dotnet build "Operations.BackOffice.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Operations.BackOffice.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Operations.BackOffice.Server.dll"]
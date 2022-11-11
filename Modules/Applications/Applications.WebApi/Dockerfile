#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Modules/Applications/Applications.WebApi/Applications.WebApi.csproj", "Modules/Applications/Applications.WebApi/"]
RUN dotnet restore "Modules/Applications/Applications.WebApi/Applications.WebApi.csproj"
COPY . .
WORKDIR "/src/Modules/Applications/Applications.WebApi"
RUN dotnet build "Applications.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Applications.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Applications.WebApi.dll"]
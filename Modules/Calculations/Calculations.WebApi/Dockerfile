#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Modules/Calculations/Calculations.WebApi/Calculations.WebApi.csproj", "Modules/Calculations/Calculations.WebApi/"]
COPY ["Modules/Calculations/Calculations.Application/Calculations.Application.csproj", "Modules/Calculations/Calculations.Application/"]
COPY ["Common/Common.Application/Common.Application.csproj", "Common/Common.Application/"]
RUN dotnet restore "Modules/Calculations/Calculations.WebApi/Calculations.WebApi.csproj"
COPY . .
WORKDIR "/src/Modules/Calculations/Calculations.WebApi"
RUN dotnet build "Calculations.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Calculations.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Calculations.WebApi.dll"]
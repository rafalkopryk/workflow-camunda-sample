#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Modules/KafkaConnector/Utility.KafkaConnector/Utility.KafkaConnector.csproj", "Modules/KafkaConnector/Utility.KafkaConnector/"]
COPY ["Camunda/Camunda.Connector.Kafka/Camunda.Connector.Kafka.csproj", "Camunda/Camunda.Connector.Kafka/"]
COPY ["Camunda/Camunda.Connector.SDK.Core/Camunda.Connector.SDK.Core.csproj", "Camunda/Camunda.Connector.SDK.Core/"]
COPY ["Camunda/Camunda.Connector.SDK.Runtime/Camunda.Connector.SDK.Runtime.csproj", "Camunda/Camunda.Connector.SDK.Runtime/"]
COPY ["Camunda/Camunda.BPMN.Model/Camunda.BPMN.Model.csproj", "Camunda/Camunda.BPMN.Model/"]
COPY ["Camunda/Camunda.Client.Operate/Camunda.Client.Operate.csproj", "Camunda/Camunda.Client.Operate/"]
COPY ["Camunda/Camunda.Connector.SDK.Runtime.Util/Camunda.Connector.SDK.Runtime.Util.csproj", "Camunda/Camunda.Connector.SDK.Runtime.Util/"]
COPY ["Camunda/Camunda.Client/Camunda.Client.csproj", "Camunda/Camunda.Client/"]
RUN dotnet restore "./Modules/KafkaConnector/Utility.KafkaConnector/./Utility.KafkaConnector.csproj"
COPY . .
WORKDIR "/src/Modules/KafkaConnector/Utility.KafkaConnector"
RUN dotnet build "./Utility.KafkaConnector.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Utility.KafkaConnector.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utility.KafkaConnector.dll"]
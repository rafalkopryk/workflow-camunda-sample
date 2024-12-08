//registry

@minLength(5)
@maxLength(50)
@description('Provide a globally unique name of your Azure Container Registry')
param acrName string = 'acr${uniqueString(resourceGroup().id)}'

@description('Provide a location for the registry.')
param location string = resourceGroup().location

resource acrResource 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

@description('Output the login server property for later use')
output loginServer string = acrResource.properties.loginServer


//Service bus

@description('Name of the Service Bus namespace')
param serviceBusNamespaceName string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    tier: 'Standard'
    name: 'Standard'
  }
  properties: {
  }
}

var serviceBusEndpoint = '${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey'
var serviceBusConnectionString = listKeys(serviceBusEndpoint, serviceBusNamespace.apiVersion).primaryConnectionString

@description('AzServiceBusConnectionString')
output AzServiceBusConnectionString string = serviceBusConnectionString

//Container apps

@description('ContainerApp EnvironmentName')
param environmentName string

resource environment 'Microsoft.App/managedEnvironments@2023-11-02-preview' = {
  name: environmentName
  location:location
  properties: {
    workloadProfiles:[
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

//Storage

param storageAccountsName string = 'creditdemostorage'

resource storageAccounts 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountsName
  location: location
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  kind: 'StorageV2'
  properties: {
    dnsEndpointType: 'Standard'
    defaultToOAuthAuthentication: false
    publicNetworkAccess: 'Enabled'
    allowCrossTenantReplication: false
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      requireInfrastructureEncryption: false
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

// CosmosDB

param cosmosDbAccountName string = 'credit-demo-cosmosdb'

resource cosmosDbAccount 'Microsoft.DocumentDb/databaseAccounts@2024-02-15-preview' = {
  kind: 'GlobalDocumentDB'
  name: cosmosDbAccountName
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        id: '${cosmosDbAccountName}-${location}'
        failoverPriority: 0
        locationName: location
      }
    ]
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 1440
        backupRetentionIntervalInHours: 48
        backupStorageRedundancy: 'Local'
      }
    }
    isVirtualNetworkFilterEnabled: false
    virtualNetworkRules: []
    ipRules: []
    minimalTlsVersion: 'Tls12'
    enableMultipleWriteLocations: true
    capabilities: []
    enableFreeTier: true
    capacity: {
      totalThroughputLimit: 1000
    }
  }
}


// instights
param appInsightsName string = 'credit-demo-insights'

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
  scope: resourceGroup()
}

// Apps

resource creditapplications 'Microsoft.App/containerApps@2023-11-02-preview' =  {
  name: 'credit-demo-applications'
  location: location
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080       
        transport: 'auto'
      }
      secrets:[
        {
          name: 'cosmosdb-connectionstring'
          value: cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString
        }
        {
          name: 'sb-connectionstring'
          value: serviceBusConnectionString
        }
        {
          name: 'reg-password'
          value: acrResource.listCredentials().passwords[0].value
        }
        {
          name: 'insights-connectionstring'
          value: appInsights.properties.ConnectionString
        }
      ]
      registries:[
        {
          server: acrResource.properties.loginServer
          username: acrResource.listCredentials().username
          passwordSecretRef: 'reg-password'
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${acrResource.properties.loginServer}/applications-webapi:20240501074309'
          name: 'credit-demo-applications'
          env: [
            {
              name: 'otel__enabled'
              value: string(true)
            }
            {
              name: 'ConnectionStrings__APPLICATIONINSIGHTS'
              secretRef: 'insights-connectionstring'
            }
            {
              name: 'ConnectionStrings__AzCosmosDB'
              secretRef: 'cosmosdb-connectionstring'
            }
            {
              name: 'ConnectionStrings__AzServiceBus'
              secretRef: 'sb-connectionstring'
            }
            {
              name: 'DatabaseProvider'
              value: 'CosmosDb'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          volumeMounts:[]
          probes:[]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
      volumes:[]
    }
  }
}


resource creditcalculations 'Microsoft.App/containerApps@2023-11-02-preview' =  {
  name: 'credit-demo-calculations'
  location: location
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080       
        transport: 'auto'
      }
      secrets:[
        {
          name: 'cosmosdb-connectionstring'
          value: cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString
        }
        {
          name: 'sb-connectionstring'
          value: serviceBusConnectionString
        }
        {
          name: 'reg-password'
          value: acrResource.listCredentials().passwords[0].value
        }
        {
          name: 'insights-connectionstring'
          value: appInsights.properties.ConnectionString
        }
      ]
      registries:[
        {
          server: acrResource.properties.loginServer
          username: acrResource.listCredentials().username
          passwordSecretRef: 'reg-password'
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${acrResource.properties.loginServer}/calculations-webapi:20240501074452'
          name: 'credit-demo-calculations'
          env: [
            {
              name: 'otel__enabled'
              value: string(true)
            }
            {
              name: 'ConnectionStrings__APPLICATIONINSIGHTS'
              secretRef: 'insights-connectionstring'
            }
            {
              name: 'ConnectionStrings__AzCosmosDB'
              secretRef: 'cosmosdb-connectionstring'
            }
            {
              name: 'ConnectionStrings__AzServiceBus'
              secretRef: 'sb-connectionstring'
            }
            {
              name: 'DatabaseProvider'
              value: 'CosmosDb'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          volumeMounts:[]
          probes:[]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
        rules:[
          {
            name: 'simulation-queue'
            azureQueue:{
              queueName:'simulate-credit-command-handler'
              queueLength: 1
              auth:[
                {
                  secretRef: 'sb-connectionstring'
                  triggerParameter: 'connection'
                }
              ]
            }
          }
        ]
      }
      volumes:[]
    }
  }
}


//web

resource front 'Microsoft.Web/staticSites@2023-01-01' = {
  location: 'westeurope'
  name: 'credit-demo-front'
  sku:{
    tier:'Free'
    name:'Free'
  }
  properties:{
  }
}

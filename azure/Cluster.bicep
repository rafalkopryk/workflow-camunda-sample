@description('ContainerApp EnvironmentName')
param environmentName string

@description('Azure region of the deployment')
param location string = resourceGroup().location

@description('Zeebe ClusterSize')
param clusterSize int = 1

@description('Zeebe ReplicationFactor')
param replicationFactor int = clusterSize

@description('Zeebe PartitionsCount')
param partitionsCount int = clusterSize

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

resource storageAccountsFileServices 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  parent: storageAccounts
  name: 'default'
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  properties: {
    protocolSettings: {
      smb: {}
    }
    cors: {
      corsRules: []
    }
    shareDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource storageAccountsZeebeShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  parent: storageAccountsFileServices
  name: 'zeebe'
  properties: {
    accessTier: 'TransactionOptimized'
    shareQuota: 2
    enabledProtocols: 'SMB'
  }
  dependsOn: [
    storageAccounts
  ]
}

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

resource environmentZeebeShare 'Microsoft.App/managedEnvironments/storages@2023-08-01-preview' = {
  parent: environment
  name: 'zeebe'
  properties: {
    azureFile: {
      accountName: storageAccounts.name
      shareName: storageAccountsZeebeShare.name
      accessMode: 'ReadWrite'
      accountKey: storageAccounts.listKeys().keys[0].value
    }
  }
}

var nodeName = 'zeebe-node'
var clusterName = 'zeebe-cluster'
var nodes = [for i in range(0, clusterSize): '${nodeName}${i}:26501']

@batchSize(1)
resource zeebeCluster 'Microsoft.App/containerApps@2023-11-02-preview' = [for i in range(0, clusterSize): {
    name: '${nodeName}${i}'
    location: location
    properties: {
      managedEnvironmentId: environment.id
      configuration: {
        ingress: {
          external: true
          targetPort: 26500       
          transport: 'http2'
          allowInsecure:false
          additionalPortMappings: [
            {
              external: false
              targetPort: 26502
            }
            {
              external: false
              targetPort: 9600
            }
          ]
        }
      }
      template: {
        containers: [
          {
            image: 'camunda/zeebe:8.5.0'
            name: 'zeebe'
            env: [
              {
                name: 'JAVA_TOOL_OPTIONS'
                value: '-Xms512m -Xmx512m'
              }
              {
                name: 'ZEEBE_BROKER_DATA_SNAPSHOTPERIOD'
                value: '5m'
              }
              {
                name: 'ZEEBE_BROKER_DATA_DISK_FREESPACE_REPLICATION'
                value: '2GB'
              }
              {
                name: 'ZEEBE_BROKER_DATA_DISK_FREESPACE_PROCESSING'
                value: '3GB'
              }
              {
                name: 'ZEEBE_BROKER_CLUSTER_NODEID'
                value: string(i)
              }
              {
                name: 'ZEEBE_BROKER_CLUSTER_PARTITIONSCOUNT'
                value: string(partitionsCount)
              }
              {
                name: 'ZEEBE_BROKER_CLUSTER_REPLICATIONFACTOR'
                value: string(replicationFactor)
              }
              {
                name: 'ZEEBE_BROKER_CLUSTER_CLUSTERSIZE'
                value: string(replicationFactor)
              }
              {
                name: 'ZEEBE_BROKER_CLUSTER_INITIALCONTACTPOINTS'
                value: join(nodes, ',')
              }
              // {
              //   name: 'ZEEBE_BROKER_CLUSTER_CLUSTERNAME'
              //   value: clusterName
              // }
              {
                name: 'ZEEBE_BROKER_GATEWAY_ENABLE'
                value: string(true)
              }
              {
                name: 'ZEEBE_BROKER_DATA_DISKUSAGECOMMANDWATERMARK'
                value: string('0.998')
              }
              {
                name: 'ZEEBE_BROKER_DATA_DISKUSAGEREPLICATIONWATERMARK'
                value: string('0.999')
              }
            ]
            resources: {
              cpu: json('0.5')
              memory: '1Gi'
            }
            volumeMounts:[
              {
                mountPath:'/usr/local/zeebe/data'
                volumeName:'zeebe'
              }
            ]
            probes:[
              {
                type: 'Startup'
                httpGet:{
                  port: 9600
                  scheme:'HTTP'
                  path:'/actuator/health/startup'
                }
                initialDelaySeconds:30
                periodSeconds:30
                successThreshold:1
                failureThreshold:5
                timeoutSeconds:1
              }
              {
                type: 'Readiness'
                httpGet:{
                  port: 9600
                  scheme:'HTTP'
                  path:'/actuator/health/readiness'
                }
                initialDelaySeconds:30
                periodSeconds:30
                successThreshold:1
                failureThreshold:5
                timeoutSeconds:1
              }
              {
                type: 'Liveness'
                httpGet:{
                  port: 9600
                  scheme:'HTTP'
                  path:'/actuator/health/liveness'
                }
                initialDelaySeconds:30
                periodSeconds:30
                successThreshold:1
                failureThreshold:5
                timeoutSeconds:1
              }
            ]
          }
        ]
        scale: {
          minReplicas: 1
          maxReplicas: 1
        }
        volumes:[
          {
            storageName: environmentZeebeShare.name
            storageType:'AzureFile'
            name:'zeebe'
          }
        ]
      }
    }
  }]

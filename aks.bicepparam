using './aks.bicep'

param clusterName = 'camunda-aks'
param dnsPrefix = 'camunda'
param osDiskSizeGB = 64
param agentCount = 1
param agentVMSize = 'standard_d2s_v5'
param linuxAdminUsername = 'TODO'
param sshRSAPublicKey = 'ssh-rsa TODO'


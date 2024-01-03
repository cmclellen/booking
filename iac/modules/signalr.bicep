param location string
param tags object

@description('The pricing tier of the SignalR resource.')
@allowed([
  'Free_F1'
  'Standard_S1'
  'Premium_P1'
])
param pricingTier string = 'Free_F1'

@description('The number of SignalR Unit.')
@allowed([
  1
  2
  5
  10
  20
  50
  100
])
param capacity int = 1

param resourceNameFormat string

param functionAppPrincipalId string

@description('Visit https://github.com/Azure/azure-signalr/blob/dev/docs/faq.md#service-mode to understand SignalR Service Mode.')
@allowed([
  'Default'
  'Serverless'
  'Classic'
])
param serviceMode string = 'Serverless'

param enableConnectivityLogs bool = true

param enableMessagingLogs bool = true

param enableLiveTrace bool = true

@description('Set the list of origins that should be allowed to make cross-origin calls.')
param allowedOrigins array = [
  '*'
]

resource signalR 'Microsoft.SignalRService/signalR@2022-02-01' = {
  name: format(resourceNameFormat, 'sigr', '')
  location: location
  tags: tags
  sku: {
    capacity: capacity
    name: pricingTier
  }
  kind: 'SignalR'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        flag: 'ServiceMode'
        value: serviceMode
      }
      {
        flag: 'EnableConnectivityLogs'
        value: string(enableConnectivityLogs)
      }
      {
        flag: 'EnableMessagingLogs'
        value: string(enableMessagingLogs)
      }
      {
        flag: 'EnableLiveTrace'
        value: string(enableLiveTrace)
      }
    ]
    cors: {
      allowedOrigins: allowedOrigins
    }
  }
}

resource signalRAppServerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '420fcaa2-552c-430f-98ca-3264be4806c7'
}

resource signalRServiceOwnerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '7e4f1700-ea5a-4f59-8f37-079cfe29dce3'
}

resource roleAssignmentAppServer 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: signalR
  name: guid(signalR.id, functionAppPrincipalId, signalRAppServerRoleDefinition.id)
  properties: {
    roleDefinitionId: signalRAppServerRoleDefinition.id
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource roleAssignmentServiceOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: signalR
  name: guid(signalR.id, functionAppPrincipalId, signalRServiceOwnerRoleDefinition.id)
  properties: {
    roleDefinitionId: signalRServiceOwnerRoleDefinition.id
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

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

param functionBaseUrl string

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
    upstream: {
      templates: [
        {
          categoryPattern: '*'
          eventPattern: '*'
          hubPattern: 'serverless'
          urlTemplate: '${functionBaseUrl}/runtime/webhooks/signalr'
          auth: {
            managedIdentity: {
              resource: functionBaseUrl
            }
            type: 'SystemAssigned'
          }
        }
      ]
    }
  }
}

var roleIds = [
  '420fcaa2-552c-430f-98ca-3264be4806c7' // SignalR App Server
  '7e4f1700-ea5a-4f59-8f37-079cfe29dce3' // SignalR Service Owner
]

resource signalRRoleDefinitions 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = [for roleId in roleIds: {
  scope: subscription()
  name: roleId
}]

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for (roleId, index) in roleIds: {
  scope: signalR
  name: guid(signalR.id, functionAppPrincipalId, signalRRoleDefinitions[index].id)
  properties: {
    roleDefinitionId: signalRRoleDefinitions[index].id
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}]

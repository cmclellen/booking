param location string
param tags object
param resourceNameFormat string
param storageAccountType string = 'Standard_LRS'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: format(replace(resourceNameFormat, '-', ''), 'st', 'fnapp')
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: storageAccountType
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: format(resourceNameFormat, 'asp', '')
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: format(resourceNameFormat, 'appi', '')
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

var fnAppName = format(resourceNameFormat, 'fn', '')

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: fnAppName
  location: location
  tags: tags
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    clientCertEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNET|6.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: [
          'https://cmclellen.github.io'
          'http://localhost:5173'
        ]
        supportCredentials: true
      }
    }
    httpsOnly: true
  }
}

var functionAppPrincipalId = functionApp.identity.principalId

var roleIds = [
  '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3' // Storage Table Data Contributor
]

resource storageRoleDefinitions 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = [for roleId in roleIds: {
  scope: subscription()
  name: roleId
}]

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for (roleId, index) in roleIds: {
  scope: storageAccount
  name: guid(storageAccount.id, fnAppName, storageRoleDefinitions[index].id)
  properties: {
    roleDefinitionId: storageRoleDefinitions[index].id
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}]

output functionAppPrincipalId string = functionAppPrincipalId

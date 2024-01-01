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
      appSettings: [
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(fnAppName)
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
    }
    httpsOnly: true
  }
}

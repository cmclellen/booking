param location string
param tags object
param resourceNameFormat string

resource law 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: format(resourceNameFormat, 'log', '')
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 90
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

output workspaceId string = law.id

@description('The environment.')
param environment string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Tags to tag all resources.')
param tags object = {
  Environment: environment
}

var resourceNameFormat = '{0}-res{1}-aueast'

module monitoring './modules/monitoring.bicep' = {
  name: 'Monitoring'
  params: {
      location: location
      resourceNameFormat: resourceNameFormat
      tags: tags
  }
}

module fnapp './modules/function.bicep' = {
  name: 'FnApp'
  params: {
      location: location
      resourceNameFormat: resourceNameFormat
      tags: tags
  }
}

module signalr './modules/signalr.bicep' = {
    name: 'SignalR'
    params: {
        location: location
        resourceNameFormat: resourceNameFormat
        tags: tags
        functionAppPrincipalId: fnapp.outputs.functionAppPrincipalId
        functionBaseUrl: fnapp.outputs.functionBaseUrl
        workspaceId: monitoring.outputs.workspaceId
    }
}

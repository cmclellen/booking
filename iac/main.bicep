@description('The environment.')
param environment string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Tags to tag all resources.')
param tags object = {
  Environment: environment
}

var resourceNameFormat = '{0}-res{1}-aueast'

module signalr './modules/signalr.bicep' = {
    name: 'SignalR'
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

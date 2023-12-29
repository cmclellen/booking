@description('The environment.')
param environment string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Tags to tag all resources.')
param tags object = {
  Environment: environment
}

module signalr './modules/signalr.bicep' = {
    name: 'SignalR'
    params: {
        location: location
        tags: tags
    }
}

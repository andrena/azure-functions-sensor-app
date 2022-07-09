@description('Specifies the url of the private container registry.')
param containerRegistryUrl string = ''
@description('Specifies the user name for the private container registry.')
param containerRegistryUser string = ''
@description('Specifies the password for the private container registry.')
@secure()
param containerRegistryPassword string = ''
@description('Specifies the default location of resources.')
param location string = resourceGroup().location

var functionAppStorageAccount = {
  name: 'sensorapp'
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
}
var functionAppServicePlan = {
  name: 'sensor-app'
  sku: {
    name: 'EP1'
    tier: 'ElasticPremium'
  }
  properties: {
    workerSize: 3
    workerSizeId: 3
    numberOfWorkers: 1
    reserved: true
    maximumElasticWorkerCount: 5
  }
}
var functionAppService = {
  name: 'sensor-app'
}
var functionAppApplicationInsights = {
  name: 'sensor-app'
}

//========== RESOURCE FOR STORAGE ACCOUNT ==========
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: functionAppStorageAccount.name
  location: location
  kind: 'StorageV2'
  sku: functionAppStorageAccount.sku
  properties: {
    supportsHttpsTrafficOnly: true
  }
}
//========== RESOURCE FOR FUNCTION-APP-SERVICE-PLAN (SERVERFARMS) ==========
resource servicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: functionAppServicePlan.name
  location: location
  sku: functionAppServicePlan.sku
  properties: functionAppServicePlan.properties
  kind: 'linux'
}

//========== RESOURCE FOR APPLICATION-INSIGHTS ==========
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: functionAppApplicationInsights.name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'IbizaWebAppExtensionCreate'
  }
}

//========== RESOURCE FOR FUNCTION-APP-SERVICE ==========
resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppService.name
  location: location
  kind: 'functionapp,linux'
  properties: {
    httpsOnly: true
    serverFarmId: servicePlan.id
    clientAffinityEnabled: false
  }
  identity: {
    type: 'SystemAssigned'
  }
  resource config 'config' = {
    name: 'web'
    properties: {
      remoteDebuggingVersion: 'VS2019'
      use32BitWorkerProcess: false
      http20Enabled: true
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      linuxFxVersion: 'DOCKER|${containerRegistryUrl}/${functionAppService.name}:latest'
    }
  }
}

//========== RESOURCE FOR FUNCTION-APP-SERVICE (App Settings) ==========
resource functionAppConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  parent: functionApp
  name: 'appsettings'
  properties: {
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    FUNCTIONS_EXTENSION_VERSION: '~4'
    APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsights.properties.InstrumentationKey
    AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
    DOCKER_REGISTRY_SERVER_URL: 'https://${containerRegistryUrl}'
    DOCKER_REGISTRY_SERVER_USERNAME: containerRegistryUser
    DOCKER_REGISTRY_SERVER_PASSWORD: containerRegistryPassword
    WEBSITES_ENABLE_APP_SERVICE_STORAGE: 'false'
  }
}

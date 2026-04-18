@description('The location where the resources will be deployed. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('The name of the virtual network to which the VM will be connected.')
param vnetName string

@description('The name of the subnet within the virtual network.')
param subnetName string

@description('The SSH public key to be added to the VM for authentication.')
param sshPublicKey string

@description('A unique instance ID to distinguish from other deployments.')
param instanceId string

@description('The administrator username for the VM. Defaults to "systemadmin".')
param adminUsername string = 'systemadmin'

@description('An array of additional public IP addresses (resource IDs) to assign to the NIC.')
param additionalIpResourceIds string[] = []

var identifier string = uniqueString(resourceGroup().id, instanceId)

var tags = {
  InstanceId: instanceId
}

resource ip 'Microsoft.Network/publicIPAddresses@2025-05-01' = {
  name: 'ip-${identifier}'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
  }
  tags: tags
}

resource nsg 'Microsoft.Network/networkSecurityGroups@2025-05-01' = {
  name: 'nsg-${identifier}'
  location: location
  properties: {
    securityRules: [
      {
        name: 'AllowSSHInbound'
        properties: {
          description: 'Allow SSH from the internet'
          protocol: 'Tcp'
          sourcePortRange: '*'
          destinationPortRange: '22'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 1000
          direction: 'Inbound'
        }
      }
    ]
  }
  tags: tags
}

resource nic 'Microsoft.Network/networkInterfaces@2025-05-01' = {
  name: 'nic-${identifier}'
  location: location
  properties: {
    networkSecurityGroup: {
      id: nsg.id
    }
    ipConfigurations: [for (ipResourceId, i) in concat([ip.id], additionalIpResourceIds): {
      name: toLower('ipconfig-${last(split(ipResourceId, '/'))}')
      properties: {
        subnet: {
          id: resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, subnetName)
        }
        publicIPAddress: {
          id: ipResourceId
        }
        privateIPAllocationMethod: 'Dynamic'
        primary: (i == 0)
      }
    }]
  }
  tags: tags
}

resource vm 'Microsoft.Compute/virtualMachines@2025-04-01' = {
  name: 'vm-${identifier}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_B2pls_v2' // TODO: Configurable VM SKU?
    }
    osProfile: {
      computerName: identifier
      adminUsername: adminUsername
      customData: loadFileAsBase64('cloud-init.yml') // TODO: Create this file
      linuxConfiguration: {
        disablePasswordAuthentication: true
        ssh: {
          publicKeys: [
            {
              path: '/home/${adminUsername}/.ssh/authorized_keys'
              keyData: sshPublicKey
            }
          ]
        }
      }
    }
    storageProfile: {
      imageReference: {
        publisher: 'Canonical'
        offer: '0001-com-ubuntu-minimal-resolute'
        sku: 'minimal-26_04-lts-gen2'
        version: 'latest'
      }
      osDisk: {
        createOption: 'FromImage'
        diskSizeGB: 32
        managedDisk: {
          storageAccountType: 'StandardSSD_LRS'
        }
        deleteOption: 'Delete'
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: nic.id
        }
      ]
    }
  }
  tags: tags
}

@description('The VM ID')
output vmId string = vm.id

@description('The name of the VM')
output vmName string = vm.name

@description('The private IP address assigned to the VM.')
output privateIp string = nic.properties.ipConfigurations[0].properties.privateIPAddress

@description('The (static) public IP address assigned to the VM.')
output publicIp string = ip.properties.ipAddress

@description('The Principal ID of the System-Assigned Managed Identity.')
output principalId string = vm.identity.principalId

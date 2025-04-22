resource listResourceGroups 'orka.tool/az.group.list' = {
  name: 'listResourceGroups'
  properties: {
    input: {
      command: 'az group list --output json'
    }
  }
}
resource listResourceGroups 'orka.tool/az.group.list' = {
  name: 'listResourceGroups'
  properties: {
    input: {
      command: 'echo hello'
    }
  }
}

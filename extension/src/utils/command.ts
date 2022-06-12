const commandPrefix = 'shimakaze-sdk'

export const command = (command: string) => `${commandPrefix}.${command}`
export const _command = (command: string) => `_${commandPrefix}.${command}`
export const _viewCommand = (viewType: string, command: string) => `_${viewType}.${command}`

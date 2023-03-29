# Shimakaze.Sdk.NodeJS

Build your Node Project Easy!

|property|default|remark|
|:-:|:-:|:-|
|`NeedInstall`|`True`|Whether you need to restore the project before building.
|`ProjectFile`|`package.json`|Your `package.json` file.
|`LockFile`|`package-lock.json`|Your Package Manager's Lock file.
|`RestoreCommand`|`npm install`|What command do you want to use when you restore your project.
|`BuildCommand`|`npm run build`|What command do you want to use when you build your project.
|`UseUtf8Encoding`|`Detect`|You can set the value is `Always` when command output is garbage code.


## How to use?
Very easy sample like this.

```xml
<!-- Project.esproj -->
<Project Sdk="Shimakaze.Sdk.NodeJS/0.0.1"/>
```

Or you like use yarn.

```xml
<!-- Project.esproj -->
<Project Sdk="Shimakaze.Sdk.NodeJS/0.0.1">
  <PropertyGroup>
    <LockFile>yarn.lock</LockFile>
    <RestoreCommand>yarn install</RestoreCommand>
    <BuildCommand>yarn build</BuildCommand>
  </PropertyGroup>
</Project>
```
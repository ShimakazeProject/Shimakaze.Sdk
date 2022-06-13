# shimakaze-sdk

|构建状态
|:-:|
[![ci](https://github.com/ShimakazeProject/Shimakaze-SDK/actions/workflows/ci.yml/badge.svg)](https://github.com/ShimakazeProject/Shimakaze-SDK/actions/workflows/ci.yml)

Shimakaze SDK是一个将MSBuild构建系统带到Red Alert 2模组开发过程中的计划
通过使用Shimakaze SDK的编译套件, 您可以轻松的整理您的游戏的代码
编译套件将会自动帮您生成一个向前兼容的模组文件

这个计划中包含以下内容

- [x] CSF编译器 将特定格式的文档编译成游戏可用的二进制文件
- [x] INI预处理器 用于处理在INI文档中的预处理器指令
- [ ] 语言服务器 可以为编辑器提供诸如自动完成, 代码提示之类的功能的服务器
- [ ] MSBuild扩展 使MSBuild可以构建模组的扩展
- [ ] VSCode扩展 使VSCode可以通过语言服务器获得更好的开发时体验

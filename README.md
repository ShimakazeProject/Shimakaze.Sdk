# Shimakaze.SDK

|master|develop|
|:-:|:-:|
|[![Build](https://github.com/ShimakazeProject/Shimakaze.Sdk/actions/workflows/build.yml/badge.svg?branch=master&event=push)](https://github.com/ShimakazeProject/Shimakaze.Sdk/actions/workflows/build.yml)|[![Build](https://github.com/ShimakazeProject/Shimakaze.Sdk/actions/workflows/build.yml/badge.svg?branch=develop&event=push)](https://github.com/ShimakazeProject/Shimakaze.Sdk/actions/workflows/build.yml)|
|[![Coverage Status](https://coveralls.io/repos/github/ShimakazeProject/Shimakaze.Sdk/badge.svg?branch=master)](https://coveralls.io/github/ShimakazeProject/Shimakaze.Sdk?branch=master)|[![Coverage Status](https://coveralls.io/repos/github/ShimakazeProject/Shimakaze.Sdk/badge.svg?branch=develop)](https://coveralls.io/github/ShimakazeProject/Shimakaze.Sdk?branch=develop)|

## 这是什么？

这是一套为C&C模组开发设计的现代化的工具集，它致力于为C&C的模组开发融入更加现代化的方式。

## 为什么要用这个？

您是否认为在一个INI文件中保存所有的配置不便于管理？

我们的INI编译器可以将多个不同的INI合并成一个。
也就是说，您的的项目可以像这样组织
```plain
/
└── src
    ├── Infantries
    │   ├── E1
    │   │   ├── E1.art
    │   │   └── E1.rule
    │   └── E2
    │       ├── E2.art
    │       └── E2.rule
    └── Vehicles
        └── AMCV
            ├── AMCV.art
            └── AMCV.rule
```
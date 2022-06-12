# 关于此Schema的问题

您可能注意到这里的Schema与[公开的版本](https://shimakazeproject.github.io/json/csf/v2/schema.json)不同,  
这是因为[APIDevTools/json-schema-ref-parser](https://github.com/APIDevTools/json-schema-ref-parser)仓库还没有支持在顶层使用`$ref`  
您可以在[APIDevTools/json-schema-ref-parser#201](https://github.com/APIDevTools/json-schema-ref-parser/issues/201)看到此问题的描述
在[APIDevTools/json-schema-ref-parser](https://github.com/APIDevTools/json-schema-ref-parser)修复这个问题并且[bcherny/json-schema-to-typescript](https://github.com/bcherny/json-schema-to-typescript)工具更新后,  
我们会将此schema与[公开的版本](https://shimakazeproject.github.io/json/csf/v2/schema.json)同步

## 引用

- [APIDevTools/json-schema-ref-parser#201](https://github.com/APIDevTools/json-schema-ref-parser/issues/201)
- [bcherny/json-schema-to-typescript#349](https://github.com/bcherny/json-schema-to-typescript/issues/349)

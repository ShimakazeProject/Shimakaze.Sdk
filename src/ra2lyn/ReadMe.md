# Ra2lyn

面向《命令与征服：尤里的复仇》INI 数据的编译器前后端。

> 名字后缀 `lyn` 与 Roslyn 同构，表示这是一个编译器 / 分析器 / 语言服务器内核；
> `ra2` 表示目标数据来自 Red Alert 2 / Yuri's Revenge 使用的 INI 与规则体系。

## 1. 组成

| 项目 | 角色 | 职责 |
|---|---|---|
| `Shimakaze.Sdk.Ra2lyn.Common` | 后端 | 源位置与诊断、IR、规则 XML、类型系统、语义 Pass、编译驱动、发射契约 |
| `Shimakaze.Sdk.Ra2lyn.Ini` | 前端 | INI 词法、语法树、语法诊断、AST → IR 降级、INI 发射器 |
| `Shimakaze.Sdk.Analyzer.Common` | 分析器框架 | 分析器契约、分析上下文、执行顺序与诊断合并 |
| `Shimakaze.Sdk.Analyzer.IniRule` | 分析器 | 按节定义与键规则校验 INI 实体（`INI201`～`INI203`） |

依赖规则：

- `Ra2lyn.Common` 只依赖 BCL（规则 XML 使用 `System.Xml.Linq`）。
- `Ra2lyn.Ini` 只依赖 `Ra2lyn.Common` 与 BCL。
- `Analyzer.Common` 只依赖 `Ra2lyn.Common`（需要 IR 与诊断类型）；**不得**引用任何具体分析器。
- `Analyzer.IniRule` 只依赖 `Analyzer.Common`（经传递引用可访问 `Ra2lyn.Common`）。
- 四个项目都**不得**依赖 EF Core、LSP 模型或 MSBuild。
- MSBuild / CLI / LSP 是消费者：`Shimakaze.Sdk.Engine` 提供门面，CLI 提供 `ra2lyn` 子命令。

```text
Ra2lyn.Ini ──只引用──▶ Ra2lyn.Common ◀──只引用── Analyzer.Common
   │                     │                          ▲
   │ 词法 / 语法 / AST     │ IR / 诊断 / 规则 / 类型系统   │
   │ AST → IR 降级         │ 分析 Pass / 可达性 / 发射契约  │
   │ INI 文本发射          │ 编译驱动                    │
                                                    Analyzer.IniRule
```

## 2. 编译流程

```text
源文本 ─▶ IniLexer ─▶ IniParser ─▶ AstToIrLowerer ─▶ FrontendResult
                                                        │
                     Compilation ──▶ MergePass
                                ──▶ ClassificationPass
                                ──▶ ReferencePass
                                ──▶ DiscoveryPass
                                ──▶ ReachabilityPass
                                ──▶ （分析阶段）AnalyzerRunner ──▶ IniRuleAnalyzer
                                                        │
                                                  CompilationModel
                                                        │
                                                  IniWriter（IEmitter）
```

- **前端只产出 IR**：`FrontendResult(Group, Sources, Entities, Diagnostics)`；语法诊断在降级阶段一并汇入。
- **后端与源语言无关**：`Ra2lyn.Common` 不认识 `[section]`、`key=value`、继承括号等语法。
- **Pass 顺序固定**（`Compilation.CreateDefaultPasses()`）：合并 → 分类 → 引用 → 发现 → 可达性。
- **分析阶段**：通过 `Compilation.AddAnalysis(...)` 接入，在全部 Pass 结束后执行；规则校验由
  `Analyzer.IniRule` 提供，内核不依赖任何具体分析器。分析阶段排在最后，使被发现规则新标记为普通实体的节
  同样接受类型校验。

### 2.1 分析器

```text
AnalyzerRunner.Run(model, options) ─▶ foreach analyzer: analyzer.Analyze(context)
                                   ─▶ 合并编译诊断与分析诊断，按源文件与位置排序去重
```

- `IAnalyzer`：`Name` + `Analyze(AnalyzerContext)`；实例可复用，状态放在上下文里。
- `AnalyzerContext`：只读的 `CompilationModel`、`AnalyzerOptions`、`Diagnostics` 与 `Report(...)`。
- `AnalyzerRunner.RegisterDefault(...)`：宿主在启动时登记具体分析器；`Analyzer.Common` 不含任何实现。
- `IniRuleAnalyzerOptions.FromCompilation(context)`：把规则集、类型标注、校验模式与外部验证器
  从编译上下文转交给规则分析器。


## 3. 核心类型

### 3.1 源与诊断（`Ra2lyn.Common.Source`）

- `SourceSpan(Path, Start, End)`：字符偏移，`End` 为开区间；`default` 表示没有位置。
- `SourceFile(Path, Text, Checksum?)`、`Diagnostic(Code, Severity, Message, Span)`。
- `DiagnosticCodes`：`INI001`～`INI010`、`INI099`、`INI101`～`INI104`、`INI201`～`INI203`。
- `SourceLocator.GetPosition(source, offset)` 把偏移换算成零基行列（MSBuild / CLI / LSP 共用）。

### 3.2 IR（`Ra2lyn.Common.Ir`）

- `EntityId` / `PropertyId`：整数、由 `IrBuilder` 在登记时按命令顺序分配，确定且稳定。
- `EntityModel` ≈ 源语言中的“节”；`PropertyModel` ≈ “键值对”；`ReferenceModel` 是一条引用边。
- `EntityKind`：`Unknown` / `Registry` / `Global` / `EnumSection` / `Entity`。
- `ReferenceKind`：`SectionRef` / `RegistryRef` / `Discovery` / `Global`。
- 名字比较统一 `OrdinalIgnoreCase`，值保持原始字符串。

### 3.3 规则与类型（`Ra2lyn.Common.Rules` / `Ra2lyn.Common.Types`）

- `RuleSetReader.ReadFilesAsync(paths)` 读取 XML；`Include` 支持相对路径、递归与循环去重。
- 顶层 `<Type>` / `<Enum>` 登记到共享的 `default` 组；节定义只在所属组内可见。
- `TypeValidator` 校验 `int` / `float` / `percent` / `boolean` / `string`、列表、复合类型、枚举、
  外部验证器（`IExternalTypeValidator`）与未知类型兜底；`ValidationMode` 区分宽松与严格。

### 3.4 编译驱动与发射契约

- `Compilation(CompilationOptions).Add(FrontendResult).AddRules(RuleSet).Run()`。
- `IEmitter.Emit(CompilationModel, EmitOptions)`；不支持的选项（如 `PreserveComments`）必须抛异常，禁止静默忽略。
- 发射器由消费者显式调用：`IniWriter` 在 `Ra2lyn.Ini`，Common 不认识 INI 语法。

## 4. 前端（`Ra2lyn.Ini`）

- `IniLexer.Tokenize(text, path)`：`[ ] = : , ; #`、空白、文本、换行、EOF。
- `IniParser.Parse(SourceFile)`：产生 `IniSyntaxTree`（文档节点、节节点、键值节点、继承项、自由记号）。
- 继承分隔符：第一项 `Colon`，后续 `Comma`；缺失时分别报告 `INI004` / `INI005`。
- 重复节（`INI101`）、顶层键值对（`INI102`）、重复键（`INI103`）在降级阶段报告。
- `IniWriter`：`[Name]`、`[Name]:[Base1],[Base2]`、每行 `Key=Value`；按组输出 `{组名}.ini`；
  `TreeShaking` 只依赖 IR 的 `IsReachable`；`PreserveComments` 暂不支持。

## 5. 消费者

- **CLI**：`Shimakaze.Sdk.Engine.Cli` 的 `ra2lyn build`（`ra2lyn` 可缩写为 `ini`）。
  用法：`ra2lyn build --input a.ini [--input b.ini ...] [--rules x.xml ...] [--output dir] [--group g] [--tree-shaking]`。
- **MSBuild**：`Shimakaze.Sdk.Ra2lyn.props` / `.targets` 收集输入，并调用上述 CLI 完成编译。
- 门面：`Shimakaze.Sdk.Engine.Ra2lyn.Ra2lynCompiler`，只使用前后端公开类型。

## 6. 扩展点

- 新的源语言目标（SHP 等）：新增 `Ra2lyn.<Source>` 项目。Common 保持与源语言无关，
  若目标与某个源语言共用一个写入器，则放在该源语言项目内。
- 预处理与变量：`#if` / `#include` / `$()` 目前不进 MVP，预留位置为前端词法/语法与降级之间。
- 索引 / 缓存：放到独立 `Ra2lyn.Index` 项目，内核不得引用。
- LSP：独立 `Ra2lyn.Lsp`，只做 `Diagnostic` → LSP 映射，`Range` 由 `SourceFile` 计算。

## 7. 已知限制

- 注释不往返：AST 保留自由记号，但 IR 与发射丢弃注释。
- 多文件合并顺序由调用方给出，不按文件名排序。
- 规则校验只覆盖普通实体；注册表 / 全局 / 枚举节由各自语义规则处理。

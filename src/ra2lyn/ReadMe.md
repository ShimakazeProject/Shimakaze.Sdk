# Ra2lyn

面向《命令与征服：尤里的复仇》INI 数据的编译器前后端。

## 1. 组成

| 项目 | 角色 | 职责 |
|---|---|---|
| `Shimakaze.Sdk.Ra2lyn.Common` | 内核 | 源位置与诊断、IR（节/组/键/简单值或引用值）、合并、编译驱动、发射契约 |
| `Shimakaze.Sdk.Ra2lyn.Ini` | 前端 | INI 词法、语法树、语法诊断、AST → IR 降级、INI 发射器 |
| `Shimakaze.Sdk.Ra2lyn.Analyzer.Common` | 分析层 | 规则 XML、类型系统、语义注解、分类/引用/发现/可达性分析器、分析运行器 |
| `Shimakaze.Sdk.Ra2lyn.Analyzer.IniRule` | 分析器 | 按节定义与键规则校验 INI 实体（`INI201`～`INI203`） |

依赖规则：

- `Ra2lyn.Common` 只依赖 BCL；**不认识**规则、类型、分组等具体语义。
- `Ra2lyn.Ini` 只依赖 `Ra2lyn.Common` 与 BCL。
- `Analyzer.Common` 只依赖 `Ra2lyn.Common`（需要 IR 与诊断类型）；**不得**引用任何具体分析器。
- `Analyzer.IniRule` 依赖 `Analyzer.Common` 与 `Ra2lyn.Common`。
- 四个项目都**不得**依赖 EF Core、LSP 模型或 MSBuild。
- MSBuild / CLI / LSP 是消费者：`Shimakaze.Sdk.Engine` 提供门面，CLI 提供 `ra2lyn` 子命令。

```text
Ra2lyn.Ini ──只引用──▶ Ra2lyn.Common ◀──只引用── Analyzer.Common ◀──只引用── Analyzer.IniRule
   │                     │                          │
   │ 词法 / 语法 / AST     │ IR（节 / 组 / 键 / 值形态）  │ 规则 XML / 类型系统 / 语义注解
   │ AST → IR 降级         │ 合并 / 编译驱动 / 发射契约     │ 分类 / 引用 / 发现 / 可达性分析器
   │ INI 文本发射          │                            │ 分析运行器与诊断合并
```

## 2. 编译流程

```text
源文本 ─▶ IniLexer ─▶ IniParser ─▶ AstToIrLowerer ─▶ FrontendResult
                                                        │
                     Compilation ──▶ MergePass（内核唯一 Pass）
                                                        │
                     （分析阶段）AnalyzerRunner
                         ├─▶ ClassificationAnalyzer
                         ├─▶ ReferenceAnalyzer
                         ├─▶ DiscoveryAnalyzer
                         ├─▶ ReachabilityAnalyzer
                         └─▶ IniRuleAnalyzer（宿主登记）
                                                        │
                                        SemanticAnnotations / ReferenceGraph
                                                        │
                                                  CompilationModel
                                                        │
                                                  IniWriter（IEmitter）
```

- **前端只产出 IR**：`FrontendResult(Group, Sources, Entities, Diagnostics)`；语法诊断在降级阶段一并汇入。
- **内核与具体语义无关**：`Ra2lyn.Common` 只认识“节 / 组 / 键 / 简单值或引用值”，
  分类、类型、可达性等都是分析层的注解（`SemanticAnnotations`），不写进实体模型。
- **Pass 与分析的边界**：`Compilation` 内置只有 `MergePass`（跨文件合并，纯结构操作）；
  其余分析全部通过 `AddAnalysis(...)` 注入，在合并之后执行。

### 2.1 分析器

```text
AnalyzerRunner.Run(model, options) ─▶ foreach analyzer: analyzer.Analyze(context)
                                   ─▶ 合并编译诊断与分析诊断，按源文件与位置排序去重
```

- `IAnalyzer`：`Name` + `Analyze(AnalyzerContext)`；实例可复用，状态放在上下文里。
- `AnalyzerContext`：只读的 `CompilationModel`、`AnalyzerOptions`（含 `RuleSet`）、
  `SemanticAnnotations`、`ReferenceGraph`、`Diagnostics` 与 `Report(...)`。
- 默认链顺序固定：分类 → 引用 → 发现 → 可达性；规则校验类分析器由宿主追加到链尾。
- `AnalyzerRunner.RegisterDefault(...)`：宿主在启动时登记具体分析器；`Analyzer.Common` 不含具体校验规则。
- 可达性结论通过 `AnalyzerResult.Annotations` 回传，由门面写入 `CompilationModel.ReachableEntities`，
  发射器据此做 TreeShaking。
## 3. 核心类型

### 3.1 源与诊断（`Ra2lyn.Common.Source`）

- `SourceSpan(Path, Start, End)`：字符偏移，`End` 为开区间；`default` 表示没有位置。
- `SourceFile(Path, Text, Checksum?)`、`Diagnostic(Code, Severity, Message, Span)`。
- `DiagnosticCodes`：`INI001`～`INI010`、`INI099`、`INI101`～`INI104`、`INI201`～`INI203`。
- `SourceLocator.GetPosition(source, offset)` 把偏移换算成零基行列（MSBuild / CLI / LSP 共用）。

### 3.2 IR（`Ra2lyn.Common.Ir`）

- `EntityId` / `PropertyId`：整数、由 `IrBuilder` 在登记时按命令顺序分配，确定且稳定。
- `EntityModel` ≈ 源语言中的“节”；`PropertyModel` ≈ “键值对”；`ReferenceModel` 是一条引用边。
- `PropertyValueKind`：`Simple` / `Reference`——内核只区分值形态，含义由分析层解释。
- `ReferenceKind`：`Direct` / `Indirect` / `Derived` / `Entry`。
- 名字比较统一 `OrdinalIgnoreCase`，值保持原始字符串。

### 3.3 分析层（`Analyzer.Common`）

- `EntitySemanticKind`：`Unknown` / `Registry` / `Global` / `EnumSection` / `Entity`；
  与类型名、可达性一起保存在 `SemanticAnnotations`，不写进实体模型。
- `ReferenceGraph`：引用分析器的输出，供可达性分析与发射阶段使用。
- `Rules`（`RuleSetReader.ReadFilesAsync(paths)` 读取 XML；`Include` 支持相对路径、递归与循环去重；
  顶层 `<Type>` / `<Enum>` 登记到共享的 `default` 组）。
- `Types`（`TypeValidator` 校验 `int` / `float` / `percent` / `boolean` / `string`、列表、复合类型、枚举、
  外部验证器（`IExternalTypeValidator`）与未知类型兜底；`ValidationMode` 区分宽松与严格）。

### 3.4 编译驱动与发射契约

- `Compilation(CompilationOptions).Add(FrontendResult).AddPass(...).AddAnalysis(...).Run()`：内核只内置合并 Pass。
- `IEmitter.Emit(CompilationModel, EmitOptions)`；不支持的选项（如 `PreserveComments`）必须抛异常，禁止静默忽略。
- 发射器由消费者显式调用：`IniWriter` 在 `Ra2lyn.Ini`，内核不认识 INI 语法。

## 4. 前端（`Ra2lyn.Ini`）

- `IniLexer.Tokenize(text, path)`：`[ ] = : , ; #`、空白、文本、换行、EOF。
- `IniParser.Parse(SourceFile)`：产生 `IniSyntaxTree`（文档节点、节节点、键值节点、继承项、自由记号）。
- 继承分隔符：第一项 `Colon`，后续 `Comma`；缺失时分别报告 `INI004` / `INI005`。
- 重复节（`INI101`）、顶层键值对（`INI102`）、重复键（`INI103`）在降级阶段报告。
- `IniWriter`：`[Name]`、`[Name]:[Base1],[Base2]`、每行 `Key=Value`；按组输出 `{组名}.ini`；
  `TreeShaking` 只依据 `CompilationModel.ReachableEntities`（分析阶段给出）；`PreserveComments` 暂不支持。

### 4.1 预处理器（`Ra2lyn.Ini.Directives`）

在词法与语法之间执行（`IniPreprocessor`），按宏集合剔除未生效内容后再交给 `IniParser`：

| 指令 | 含义 |
|---|---|
| `#if 表达式` / `#elif 表达式` / `#else` / `#endif` | 条件编译 |
| `#region` / `#endregion` | 命名区域，只参与结构配对，不改变是否生效 |

- 结构语义与 C# 一致：条件段与区域共用分层栈，只能打开或关闭栈顶同类结构，
  因此 `#region` 不能跨越条件段边界，条件段也不能跨越区域边界。
- 表达式只支持布尔运算：标识符、`!`、`&&`、`||` 与括号，优先级 `!` → `&&` → `||`。
  比较、算术与字面量都报告 `INI013`。
- 宏名大小写敏感：`--defines=A,B,C` 时 `A`/`B`/`C` 为真，其余一律为假。
- 诊断：`INI011`（未知指令）、`INI012`（结构不成立）、`INI013`（表达式错误）。
- 未生效部分的指令仍参与配对（未生效的 `#if` 内部必须有 `#endif`），与 C# 行为一致。
- 指令行本身不进入语法分析；`#` 后紧跟字母才是指令，其余 `#` 仍是行注释。

## 5. 消费者

- **CLI**：`Shimakaze.Sdk.Engine.Cli` 的 `ra2lyn build`（`ra2lyn` 可缩写为 `ini`）。
  用法：`ra2lyn build --input a.ini [--input b.ini ...] [--rules x.xml ...] [--output dir] [--group g] [--defines A,B,C] [--tree-shaking]`。
- **MSBuild**：`Shimakaze.Sdk.Ra2lyn.props` / `.targets` 收集输入，并调用上述 CLI 完成编译。
- 门面：`Shimakaze.Sdk.Engine.Ra2lyn.Ra2lynCompiler`，只使用前端、内核与分析层的公开类型。

## 6. 扩展点

- 新的源语言目标（SHP 等）：新增 `Ra2lyn.<Source>` 项目。Common 保持与源语言无关，
  若目标与某个源语言共用一个写入器，则放在该源语言项目内。
- 预处理：`#if` 系列与 `#region` 已实现；`#include` / `$()` 变量替换仍未实现，预留位置同上。
- 索引 / 缓存：放到独立 `Ra2lyn.Index` 项目，内核不得引用。
- LSP：独立 `Ra2lyn.Lsp`，只做 `Diagnostic` → LSP 映射，`Range` 由 `SourceFile` 计算。

## 7. 已知限制

- 注释不往返：AST 保留自由记号，但 IR 与发射丢弃注释。
- 多文件合并顺序由调用方给出，不按文件名排序。
- 规则校验只覆盖普通实体；注册表 / 全局 / 枚举节由各自语义规则处理。

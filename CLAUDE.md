# HUDInfo

## 项目概述

SCP: Secret Laboratory 的 HUD 信息显示扩展插件，基于 **LabAPI** 框架（不是 EXILED）。
提供 SCP-914 激活提示、电梯使用提示、阵营剩余人数显示、NTF/混沌重生倒计时（仅观察者可见）。

## 文件结构

```
HUDInfo/
├── HUDInfo.cs           # 插件主体：HUDInfo 类（生命周期/事件）+ PlayerHud 类（per-player HUD）
├── Config.cs            # HUDInfoConfig — 所有可配置的开关/坐标/字体/时长/间隔
├── Translations.cs      # HUDTranslation — 阵营名称/颜色/消息模板
├── Properties/
│   └── AssemblyInfo.cs  # 程序集版本（必须与 Plugin.Version 保持一致，见版本同步）
├── HUDInfo.csproj       # .NET 4.8.1 旧式项目，引用 NuGet 包和 using/ 下的游戏 DLL
├── packages.config      # NuGet 包版本声明
├── app.config           # YamlDotNet 的 bindingRedirect
└── using/               # 游戏原生 DLL（不入 git，需从服务器手动复制）
```

## 构建

1. 将服务器目录下的以下文件复制到 `using/`：
   `Assembly-CSharp.dll`、`Assembly-CSharp-firstpass.dll`、`Mirror.dll`、
   `Pooling.dll`、`UnityEngine.dll`、`UnityEngine.CoreModule.dll`
2. Visual Studio 2022 打开 `HUDInfo.sln`，NuGet 自动还原包
3. 选 `Release` 配置 → 生成
4. 产物：`bin/Release/HUDInfo.dll`

命令行构建（Git Bash 下必须用单杠 `-p:`/`-t:`，不要传 `-p:Platform=x64`）：
```bash
MSBUILD="D:/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe"
"$MSBUILD" HUDInfo.sln -p:Configuration=Release -t:Rebuild
```

## 关键设计

### _huds 生命周期（per-player 缓存审计）

`_huds: Dictionary<Player, PlayerHud>` 是插件唯一的 per-player 缓存。

| 事件 | 操作 | 说明 |
|---|---|---|
| `PlayerEvents.Joined` | 创建条目 | `OnPlayerJoin` 初始化 `PlayerHud` 并注册所有 Hint |
| `PlayerEvents.Left` | 删除条目 | `OnPlayerLeft` 调用 `Dispose()`，杀协程、移除 Hint |
| `PlayerEvents.ChangedRole` | 更新条目 | 调整倒计时 Hint 的显示/隐藏状态 |
| `ServerEvents.RoundStarted` | 无操作 | 预留钩子，HUD 由各自协程自行刷新 |
| 回合结束 | **不清理** | `_huds` 是每个连接的 UI 对象，跨回合应保留 |

> ⚠️ 不要在回合结束时清空 `_huds`。这与 DDSurrender 的 `_factionStates`（每回合状态）
> 性质不同，切勿照搬"回合结束清缓存"的模式。

### PlayerHud 内部结构

每个 `PlayerHud` 持有：
- 7 个 `Hint` 对象（914、阵营、NTF大波、NTF小波、混沌大波、混沌小波、电梯）
- 2 个 `CoroutineHandle`（`_factionCoroutine`、`_respawnCoroutine`）
- `PlayerDisplay _display` — HintServiceMeow 渲染句柄（Init 时通过 `PlayerDisplay.Get(_pl)` 获取并缓存）

`Dispose()` 执行三步：`Timing.KillCoroutines(...)` → `_display.RemoveHint(_hints)` → `_hints.Clear()`。
不要只设置 `Hide=true`，那样协程仍在运行。

### Hints* 平行结构体（刻意保留的决策）

`Templates`/`HintsTranslations`/`HintsConfigs`/`HintsBase` 这四个结构体把 `HUDInfoConfig`/`HUDTranslation`
的字段复制一遍，`Enable()` 里有约 35 行手动拷贝。**这是刻意保留的**，不是技术债：

- 好处：`PlayerHud` 不直接依赖配置 schema，形成一层解耦
- 代价：每新增配置字段需手动在拷贝块里多加一行
- 结论：合并的收益（省拷贝代码）小于风险（强耦合），对线上插件不值得做结构性改动

如果将来真的要合并，请把 `PlayerHud.Init()` 的签名从 `(HUDInfo.HintsBase, HUDInfo.HintsTranslations)`
改为直接接受 `HUDInfoConfig` 和 `HUDTranslation`，删掉四个结构体和手动拷贝块。

### 版本号同步规则（发布前必查）

`Properties/AssemblyInfo.cs` 中的 `AssemblyVersion`/`AssemblyFileVersion`
**必须与 `HUDInfo.cs` 的 `Plugin.Version new Version(X, Y, Z)` 保持一致**。

| 文件 | 字段 | 当前值 |
|---|---|---|
| `HUDInfo.cs` | `Plugin.Version` | `new Version(2, 0, 0)` |
| `AssemblyInfo.cs` | `AssemblyVersion` | `2.0.0.0` |
| `AssemblyInfo.cs` | `AssemblyFileVersion` | `2.0.0.0` |
| `README.md` | 版本历史表 | `v2.0.0` |

## 版本对应

| 插件版本 | LabAPI | SCP:SL | 说明 |
|----------|--------|--------|------|
| v2.0.0 | 1.1.7 | 14.x | 当前版本，修复泄漏/越界/顺序等bug |

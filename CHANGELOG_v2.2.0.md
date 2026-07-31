# HUDInfo v2.2.0 更新日志

发布日期：2026-07-31

## 重大变更

**⚠️ 此版本配置文件格式完全重构，不兼容旧版本！**

首次启动 v2.2.0 时，插件会自动生成新格式的配置文件，请根据需要重新调整配置。

---

## Bug 修复

### 1. 电梯显示逻辑修复
**问题**：玩家多次按电梯召唤按钮时，会重复显示"电梯已召唤"提示，即使电梯已在移动中。

**修复**：
- 添加了 `ev.Elevator.IsReady` 状态检查
- 只有当电梯处于就绪状态（未移动）时才触发提示
- 避免了玩家重复按按钮造成的重复显示

**技术细节**：使用 LabAPI 的 `Elevator.IsReady` 属性判断电梯状态，确保时序安全。

### 2. 电梯倒计时不显示修复
**问题**：电梯提示模板中的 `{sec}` 占位符被提前替换，导致实时倒计时功能失效。

**修复**：
- 在 `OnInteractingElevator` 中只替换 `{p_operator}` 占位符
- 保留 `{sec}` 占位符传递给协程，由 `UpdateElevator` 动态更新秒数
- 现在电梯提示会正确显示实时倒计时（如 `[7秒]` → `[6秒]` → ...）

---

## 配置文件重构

### 新配置结构
采用嵌套类组织配置，更清晰易读：

```yaml
# 全局开关
Enable914Hint: true
EnableElevatorHint: true
EnableFactionCount: true
EnableRespawnTimer: true
UpdateInterval: 1.0

# SCP-914 配置
Scp914:
  X: 0
  Y: 50
  FontSize: 25
  Duration: 15.0

# 电梯配置
Elevator:
  X: 0
  Y: 850
  FontSize: 22
  Duration: 7.0
  Range: 10.0

# 阵营人数配置
FactionCount:
  X: 750
  Y: 950
  FontSize: 26

# 重生倒计时配置
RespawnTimer:
  NtfPrimary:
    X: -420
    Y: 100
    FontSize: 26
  NtfMini:
    X: -420
    Y: 135
    FontSize: 24
  ChaosPrimary:
    X: 420
    Y: 100
    FontSize: 26
  ChaosMini:
    X: 420
    Y: 135
    FontSize: 24
```

### 配置改进
- **分组清晰**：相关配置集中在同一对象下
- **命名统一**：使用 PascalCase 命名风格
- **类型安全**：通过嵌套类提供更好的类型检查
- **易于扩展**：新增功能时只需添加新的配置对象

---

## 优化改进

### 1. 默认模板优化
采用对称美学风格，使用优雅的装饰符号和清晰的信息层次：

**SCP-914 提示**：
- 旧：`[Scp914] 已启动! 模式: {mode}, 操作人: {p_operator}`
- 新：`「 SCP-914 」档位: {mode} · 操作者: {p_operator}`

**电梯提示**：
- 旧：`[{sec}] 电梯使用者: {p_operator}`
- 新：`『 电梯 』{p_operator} 召唤中 · 剩余 {sec}秒`

**阵营人数**：
- 旧：`{name}: {count}`
- 新：`◈ {name} ━ {count}`

**重生倒计时**：
- 旧：`下一次刷新时间: {Time}`
- 新：`◆ 九尾刷新 ▸ {Time}` / `◇ 九尾小波 ▸ {Time}` / `◆ 混沌刷新 ▸ {Time}` / `◇ 混沌小波 ▸ {Time}`
- 使用实心◆和空心◇菱形区分大小波

### 2. 默认坐标调整

| 功能 | 旧坐标 | 新坐标 | 说明 |
|------|--------|--------|------|
| SCP-914 提示 | Y=80 | Y=50 | 更靠近屏幕顶部 |
| 阵营人数 | X=800, Y=900 | X=750, Y=950 | 移至右下角 |
| 电梯提示 | Y=800 | Y=850 | 略微下移避免遮挡 |

### 3. 默认字号调整
- **914 提示**：20 → 25（更醒目）
- **电梯提示**：20 → 22（略微增大）
- **阵营人数**：30 → 26（减小避免占用过多空间）
- **重生倒计时**：30 → 26（统一字号，简洁一致）

### 4. 代码质量改进
- **配置验证**：启动时检查翻译字典完整性，避免运行时空引用
- **代码简化**：移除中间数据结构，直接使用配置对象
- **可读性**：重命名变量使用更清晰的命名（如 `_config`、`_translation`）
- **注释完善**：添加更详细的功能说明和技术细节注释

---

## 技术细节

### 新增配置类
- `HintDisplayConfig`：基础 Hint 显示配置
- `ElevatorHintConfig`：电梯专用配置（继承基础配置 + Range）
- `RespawnTimerConfig`：重生倒计时分组配置

### 风险评估
1. **电梯状态判断**：使用 `Elevator.IsReady` 属性，时序安全
2. **Unicode 符号**：使用通用符号（● ▸ ◆），Unity TextMeshPro 广泛支持

### 依赖版本
- LabAPI：1.1.7+
- HintServiceMeow：5.5.0
- SCP:SL：14.x

---

## 升级指南

### 从 v2.1.0 升级到 v2.2.0

1. **备份旧配置**（可选）
   ```bash
   cp config.yml config.yml.backup
   cp translations.yml translations.yml.backup
   ```

2. **替换插件文件**
   - 将 `HUDInfo.dll` 替换到 `LabAPI/plugins/` 目录

3. **删除旧配置文件**
   ```bash
   rm config.yml
   rm translations.yml
   ```

4. **重启服务器**
   - 插件会自动生成新格式的配置文件

5. **调整配置**
   - 根据需要修改新生成的 `config.yml` 和 `translations.yml`
   - 参考本文档的"新配置结构"部分

### 主要配置项对照表

| v2.1.0 | v2.2.0 |
|--------|--------|
| `info_914` | `Enable914Hint` |
| `info_elevator` | `EnableElevatorHint` |
| `info_faction` | `EnableFactionCount` |
| `info_timer` | `EnableRespawnTimer` |
| `info_update_interval` | `UpdateInterval` |
| `_914_x` | `Scp914.X` |
| `_914_y` | `Scp914.Y` |
| `_914_font` | `Scp914.FontSize` |
| `_914_duration` | `Scp914.Duration` |
| `elev_range` | `Elevator.Range` |
| `f_template` | `FactionTemplate` |
| `elev_template` | `ElevatorTemplate` |
| `scp914_template` | `Scp914Template` |
| `unknown_operator` | `UnknownOperator` |
| `scp914_trans` | `Scp914Modes` |

---

## 已知问题

无

---

## 下一步计划

- 考虑添加更多自定义选项（如渐变色支持）
- 探索性能优化空间
- 收集用户反馈进一步完善

---

**反馈渠道**：
- GitHub Issues: https://github.com/whystars/HUDInfo/issues
- 贡献者：感谢所有使用和反馈的服务器管理员

# HUDInfo

![LabAPI](https://img.shields.io/badge/LabAPI-1.1.7+-blue)
![Version](https://img.shields.io/badge/Version-2.1.0-green)
![SCP:SL](https://img.shields.io/badge/SCP%3ASL-14.x-orange)
![License](https://img.shields.io/badge/License-GPL--3.0-red)

SCP: Secret Laboratory 服务器的 **HUD信息显示扩展** 插件，基于 LabAPI 框架开发。

## 功能

- **SCP-914 激活提示**：显示谁激活了 SCP-914 以及操作模式，对914房间内的玩家可见
- **电梯使用提示**：支持多条同时显示（最多3条），每条带实时倒计时，各自到期后自动消失
- **阵营剩余人数显示**：根据玩家当前阵营，显示对立双方的剩余人数
  - 九尾/科学家阵营：显示九尾和科学家剩余人数
  - 混沌/D级阵营：显示混沌和D级剩余人数
  - SCP：显示SCP剩余人数
- **重生波倒计时**：显示 NTF 和混沌的大波/小波剩余重生时间，仅**观察者**可见

## 安装

1. 将 `HUDInfo.dll` 放入服务器的 LabAPI 插件目录（通常为 `LabAPI/plugins/`）
2. 确保 `HintServiceMeow.dll` 也已安装在同一目录（本插件的必需依赖）
3. 启动服务器，插件会自动生成 `config.yml` 和 `translations.yml`
4. 按需修改配置文件后重启服务器

## 配置项（config.yml）

### 开关

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `info_914` | `true` | 是否显示 SCP-914 提示信息 |
| `info_elevator` | `true` | 是否显示电梯提示信息 |
| `info_faction` | `true` | 是否显示阵营剩余人数 |
| `info_timer` | `true` | 是否显示重生时间计时器 |
| `info_update_interval` | `1.0` | 阵营人数/重生倒计时刷新间隔（秒） |

### SCP-914 提示位置

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `_914_x` | `0` | X 轴坐标（0 为正中，负为左，正为右） |
| `_914_y` | `80` | Y 轴坐标（屏幕上端约100，下端约1000） |
| `_914_font` | `20` | 字体大小 |
| `_914_duration` | `15.0` | 提示持续时间（秒） |

### 阵营剩余人数显示位置

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `_faction_x` | `800` | X 轴坐标 |
| `_faction_y` | `900` | Y 轴坐标 |
| `_faction_font` | `30` | 字体大小 |

### 重生波倒计时位置（仅观察者可见）

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `_ntfResp_x` | `-400` | NTF 大波 X 轴坐标 |
| `_ntfResp_y` | `120` | NTF 大波 Y 轴坐标 |
| `_ntfResp_font` | `30` | NTF 大波字体大小 |
| `_ntfmini_x` | `-480` | NTF 小波 X 轴坐标 |
| `_ntfmini_y` | `170` | NTF 小波 Y 轴坐标 |
| `_ntfmini_font` | `30` | NTF 小波字体大小 |
| `_CiResp_x` | `400` | 混沌大波 X 轴坐标 |
| `_CiResp_y` | `120` | 混沌大波 Y 轴坐标 |
| `_CiResp_font` | `30` | 混沌大波字体大小 |
| `_Cimini_x` | `480` | 混沌小波 X 轴坐标 |
| `_Cimini_y` | `170` | 混沌小波 Y 轴坐标 |
| `_Cimini_font` | `30` | 混沌小波字体大小 |

### 电梯提示位置

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `_elev_x` | `0` | X 轴坐标 |
| `_elev_y` | `800` | Y 轴坐标 |
| `_elev_font` | `20` | 字体大小 |
| `_elev_duration` | `7.0` | 提示持续时间（秒） |
| `elev_range` | `10.0` | 可见范围半径（以操作者为中心，游戏单位） |

## 翻译自定义（translations.yml）

服务器可在 `translations.yml` 中自定义阵营名称、颜色和消息模板，首次运行会自动生成默认值。

模板支持以下占位符：

| 占位符 | 用途 |
|--------|------|
| `{name}` | 阵营名称（来自 `teamNames` 字典） |
| `{color}` | 阵营颜色（来自 `teamColors` 字典，HEX格式如 `#FF0000`） |
| `{count}` | 阵营剩余人数 |
| `{p_operator}` | 操作者昵称（获取失败时显示 `unknown_operator` 配置的文案） |
| `{sec}` | 电梯提示的剩余秒数倒计时（仅 `elev_template` 使用） |
| `{Time}` | 重生倒计时秒数 |
| `{mode}` | SCP-914 操作模式（来自 `scp914_trans` 字典） |

## 编译

1. 安装 Visual Studio 2022
2. 克隆仓库：
   ```bash
   git clone https://github.com/whystars/HUDInfo.git
   ```
3. 用 Visual Studio 打开 `HUDInfo.sln`，NuGet 自动还原 `Northwood.LabAPI` 和 `HintServiceMeow` 等包
4. 将服务器目录下的以下文件复制到项目 `using/` 目录：
   `Assembly-CSharp.dll`、`Assembly-CSharp-firstpass.dll`、`Mirror.dll`、`Pooling.dll`、
   `UnityEngine.dll`、`UnityEngine.CoreModule.dll`
5. 选择 `Release` 配置，生成项目
6. 产物位于 `bin/Release/HUDInfo.dll`

## 版本历史

| 版本 | LabAPI | SCP:SL | 说明 |
|------|--------|--------|------|
| v2.1.0 | 1.1.7 | 14.x | 电梯提示支持多条目实时倒计时（最多3条，模板新增 `{sec}` 占位符） |
| v2.0.0 | 1.1.7 | 14.x | 修复协程/内存泄漏、字典越界、空检查顺序等bug；清理死代码；添加显示时长/轮询间隔/兜底文案等可配置项 |

## 许可证

本项目基于 GPL-3.0 许可证开源，详见 [LICENSE](LICENSE)。

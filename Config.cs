using System.ComponentModel;

namespace HUDInfo;

/// <summary>
/// HUDInfo 插件配置
/// </summary>
public class HUDInfoConfig
{
    [Description("是否启用 SCP-914 激活提示")]
    public bool Enable914Hint { get; set; } = true;

    [Description("是否启用电梯召唤提示")]
    public bool EnableElevatorHint { get; set; } = true;

    [Description("是否启用阵营剩余人数显示")]
    public bool EnableFactionCount { get; set; } = true;

    [Description("是否启用重生倒计时显示")]
    public bool EnableRespawnTimer { get; set; } = true;

    [Description("阵营人数和重生倒计时的刷新间隔（秒），越小越实时但服务器开销略高")]
    public float UpdateInterval { get; set; } = 1f;

    [Description("SCP-914 激活提示配置")]
    public HintDisplayConfig Scp914 { get; set; } = new()
    {
        X = 0,
        Y = 50,
        FontSize = 25,
        Duration = 15f
    };

    [Description("电梯召唤提示配置")]
    public ElevatorHintConfig Elevator { get; set; } = new()
    {
        X = 0,
        Y = 850,
        FontSize = 22,
        Duration = 7f,
        Range = 10f
    };

    [Description("阵营剩余人数显示配置")]
    public HintDisplayConfig FactionCount { get; set; } = new()
    {
        X = 750,
        Y = 950,
        FontSize = 26
    };

    [Description("重生倒计时显示配置")]
    public RespawnTimerConfig RespawnTimer { get; set; } = new();
}

/// <summary>
/// Hint 显示基础配置
/// </summary>
public class HintDisplayConfig
{
    [Description("X 轴坐标（0为正中，负为左，正为右）")]
    public float X { get; set; }

    [Description("Y 轴坐标（屏幕上端约100，下端约1000）")]
    public float Y { get; set; }

    [Description("字体大小")]
    public int FontSize { get; set; }

    [Description("提示持续时间（秒）")]
    public float Duration { get; set; }
}

/// <summary>
/// 电梯提示配置（继承基础配置并添加可见范围）
/// </summary>
public class ElevatorHintConfig : HintDisplayConfig
{
    [Description("提示可见范围半径（以操作者为中心，游戏单位）")]
    public float Range { get; set; }
}

/// <summary>
/// 重生倒计时配置
/// </summary>
public class RespawnTimerConfig
{
    [Description("九尾大波刷新显示配置")]
    public HintDisplayConfig NtfPrimary { get; set; } = new()
    {
        X = -420,
        Y = 100,
        FontSize = 26
    };

    [Description("九尾小波刷新显示配置")]
    public HintDisplayConfig NtfMini { get; set; } = new()
    {
        X = -420,
        Y = 135,
        FontSize = 24
    };

    [Description("混沌大波刷新显示配置")]
    public HintDisplayConfig ChaosPrimary { get; set; } = new()
    {
        X = 420,
        Y = 100,
        FontSize = 26
    };

    [Description("混沌小波刷新显示配置")]
    public HintDisplayConfig ChaosMini { get; set; } = new()
    {
        X = 420,
        Y = 135,
        FontSize = 24
    };
}

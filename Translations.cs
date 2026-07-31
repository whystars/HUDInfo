using PlayerRoles;
using Scp914;
using System.Collections.Generic;
using System.ComponentModel;

namespace HUDInfo;

/// <summary>
/// HUDInfo 翻译配置
/// </summary>
public class HUDTranslation
{
    [Description("各阵营名称显示")]
    public Dictionary<Team, string> TeamNames { get; set; } = new()
    {
        { Team.SCPs, "SCP" },
        { Team.ChaosInsurgency, "混沌" },
        { Team.FoundationForces, "九尾" },
        { Team.ClassD, "D级" },
        { Team.Scientists, "科学家" }
    };

    [Description("各阵营颜色（HEX格式）")]
    public Dictionary<Team, string> TeamColors { get; set; } = new()
    {
        { Team.SCPs, "#FF0000" },
        { Team.ChaosInsurgency, "#008F1C" },
        { Team.FoundationForces, "#00BFFF" },
        { Team.ClassD, "#FF8C00" },
        { Team.Scientists, "#FEFE7A" }
    };

    [Description("阵营剩余人数显示模板（{color}=阵营颜色, {name}=阵营名称, {count}=剩余人数）")]
    public string FactionTemplate { get; set; } = "<color={color}>● {name}</color> <size=28>{count}</size>";

    [Description("电梯召唤显示模板（{sec}=剩余秒数倒计时, {p_operator}=操作者昵称）")]
    public string ElevatorTemplate { get; set; } = "▸ 电梯已召唤 <size=20>[{sec}秒]</size> ▸\n<size=18><color=#B952FA>{p_operator}</color></size>";

    [Description("九尾大波重生倒计时模板（{Time}=重生时间）")]
    public string NtfRespawnTemplate { get; set; } = "<color=#00BFFF>▸ 九尾刷新</color>\n<size=24>{Time}</size>";

    [Description("九尾小波重生倒计时模板（{Time}=重生时间）")]
    public string NtfMiniRespawnTemplate { get; set; } = "<color=#00BFFF>▸ 小波</color>\n<size=22>{Time}</size>";

    [Description("混沌大波重生倒计时模板（{Time}=重生时间）")]
    public string CiRespawnTemplate { get; set; } = "<color=#008F1C>▸ 混沌刷新</color>\n<size=24>{Time}</size>";

    [Description("混沌小波重生倒计时模板（{Time}=重生时间）")]
    public string CiMiniRespawnTemplate { get; set; } = "<color=#008F1C>▸ 小波</color>\n<size=22>{Time}</size>";

    [Description("SCP-914 激活显示模板（{mode}=操作模式, {p_operator}=操作者昵称）")]
    public string Scp914Template { get; set; } = "◆ SCP-914 已启动 ◆\n<size=20>档位: <color=#F7C73E>{mode}</color> | 操作者: <color=#0080FF>{p_operator}</color></size>";

    [Description("无法获取操作者名称时的兜底文案")]
    public string UnknownOperator { get; set; } = "未知";

    [Description("SCP-914 各档位翻译")]
    public Dictionary<Scp914KnobSetting, string> Scp914Modes { get; set; } = new()
    {
        { Scp914KnobSetting.Rough, "粗加工" },
        { Scp914KnobSetting.Coarse, "半粗加工" },
        { Scp914KnobSetting.OneToOne, "1:1" },
        { Scp914KnobSetting.Fine, "精加工" },
        { Scp914KnobSetting.VeryFine, "超精加工" }
    };
}

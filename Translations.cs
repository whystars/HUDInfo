using LabApi.Loader.Features.Plugins;
using PlayerRoles;
using Scp914;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HUDInfo.HUDInfo;

namespace HUDInfo;

public class HUDTranslation
{
    [Description("各个阵营剩余人数显示, 内容:")]
    public Dictionary<Team, string> teamNames { get; set; } = new Dictionary<Team, string>()
    {
        { Team.SCPs, "SCP剩余" },
        { Team.ChaosInsurgency, "混沌剩余" },
        { Team.FoundationForces, "九尾剩余" },
        { Team.ClassD, "DD剩余" },
        { Team.Scientists, "科学家剩余" }
    };

    [Description("各个阵营剩余人数显示, 颜色:")]
    public Dictionary<Team, string> teamColors { get; set; } = new Dictionary<Team, string>()
    { 
        { Team.SCPs, "#FF0000" },
        { Team.ChaosInsurgency, "#008F1C" },
        { Team.FoundationForces, "#00BFFF" },
        { Team.ClassD, "#FF8C00" },
        { Team.Scientists, "#FEFE7A" }
    };

    [Description("阵营剩余人数显示模板({color}为当前阵营对应的颜色, {name}是当前对应阵营的显示内容, {count}为当前阵营剩余人数):")]
    public string f_template { get; set; } = "<color={color}>{name}</color>: {count}";

    [Description("电梯显示模板(HEX color写死, {p_operator}表示操作人, 没有的话自动为未知):")]
    public string elev_template { get; set; } = "[Elevator] 电梯使用者: <color=#B952FA>{p_operator}</color>";

    [Description("NTF普通波重生时间显示模板(HEX color写死, {Time}表示重生时间):")]
    public string ntf_respawn_template { get; set; } = "<color=#00BFFF>下一次刷新时间:</color> {Time}";

    [Description("NTF小波重生时间显示模板(HEX color写死, {Time}表示重生时间):")]
    public string ntf_mini_respawn_template { get; set; } = "<color=#00BFFF>迷你波:</color> {Time}";

    [Description("ChaosInsurgency普通波重生时间显示模板(HEX color写死, {Time}表示重生时间):")]
    public string ci_respawn_template { get; set; } = "<color=#008F1C>下一次刷新时间:</color> {Time}";

    [Description("ChaosInsurgency小波重生时间显示模板(HEX color写死, {Time}表示重生时间):")]
    public string ci_mini_respawn_template { get; set; } = "<color=#008F1C>迷你波:</color> {Time}";

    [Description("SCP914显示模板(HEX color写死, {mode}表示操作模式, {p_operator}表示操作人, 没有的话自动为未知):")]
    public string scp914_template { get; set; } = "[Scp914] 已启动! 模式: <color=#F7C73E>{mode}</color>, 操作人: <color=#0080FF>{p_operator}</color>";

    [Description("无法获取操作者名称时显示的兜底文案:")]
    public string unknown_operator { get; set; } = "未知";

    [Description("SCP914, Rough模式翻译:")]
    public Dictionary<Scp914KnobSetting, string> scp914_trans { get; set; } = new Dictionary<Scp914KnobSetting, string>()
    {
        { Scp914KnobSetting.Rough, "粗加" },
        { Scp914KnobSetting.Coarse, "半粗" },
        { Scp914KnobSetting.OneToOne, "1:1" },
        { Scp914KnobSetting.Fine, "精加" },
        { Scp914KnobSetting.VeryFine, "超精加工" }
    };
}

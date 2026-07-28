using LabApi.Loader.Features.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HUDInfo.HUDInfo;

namespace HUDInfo;

public class HUDInfoConfig
{
    [Description("是否显示SCP914提示信息")]
    public bool info_914 { get; set; } = true;

    [Description("是否显示电梯提示信息")]
    public bool info_elevator { get; set; } = true;

    [Description("是否显示阵营剩余人数")]
    public bool info_faction { get; set; } = true;

    [Description("是否显示重生时间计时器")]
    public bool info_timer { get;set; } = true;

    [Description("914显示,X轴坐标(0为正中,-为左,+为右):")]
    public float _914_x { get; set; } = 0;
    [Description("914显示,Y轴坐标(屏幕最上端大概100,最下端大概1000):")]
    public float _914_y { get; set; } = 80;
    [Description("914显示,字体大小:")]
    public int _914_font { get; set; } = 20;
    [Description("914显示,提示持续时间(秒):")]
    public float _914_duration { get; set; } = 15f;

    [Description("阵营剩余显示,X轴坐标(0为正中,-为左,+为右):")]
    public float _faction_x { get; set; } = 800;
    [Description("阵营剩余显示,Y轴坐标(屏幕最上端大概100,最下端大概1000):")]
    public float _faction_y { get; set; } = 900;
    [Description("阵营剩余显示,字体大小:")]
    public int _faction_font { get; set; } = 30;

    [Description("重生时间显示,九尾普通刷新波(大波),X轴坐标(0为正中,-为左,+为右):")]
    public float _ntfResp_x { get; set; } = -400;
    [Description("重生时间显示,九尾普通刷新波(大波),Y轴坐标(屏幕最上端大概100,最下端大概1000):")]
    public float _ntfResp_y { get; set; } = 120;
    [Description("重生时间显示,九尾普通刷新波(大波),字体大小:")]
    public int _ntfResp_font { get; set; } = 30;

    [Description("重生时间显示,九尾迷你刷新波(小波),X轴坐标(0为正中,-为左,+为右):")]
    public float _ntfmini_x { get; set; } = -480;
    [Description("重生时间显示,九尾迷你刷新波(小波),Y轴坐标(屏幕最上端大概100,最下端大概1000):")]
    public float _ntfmini_y { get; set; } = 170;
    [Description("重生时间显示,九尾迷你刷新波(小波),字体大小:")]
    public int _ntfmini_font { get; set; } = 30;

    [Description("重生时间显示,混沌普通刷新波(大波),X轴坐标(0为正中,-为左,+为右):")]
    public float _CiResp_x { get; set; } = 400;
    [Description("重生时间显示,混沌普通刷新波(大波),Y轴坐标(屏幕最上端大概100,最下端大概1000):")]
    public float _CiResp_y { get; set; } = 120;
    [Description("重生时间显示,混沌普通刷新波(大波),字体大小:")]
    public int _CiResp_font { get; set; } = 30;

    [Description("重生时间显示,混沌迷你刷新波(小波),X轴坐标(0为正中,-为左,+为右):")]
    public float _Cimini_x { get; set; } = 480;
    [Description("重生时间显示,混沌迷你刷新波(小波),Y轴坐标(屏幕最上端大概100,最下端大概1000):")]
    public float _Cimini_y { get; set; } = 170;
    [Description("重生时间显示,混沌迷你刷新波(小波),字体大小:")]
    public int _Cimini_font { get; set; } = 30;

    [Description("电梯显示,X轴坐标(0为正中,-为左,+为右):")]
    public float _elev_x { get; set; } = 0;
    [Description("电梯显示,Y轴坐标(屏幕最上端大概100,最下端大概1000):")]
    public float _elev_y { get; set; } = 800;
    [Description("电梯显示,字体大小:")]
    public int _elev_font { get; set; } = 20;
    [Description("电梯显示,提示持续时间(秒):")]
    public float _elev_duration { get; set; } = 7f;

    [Description("电梯提示显示可见范围（操作者为中心）:")]
    public float elev_range { get; set; } = 10f;

    [Description("阵营人数/重生倒计时的刷新间隔(秒), 越小越实时但服务器开销略高:")]
    public float info_update_interval { get; set; } = 1f;
}

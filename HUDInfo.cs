// HUDInfo.cs
using Achievements.Handlers;
using GameCore;
using Hints;
using HintServiceMeow;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI;
using Interactables.Interobjects;
using InventorySystem.Items.Coin;
using LabApi;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Configuration;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MapGeneration;
using MEC;
using PlayerRoles;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Xml.Linq;
using UnityEngine;
using YamlDotNet.Core.Tokens;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;
using Logger = LabApi.Features.Console.Logger;
using Vector3 = UnityEngine.Vector3;
using Version = System.Version;

namespace HUDInfo;

public class HUDInfo : Plugin<HUDInfoConfig>
{
    public static HUDInfo Instance { get; set; } = null!;

    private readonly Dictionary<Player, PlayerHud> _huds = new();

    private HUDInfoConfig infoconfig;  //插件配置
    private HUDTranslation hudtranslations; //翻译信息

    private bool _hasIncorrectSettings = false;
    private bool _hasIncorrectTraslation = false;

    public struct Templates
    {
        public string faction, scp914, elevator, ntf, ntfmini, ci, cimini;
    }

    public struct HintsTranslations
    {
        public Dictionary<Team, string> teamNames;
        public Dictionary<Team, string> teamColors;
        public Dictionary<Scp914KnobSetting, string> scp914knobtranslations;
        public Templates templates;
    }

    public struct HintsConfigs
    {
        public float x, y;
        public int font;
    }

    public struct HintsBase
    {
        public HintsConfigs i_914, i_faction, i_elevator, i_ntf, i_ntfmini, i_ci, i_cimini;
        public bool i_info_timer, i_info_faction, i_info_914, i_info_elevator;
        public float _elev_range;
    }

    private HintsBase hintsBase; //数据文件
    private HintsTranslations hintsTranslations; //翻译文件

    public override LoadPriority Priority { get; } = LoadPriority.High;// 插件加载优先级

    public override string Name { get; } = "HUDInfo"; // 插件名称

    public override string Description { get; } = "一个优秀的信息显示拓展插件!"; // 插件描述

    public override string Author { get; } = "Crystal";// 插件作者

    public override Version Version { get; } = new Version(2, 0, 0); // 插件版本

    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion); // 插件依赖的 API 版本

    public override void LoadConfigs()  //读取配置文件
    {
        _hasIncorrectSettings = !this.TryLoadConfig("config.yml", out infoconfig);
        _hasIncorrectTraslation = !this.TryLoadConfig("translations.yml", out hudtranslations);

        base.LoadConfigs();
    }

    public override void Enable()
    {
        Instance = this;

        PlayerEvents.Joined += OnPlayerJoin;
        PlayerEvents.ChangedRole += OnRoleChanged;
        PlayerEvents.InteractingElevator += OnInteractingElevator;
        ServerEvents.RoundStarted += OnRoundStart;
        Scp914Events.Activating += On914Activating;

        if (_hasIncorrectSettings || infoconfig == null)
        {
            Logger.Error($"{Name} 配置文件加载失败，请检查 config.yml 格式或删除后重启！");
            return;
        }

        if (_hasIncorrectTraslation || hudtranslations == null)
        {
            Logger.Error($"{Name} 翻译文件加载失败，请检查 translations.yml 格式或删除后重启！");
            return;
        }

        Logger.Info($"{Name} 插件加载成功! v{Version} by {Author} - {Description}");

        //加载配置到本地
        hintsBase.i_info_914 = infoconfig.info_914;
        hintsBase.i_info_elevator = infoconfig.info_elevator;
        hintsBase.i_info_timer = infoconfig.info_timer;
        hintsBase.i_info_faction = infoconfig.info_faction;

        hintsBase.i_914.x = infoconfig._914_x;
        hintsBase.i_914.y = infoconfig._914_y;
        hintsBase.i_914.font = infoconfig._914_font;

        hintsBase.i_elevator.x = infoconfig._elev_x;
        hintsBase.i_elevator.y = infoconfig._elev_y;
        hintsBase.i_elevator.font = infoconfig._elev_font;

        hintsBase.i_ntf.x = infoconfig._ntfResp_x;
        hintsBase.i_ntf.y = infoconfig._ntfResp_y;
        hintsBase.i_ntf.font = infoconfig._ntfResp_font;

        hintsBase.i_ntfmini.x = infoconfig._ntfmini_x;
        hintsBase.i_ntfmini.y = infoconfig._ntfmini_y;
        hintsBase.i_ntfmini.font = infoconfig._ntfmini_font;

        hintsBase.i_ci.x = infoconfig._CiResp_x;
        hintsBase.i_ci.y = infoconfig._CiResp_y;
        hintsBase.i_ci.font = infoconfig._CiResp_font;

        hintsBase.i_cimini.x = infoconfig._Cimini_x;
        hintsBase.i_cimini.y = infoconfig._Cimini_y;
        hintsBase.i_cimini.font = infoconfig._Cimini_font;

        hintsBase.i_faction.x = infoconfig._faction_x;
        hintsBase.i_faction.y = infoconfig._faction_y;
        hintsBase.i_faction.font = infoconfig._faction_font;

        hintsBase._elev_range = infoconfig.elev_range;


        //加载翻译到本地
        hintsTranslations.teamNames = hudtranslations.teamNames;

        hintsTranslations.teamColors = hudtranslations.teamColors;

        hintsTranslations.templates.scp914 = hudtranslations.scp914_template;
        hintsTranslations.templates.faction = hudtranslations.f_template;
        hintsTranslations.templates.elevator = hudtranslations.elev_template;
        hintsTranslations.templates.ntf = hudtranslations.ntf_respawn_template;
        hintsTranslations.templates.ci = hudtranslations.ci_respawn_template;
        hintsTranslations.templates.ntfmini = hudtranslations.ntf_mini_respawn_template;
        hintsTranslations.templates.cimini = hudtranslations.ci_mini_respawn_template;

        hintsTranslations.scp914knobtranslations = hudtranslations.scp914_trans;
    }

    public override void Disable()
    {
        Instance = null!;

        PlayerEvents.Joined -= OnPlayerJoin;
        PlayerEvents.ChangedRole -= OnRoleChanged;
        PlayerEvents.InteractingElevator -= OnInteractingElevator;
        ServerEvents.RoundStarted -= OnRoundStart;
        Scp914Events.Activating -= On914Activating;

        foreach (var hud in _huds.Values) hud.Dispose();
            _huds.Clear();
    }

    //电梯显示已完成
    private void OnInteractingElevator(PlayerInteractingElevatorEventArgs ev) 
    {
        if(hintsBase.i_info_elevator == false) return; //配置文件禁用则不处理

        var near = Player.List.Where(p =>
            Vector3.Distance(p.Position, ev.Player.Position) <= hintsBase._elev_range);

        var p_operator = ev.Player.Nickname ?? "未知";
        var text = hintsTranslations.templates.elevator.Replace("{p_operator}",p_operator);
        foreach (var p in near)
            _huds[p].ShowElevator(text);
    }

    private void OnPlayerJoin(PlayerJoinedEventArgs ev)
    {
        _huds[ev.Player] = new PlayerHud(ev.Player);
        _huds[ev.Player].Init(hintsBase, hintsTranslations);
    }

    private void OnRoleChanged(PlayerChangedRoleEventArgs ev)
    {
        _huds[ev.Player].OnRoleChanged(ev.NewRole.RoleTypeId, hintsBase.i_info_timer);
    }

    private void OnRoundStart()
    {
        _huds.Values.ToList().ForEach(h => h.Start());
    }

    //914显示已完成
    private void On914Activating(Scp914ActivatingEventArgs ev)
    {
        if (hintsBase.i_info_914 == false) return; //配置文件禁用则不处理

        var knob = ev.KnobSetting;
        var mode = hintsTranslations.scp914knobtranslations[knob];

        var p_operator = ev.Player.Nickname ?? "未知";

        var msg = hintsTranslations.templates.scp914.Replace("{mode}", mode)
                               .Replace("{p_operator}", p_operator);

        foreach (var p in Player.List)
        {
            if(p.IsAlive && p != null && p.Room.Name == RoomName.Lcz914)
            {
                _huds[p].Show914(msg);
            }
        }
    }
}

public class PlayerHud : IDisposable
{
    private readonly Player _pl;
    private readonly List<Hint> _hints = new();

    private Dictionary<Team, string> _teamNames = new();
    private Dictionary<Team, string> _teamColors = new();

    private Hint _h914;
    private Hint _hFaction;
    private Hint _hNtfResp;
    private Hint _hNtfMini;
    private Hint _hCiResp;
    private Hint _hCiMini;
    private Hint _hElevator;

    const int INT_MAX = 2147483647;

    private Dictionary<int, int> lastTime = new();

    HUDInfo.HintsBase BaseA; 
    HUDInfo.HintsTranslations BaseTrans;
    public PlayerHud(Player pl)
    {
        _pl = pl;
        
        //剩余交给Init处理
    }

    public void Init(HUDInfo.HintsBase baseori, HUDInfo.HintsTranslations transoir)
    {
        BaseA = baseori;
        BaseTrans = transoir; //翻译库
        _h914 = new Hint
        {
            Text = "",
            XCoordinate = BaseA.i_914.x,
            YCoordinate = BaseA.i_914.y,
            FontSize = BaseA.i_914.font,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hFaction = new Hint
        {
            Text = "",
            XCoordinate = BaseA.i_faction.x,
            YCoordinate = BaseA.i_faction.y,
            FontSize = BaseA.i_faction.font,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hNtfResp = new Hint
        {
            Text = "",
            XCoordinate = BaseA.i_ntf.x,
            YCoordinate = BaseA.i_ntf.y,
            FontSize = BaseA.i_ntf.font,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hNtfMini = new Hint
        {
            Text = "",
            XCoordinate = BaseA.i_ntfmini.x,
            YCoordinate = BaseA.i_ntfmini.y,
            FontSize = BaseA.i_ntfmini.font,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hCiResp = new Hint
        {
            Text = "",
            XCoordinate = BaseA.i_ci.x,
            YCoordinate = BaseA.i_ci.y,
            FontSize = BaseA.i_ci.font,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hCiMini = new Hint
        {
            Text = "",
            XCoordinate = BaseA.i_cimini.x,
            YCoordinate = BaseA.i_cimini.y,
            FontSize = BaseA.i_cimini.font,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hElevator = new Hint
        {
            Text = "",
            XCoordinate = BaseA.i_elevator.x,
            YCoordinate = BaseA.i_elevator.y,
            FontSize = BaseA.i_elevator.font,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        //翻译替换
        _teamNames = BaseTrans.teamNames;

        _teamColors = BaseTrans.teamColors;

        lastTime = new Dictionary<int, int>()
        {
            { 0, INT_MAX },
            { 1, INT_MAX },
            { 2, INT_MAX },
            { 3, INT_MAX }
        };

        var display = PlayerDisplay.Get(_pl);
        _hints.AddRange(new[] { _h914, _hFaction, _hNtfResp, _hNtfMini, _hCiResp, _hCiMini, _hElevator });
        _hints.ForEach(display.AddHint);

        Timing.RunCoroutine(UpdateFaction(BaseA.i_info_faction));
        Timing.RunCoroutine(UpdateRespawn(BaseA.i_info_timer));
    }

    public void Show914(string text)
    {
        _h914.Text = text;
        _h914.Hide = false;
        _h914.HideAfter(15f);
    }

    public void ShowElevator(string text)
    {
        _hElevator.Text = text;
        _hElevator.Hide = false;
        _hElevator.HideAfter(7f);
    }

    public void OnRoleChanged(RoleTypeId newRole, bool is_toshow)
    {
        if(!is_toshow)
        {
            _hNtfResp.Hide = _hNtfMini.Hide = _hCiResp.Hide = _hCiMini.Hide = true;
            return;
        }
        var spectator = newRole == RoleTypeId.Spectator;
        _hNtfResp.Hide = _hNtfMini.Hide = _hCiResp.Hide = _hCiMini.Hide = !spectator;
    }

    public void Start() { }

    private IEnumerator<float> UpdateFaction(bool is_toshow)
    {
        if(!is_toshow) yield break;
        while (true)
        {
            if (_pl.IsAlive && _pl.Role != RoleTypeId.Spectator && 
                (_pl.Team == Team.FoundationForces || _pl.Team == Team.ClassD 
                || _pl.Team == Team.ChaosInsurgency 
                || _pl.Team == Team.SCPs || _pl.Team == Team.Scientists))
            {
                if(_pl.Team == Team.FoundationForces || _pl.Team == Team.Scientists)
                {
                    var team1 = Team.Scientists;
                    var team2 = Team.FoundationForces;

                    var count1 = Player.List.Count(p => p.Team == team1);
                    var count2 = Player.List.Count(p => p.Team == team2);

                    _teamNames.TryGetValue(team1, out var name1);
                    _teamNames.TryGetValue(team2, out var name2);

                    _teamColors.TryGetValue(team1, out var color1);
                    _teamColors.TryGetValue(team2, out var color2);

                    var text = BaseTrans.templates.faction
                               .Replace("{color}", color1)
                               .Replace("{name}", name1)
                               .Replace("{count}", count1.ToString()); //根据模板替换
                    text += "\n";
                    text += BaseTrans.templates.faction
                               .Replace("{color}", color2)
                               .Replace("{name}", name2)
                               .Replace("{count}", count2.ToString()); //根据模板替换

                    _hFaction.Text = text; //更新文本

                    _hFaction.Hide = false; //显示
                }
                else if(_pl.Team == Team.ClassD || _pl.Team == Team.ChaosInsurgency)
                {
                    var team1 = Team.ChaosInsurgency;
                    var team2 = Team.ClassD;

                    var count1 = Player.List.Count(p => p.Team == team1);
                    var count2 = Player.List.Count(p => p.Team == team2);

                    _teamNames.TryGetValue(team1, out var name1);
                    _teamNames.TryGetValue(team2, out var name2);

                    _teamColors.TryGetValue(team1, out var color1);
                    _teamColors.TryGetValue(team2, out var color2);

                    var text = BaseTrans.templates.faction
                               .Replace("{color}", color1)
                               .Replace("{name}", name1)
                               .Replace("{count}", count1.ToString()); //根据模板替换
                    text += "\n";
                    text += BaseTrans.templates.faction
                               .Replace("{color}", color2)
                               .Replace("{name}", name2)
                               .Replace("{count}", count2.ToString()); //根据模板替换

                    _hFaction.Text = text; //更新文本

                    _hFaction.Hide = false; //显示
                }
                else if(_pl.Team == Team.SCPs)
                {
                    var team = Team.SCPs;
                    var count = Player.List.Count(p => p.Team == team);

                    //获取显示队伍名称和颜色
                    _teamColors.TryGetValue(team, out var color);
                    _teamNames.TryGetValue(team, out var name);

                    var text = BaseTrans.templates.faction
                                   .Replace("{color}", color)
                                   .Replace("{name}", name)
                                   .Replace("{count}", count.ToString()); //根据模板替换

                    _hFaction.Text = text; //更新文本

                    _hFaction.Hide = false; //显示
                }
            }else //其他（如管理等）
            {
                _hFaction.Hide = true; //隐藏
            }
            yield return Timing.WaitForSeconds(1f);
        }
    }

    private IEnumerator<float> UpdateRespawn(bool is_toshow)
    {
        if(!is_toshow) yield break;
        while (true)
        {
            if (_pl.Role == RoleTypeId.Spectator)
            {
                int _ntf = (int)(RespawnWaves.PrimaryMtfWave.TimeLeft + RespawnWaves.PrimaryMtfWave.AnimationTime);
                int _ntfmini = (int)(RespawnWaves.MiniMtfWave.TimeLeft + RespawnWaves.MiniMtfWave.AnimationTime);
                int _ci = (int)(RespawnWaves.PrimaryChaosWave.TimeLeft + RespawnWaves.PrimaryChaosWave.AnimationTime);
                int _cimini = (int)(RespawnWaves.MiniChaosWave.TimeLeft + RespawnWaves.MiniChaosWave.AnimationTime);

                var paused = "<color=red>已暂停</color>";

                var ntfTime = (_ntf > 0 && lastTime[0] != _ntf)
                    ? $"{_ntf}秒"
                    : paused;
                var ntfMini = (_ntfmini > 0 && lastTime[1] != _ntfmini)
                    ? $"{_ntfmini}秒"
                    : paused;
                var ciTime = (_ci > 0 && lastTime[2] != _ci)
                    ? $"{_ci}秒"
                    : paused;
                var ciMini = (_cimini > 0 && lastTime[3] != _cimini)
                    ? $"{_cimini}秒"
                    : paused;

                lastTime[0] = _ntf;
                lastTime[1] = _ntfmini;
                lastTime[2] = _ci;
                lastTime[3] = _cimini;

                //2次确认显示
                _hNtfMini.Hide = _hNtfResp.Hide = _hCiResp.Hide = _hCiMini.Hide = false; //显示

                _hNtfResp.Text = BaseTrans.templates.ntf.Replace("{Time}", ntfTime);
                _hNtfMini.Text = BaseTrans.templates.ntfmini.Replace("{Time}", ntfMini);
                _hCiResp.Text = BaseTrans.templates.ci.Replace("{Time}", ciTime);
                _hCiMini.Text = BaseTrans.templates.cimini.Replace("{Time}", ciMini);
                //_hNtfResp.Text = $"<color=#00BFFF>下一次刷新时间:</color> {ntfTime}";
                //_hNtfMini.Text = $"<color=#00BFFF>迷你波:</color> {ntfMini}";
                //_hCiResp.Text = $"<color=#008F1C>下一次刷新时间:</color> {ciTime}";
                //_hCiMini.Text = $"<color=#008F1C>迷你波:</color> {ciMini}";
            }else{
                _hNtfResp.Hide = _hNtfMini.Hide = _hCiResp.Hide = _hCiMini.Hide = true; //隐藏
            }
            yield return Timing.WaitForSeconds(1f);
        }
    }

    public void Dispose()
    {
        foreach (var h in _hints) h.Hide = true;
    }
}
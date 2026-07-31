// HUDInfo.cs
using Hints;
using HintServiceMeow;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Extension;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI;
using LabApi;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MapGeneration;
using MEC;
using PlayerRoles;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;
using Logger = LabApi.Features.Console.Logger;
using Vector3 = UnityEngine.Vector3;
using Version = System.Version;

namespace HUDInfo;

public class HUDInfo : Plugin<HUDInfoConfig>
{
    public static HUDInfo Instance { get; set; } = null!;

    // 按连接（Player 实例）为单位持有 HUD 状态，玩家断线时清理（见 OnPlayerLeft）
    private readonly Dictionary<Player, PlayerHud> _huds = new();

    private HUDInfoConfig _config;
    private HUDTranslation _translation;

    private bool _hasIncorrectSettings = false;
    private bool _hasIncorrectTranslation = false;

    public override LoadPriority Priority { get; } = LoadPriority.High;
    public override string Name { get; } = "HUDInfo";
    public override string Description { get; } = "一个优秀的信息显示拓展插件!";
    public override string Author { get; } = "Crystal";
    public override Version Version { get; } = new Version(2, 2, 0);
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    public override void LoadConfigs()
    {
        _hasIncorrectSettings = !this.TryLoadConfig("config.yml", out _config);
        _hasIncorrectTranslation = !this.TryLoadConfig("translations.yml", out _translation);
        base.LoadConfigs();
    }

    public override void Enable()
    {
        Instance = this;

        // 配置/翻译校验必须在订阅任何事件之前完成
        if (_hasIncorrectSettings || _config == null)
        {
            Logger.Error($"{Name} 配置文件加载失败，请检查 config.yml 格式或删除后重启！");
            return;
        }

        if (_hasIncorrectTranslation || _translation == null)
        {
            Logger.Error($"{Name} 翻译文件加载失败，请检查 translations.yml 格式或删除后重启！");
            return;
        }

        // 配置验证：检查翻译字典是否包含所有必需的键
        if (!ValidateTranslations())
        {
            Logger.Error($"{Name} 翻译配置缺少必需的键，请检查 translations.yml 或删除后重启！");
            return;
        }

        PlayerEvents.Joined += OnPlayerJoin;
        PlayerEvents.Left += OnPlayerLeft;
        PlayerEvents.ChangedRole += OnRoleChanged;
        PlayerEvents.InteractingElevator += OnInteractingElevator;
        ServerEvents.RoundStarted += OnRoundStart;
        Scp914Events.Activating += On914Activating;

        Logger.Info($"{Name} 插件加载成功! v{Version} by {Author} - {Description}");
    }

    public override void Disable()
    {
        Instance = null!;

        PlayerEvents.Joined -= OnPlayerJoin;
        PlayerEvents.Left -= OnPlayerLeft;
        PlayerEvents.ChangedRole -= OnRoleChanged;
        PlayerEvents.InteractingElevator -= OnInteractingElevator;
        ServerEvents.RoundStarted -= OnRoundStart;
        Scp914Events.Activating -= On914Activating;

        foreach (var hud in _huds.Values)
        {
            hud.Dispose();
        }
        _huds.Clear();
    }

    /// <summary>
    /// 验证翻译字典是否包含所有必需的键
    /// </summary>
    private bool ValidateTranslations()
    {
        var requiredTeams = new[] { Team.SCPs, Team.ChaosInsurgency, Team.FoundationForces, Team.ClassD, Team.Scientists };
        foreach (var team in requiredTeams)
        {
            if (!_translation.TeamNames.ContainsKey(team) || !_translation.TeamColors.ContainsKey(team))
            {
                Logger.Error($"翻译配置缺少阵营 {team} 的名称或颜色定义");
                return false;
            }
        }

        var requiredModes = new[] { Scp914KnobSetting.Rough, Scp914KnobSetting.Coarse, Scp914KnobSetting.OneToOne, Scp914KnobSetting.Fine, Scp914KnobSetting.VeryFine };
        foreach (var mode in requiredModes)
        {
            if (!_translation.Scp914Modes.ContainsKey(mode))
            {
                Logger.Error($"翻译配置缺少 SCP-914 模式 {mode} 的翻译");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 电梯交互事件处理 - 修复Bug1和Bug2
    /// </summary>
    private void OnInteractingElevator(PlayerInteractingElevatorEventArgs ev)
    {
        if (!_config.EnableElevatorHint) return;

        // Bug修复1: 只有当电梯就绪（不在移动中）时才显示提示，避免重复按按钮时多次显示
        if (ev.Elevator == null || !ev.Elevator.IsReady) return;

        var near = Player.List.Where(p =>
            Vector3.Distance(p.Position, ev.Player.Position) <= _config.Elevator.Range);

        var p_operator = ev.Player?.Nickname ?? _translation.UnknownOperator;

        // Bug修复2: 保留 {sec} 占位符，只替换 {p_operator}，让协程动态更新秒数
        var template = _translation.ElevatorTemplate.Replace("{p_operator}", p_operator);

        foreach (var p in near)
        {
            if (_huds.TryGetValue(p, out var hud))
                hud.ShowElevator(template);
        }
    }

    private void OnPlayerJoin(PlayerJoinedEventArgs ev)
    {
        var hud = new PlayerHud(ev.Player, _config, _translation);
        _huds[ev.Player] = hud;
    }

    private void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
        if (_huds.TryGetValue(ev.Player, out var hud))
        {
            hud.Dispose();
            _huds.Remove(ev.Player);
        }
    }

    private void OnRoleChanged(PlayerChangedRoleEventArgs ev)
    {
        if (_huds.TryGetValue(ev.Player, out var hud))
            hud.OnRoleChanged(ev.NewRole.RoleTypeId);
    }

    private void OnRoundStart()
    {
        // 预留：回合开始时的 HUD 重置逻辑
    }

    /// <summary>
    /// SCP-914 激活事件处理
    /// </summary>
    private void On914Activating(Scp914ActivatingEventArgs ev)
    {
        if (!_config.Enable914Hint) return;

        var mode = _translation.Scp914Modes[ev.KnobSetting];
        var p_operator = ev.Player?.Nickname ?? _translation.UnknownOperator;

        var msg = _translation.Scp914Template
            .Replace("{mode}", mode)
            .Replace("{p_operator}", p_operator);

        foreach (var p in Player.List)
        {
            if (p != null && p.IsAlive && p.Room?.Name == RoomName.Lcz914)
            {
                if (_huds.TryGetValue(p, out var hud))
                    hud.Show914(msg);
            }
        }
    }
}

public class PlayerHud : IDisposable
{
    private readonly Player _pl;
    private readonly HUDInfoConfig _config;
    private readonly HUDTranslation _translation;
    private readonly List<Hint> _hints = new();

    private Hint _h914;
    private Hint _hFaction;
    private Hint _hNtfResp;
    private Hint _hNtfMini;
    private Hint _hCiResp;
    private Hint _hCiMini;
    private Hint _hElevator;

    // 电梯：多条目列表（最多同时显示3条），每条有独立的到期时间
    private const int ElevMaxEntries = 3;
    private readonly List<(string template, float expiresAt)> _elevEntries = new();
    private CoroutineHandle _elevatorCoroutine;

    // 上一次轮询读到的各刷新波剩余秒数，用于判断计时器是否处于暂停状态
    private int _lastNtf = int.MaxValue;
    private int _lastNtfMini = int.MaxValue;
    private int _lastCi = int.MaxValue;
    private int _lastCiMini = int.MaxValue;

    private PlayerDisplay _display;
    private CoroutineHandle _factionCoroutine;
    private CoroutineHandle _respawnCoroutine;

    public PlayerHud(Player pl, HUDInfoConfig config, HUDTranslation translation)
    {
        _pl = pl;
        _config = config;
        _translation = translation;

        InitializeHints();

        _display = PlayerDisplay.Get(_pl);
        _hints.ForEach(_display.AddHint);

        _factionCoroutine = Timing.RunCoroutine(UpdateFaction());
        _respawnCoroutine = Timing.RunCoroutine(UpdateRespawn());
    }

    private void InitializeHints()
    {
        _h914 = new Hint
        {
            Text = "",
            XCoordinate = _config.Scp914.X,
            YCoordinate = _config.Scp914.Y,
            FontSize = _config.Scp914.FontSize,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hFaction = new Hint
        {
            Text = "",
            XCoordinate = _config.FactionCount.X,
            YCoordinate = _config.FactionCount.Y,
            FontSize = _config.FactionCount.FontSize,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hNtfResp = new Hint
        {
            Text = "",
            XCoordinate = _config.RespawnTimer.NtfPrimary.X,
            YCoordinate = _config.RespawnTimer.NtfPrimary.Y,
            FontSize = _config.RespawnTimer.NtfPrimary.FontSize,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hNtfMini = new Hint
        {
            Text = "",
            XCoordinate = _config.RespawnTimer.NtfMini.X,
            YCoordinate = _config.RespawnTimer.NtfMini.Y,
            FontSize = _config.RespawnTimer.NtfMini.FontSize,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hCiResp = new Hint
        {
            Text = "",
            XCoordinate = _config.RespawnTimer.ChaosPrimary.X,
            YCoordinate = _config.RespawnTimer.ChaosPrimary.Y,
            FontSize = _config.RespawnTimer.ChaosPrimary.FontSize,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hCiMini = new Hint
        {
            Text = "",
            XCoordinate = _config.RespawnTimer.ChaosMini.X,
            YCoordinate = _config.RespawnTimer.ChaosMini.Y,
            FontSize = _config.RespawnTimer.ChaosMini.FontSize,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hElevator = new Hint
        {
            Text = "",
            XCoordinate = _config.Elevator.X,
            YCoordinate = _config.Elevator.Y,
            FontSize = _config.Elevator.FontSize,
            YCoordinateAlign = HintVerticalAlign.Bottom
        };

        _hints.AddRange(new[] { _h914, _hFaction, _hNtfResp, _hNtfMini, _hCiResp, _hCiMini, _hElevator });
    }

    public void Show914(string text)
    {
        _h914.Text = text;
        _h914.Hide = false;
        _h914.HideAfter(_config.Scp914.Duration);
    }

    public void ShowElevator(string template)
    {
        // 超过上限时移除最旧条目
        if (_elevEntries.Count >= ElevMaxEntries)
            _elevEntries.RemoveAt(0);

        _elevEntries.Add((template, Timing.LocalTime + _config.Elevator.Duration));

        // 协程未运行时启动
        if (!_elevatorCoroutine.IsRunning)
            _elevatorCoroutine = Timing.RunCoroutine(UpdateElevator());
    }

    private IEnumerator<float> UpdateElevator()
    {
        while (true)
        {
            float now = Timing.LocalTime;
            _elevEntries.RemoveAll(e => now >= e.expiresAt);

            if (_elevEntries.Count == 0)
            {
                _hElevator.Hide = true;
                yield break; // 没有条目时自动停止协程
            }

            _hElevator.Text = string.Join("\n", _elevEntries.Select(e =>
            {
                int rem = Math.Max(1, (int)Math.Ceiling(e.expiresAt - Timing.LocalTime));
                return e.template.Replace("{sec}", rem.ToString());
            }));
            _hElevator.Hide = false;

            yield return Timing.WaitForSeconds(1f);
        }
    }

    public void OnRoleChanged(RoleTypeId newRole)
    {
        if (!_config.EnableRespawnTimer)
        {
            _hNtfResp.Hide = _hNtfMini.Hide = _hCiResp.Hide = _hCiMini.Hide = true;
            return;
        }

        var spectator = newRole == RoleTypeId.Spectator;
        _hNtfResp.Hide = _hNtfMini.Hide = _hCiResp.Hide = _hCiMini.Hide = !spectator;
    }

    private IEnumerator<float> UpdateFaction()
    {
        if (!_config.EnableFactionCount) yield break;

        while (true)
        {
            if (_pl.IsAlive && _pl.Role != RoleTypeId.Spectator)
            {
                if (_pl.Team == Team.FoundationForces || _pl.Team == Team.Scientists)
                {
                    var text = BuildFactionText(Team.Scientists, Team.FoundationForces);
                    _hFaction.Text = text;
                    _hFaction.Hide = false;
                }
                else if (_pl.Team == Team.ClassD || _pl.Team == Team.ChaosInsurgency)
                {
                    var text = BuildFactionText(Team.ChaosInsurgency, Team.ClassD);
                    _hFaction.Text = text;
                    _hFaction.Hide = false;
                }
                else if (_pl.Team == Team.SCPs)
                {
                    var count = Player.List.Count(p => p.Team == Team.SCPs);
                    var text = _translation.FactionTemplate
                        .Replace("{color}", _translation.TeamColors[Team.SCPs])
                        .Replace("{name}", _translation.TeamNames[Team.SCPs])
                        .Replace("{count}", count.ToString());
                    _hFaction.Text = text;
                    _hFaction.Hide = false;
                }
                else
                {
                    _hFaction.Hide = true;
                }
            }
            else
            {
                _hFaction.Hide = true;
            }

            yield return Timing.WaitForSeconds(_config.UpdateInterval);
        }
    }

    private string BuildFactionText(Team team1, Team team2)
    {
        var count1 = Player.List.Count(p => p.Team == team1);
        var count2 = Player.List.Count(p => p.Team == team2);

        var text1 = _translation.FactionTemplate
            .Replace("{color}", _translation.TeamColors[team1])
            .Replace("{name}", _translation.TeamNames[team1])
            .Replace("{count}", count1.ToString());

        var text2 = _translation.FactionTemplate
            .Replace("{color}", _translation.TeamColors[team2])
            .Replace("{name}", _translation.TeamNames[team2])
            .Replace("{count}", count2.ToString());

        return $"{text1}\n{text2}";
    }

    private IEnumerator<float> UpdateRespawn()
    {
        if (!_config.EnableRespawnTimer) yield break;

        while (true)
        {
            if (_pl.Role == RoleTypeId.Spectator)
            {
                int _ntf = (int)(RespawnWaves.PrimaryMtfWave.TimeLeft + RespawnWaves.PrimaryMtfWave.AnimationTime);
                int _ntfmini = (int)(RespawnWaves.MiniMtfWave.TimeLeft + RespawnWaves.MiniMtfWave.AnimationTime);
                int _ci = (int)(RespawnWaves.PrimaryChaosWave.TimeLeft + RespawnWaves.PrimaryChaosWave.AnimationTime);
                int _cimini = (int)(RespawnWaves.MiniChaosWave.TimeLeft + RespawnWaves.MiniChaosWave.AnimationTime);

                var paused = "<color=red>已暂停</color>";

                var ntfTime = (_ntf > 0 && _lastNtf != _ntf) ? $"{_ntf}秒" : paused;
                var ntfMini = (_ntfmini > 0 && _lastNtfMini != _ntfmini) ? $"{_ntfmini}秒" : paused;
                var ciTime = (_ci > 0 && _lastCi != _ci) ? $"{_ci}秒" : paused;
                var ciMini = (_cimini > 0 && _lastCiMini != _cimini) ? $"{_cimini}秒" : paused;

                _lastNtf = _ntf;
                _lastNtfMini = _ntfmini;
                _lastCi = _ci;
                _lastCiMini = _cimini;

                _hNtfResp.Hide = _hNtfMini.Hide = _hCiResp.Hide = _hCiMini.Hide = false;

                _hNtfResp.Text = _translation.NtfRespawnTemplate.Replace("{Time}", ntfTime);
                _hNtfMini.Text = _translation.NtfMiniRespawnTemplate.Replace("{Time}", ntfMini);
                _hCiResp.Text = _translation.CiRespawnTemplate.Replace("{Time}", ciTime);
                _hCiMini.Text = _translation.CiMiniRespawnTemplate.Replace("{Time}", ciMini);
            }
            else
            {
                _hNtfResp.Hide = _hNtfMini.Hide = _hCiResp.Hide = _hCiMini.Hide = true;
            }

            yield return Timing.WaitForSeconds(_config.UpdateInterval);
        }
    }

    public void Dispose()
    {
        // 杀掉所有轮询协程，避免玩家断线后协程继续运行
        Timing.KillCoroutines(_factionCoroutine, _respawnCoroutine, _elevatorCoroutine);

        // 把 Hint 从 PlayerDisplay 上摘除，避免内存泄漏
        if (_display != null)
            _display.RemoveHint(_hints);

        _hints.Clear();
    }
}

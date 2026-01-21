using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Utils;
using CSSharpUtils.Extensions;
using CSSharpUtils.Utils;

namespace hvhgg_essentials.Features;

public class RageQuit
{
    private readonly Plugin _plugin;
    public static readonly FakeConVar<bool> hvh_ragequit = new("hvh_ragequit", "Enables the rage quit feature", true, ConVarFlags.FCVAR_REPLICATED);

    public RageQuit(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        hvh_ragequit.Value = _plugin.Config.AllowRageQuit;
    }
    
    [ConsoleCommand("css_rq", "Rage quit")]
    [ConsoleCommand("css_ragequit", "Rage quit")]
    [ConsoleCommand("rq", "Rage quit")]
    [ConsoleCommand("ragequit", "Rage quit")]
    public void OnRageQuit(CCSPlayerController? player, CommandInfo inf)
    {
        if (!hvh_ragequit.Value)
            return;
        
        if (!player.IsPlayer()) 
            return;
        
        // Save player name BEFORE kicking (player object becomes invalid after kick)
        var playerName = player!.PlayerName;
        
        // Announce to all players first
        Server.PrintToChatAll($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} {ChatColors.Red}{playerName}{ChatColors.Default} deu ragequit!");
        
        // Then kick the player
        player.Kick("Rage quit");
    }
}
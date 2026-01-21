using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CSSharpUtils.Extensions;
using CSSharpUtils.Utils;
using hvhgg_essentials.Enums;

namespace hvhgg_essentials.Features;

public class Misc
{
    private readonly Plugin _plugin;
    private readonly Dictionary<uint, float> _lastRulePrint = new();

    public Misc(Plugin plugin)
    {
        _plugin = plugin;
    }

    [ConsoleCommand("settings", "Print settings")]
    public void OnSettings(CCSPlayerController? player, CommandInfo inf)
    {
        AnnounceRules(player, true);
    }
    
    [ConsoleCommand("hvh_cfg_reload", "Reload the config in the current session without restarting the server")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnReloadConfigCommand(CCSPlayerController? player, CommandInfo info)
    {
        _plugin.OnConfigParsed(new Cs2EssentialsConfig().Reload());
    }
    
    public void AnnounceRules(CCSPlayerController? player, bool force = false)
    {
        if (!player.IsPlayer())
            return;

        if (_lastRulePrint.TryGetValue(player!.Pawn.Index, out var lastPrintTime) &&
            lastPrintTime + 600 > Server.CurrentTime && !force)
            return;

        _lastRulePrint[player.Pawn.Index] = Server.CurrentTime;

        if (_plugin.Config.AllowSettingsPrint)
        {
            player.PrintToChat("Regras do Servidor:");
            player.PrintToChat(
                $"Fogo amigo apenas de utilitários: {(_plugin.Config.UnmatchedFriendlyFire ? $"{ChatColors.Lime}ativado" : $"{ChatColors.Red}desativado")}");
            player.PrintToChat(
                $"Teleporte/Airstuck: {(!_plugin.Config.RestrictTeleport ? $"{ChatColors.Lime}permitido" : $"{ChatColors.Red}bloqueado")}");
            player.PrintToChat(
                $"Rapid fire: {(_plugin.Config.RapidFireFixMethod == FixMethod.Allow ? $"{ChatColors.Lime}permitido" : $"{ChatColors.Red}bloqueado")}");

            switch (_plugin.Config.RapidFireFixMethod)
            {
                case FixMethod.Allow:
                    break;
                case FixMethod.Ignore:
                    player.PrintToChat($"Método: {ChatColors.Red}bloquear dano");
                    break;
                case FixMethod.Reflect:
                    player.PrintToChat(
                        $"Método: {ChatColors.Red}refletir dano{ChatColors.Default} em {ChatColors.Orange}{_plugin.Config.RapidFireReflectScale}x{ChatColors.Default}");
                    break;
                case FixMethod.ReflectSafe:
                    player.PrintToChat(
                        $"Método: {ChatColors.Red}refletir dano{ChatColors.Default} em {ChatColors.Orange}{_plugin.Config.RapidFireReflectScale}x{ChatColors.Default} sem matar");
                    break;
                default:
                    break;
            }

            player.PrintToChat(" ");
            player.PrintToChat("Restrição de armas:");
            if (_plugin.Config.AllowedAwpCount != -1)
                player.PrintToChat(
                    $"AWP: {(_plugin.Config.AllowedAwpCount == 0 ? ChatColors.Red : ChatColors.Orange)}{_plugin.Config.AllowedAwpCount} por time");
            if (_plugin.Config.AllowedScoutCount != -1)
                player.PrintToChat(
                    $"Scout: {(_plugin.Config.AllowedScoutCount == 0 ? ChatColors.Red : ChatColors.Orange)}{_plugin.Config.AllowedScoutCount} por time");
            if (_plugin.Config.AllowedAutoSniperCount != -1)
                player.PrintToChat(
                    $"Auto: {(_plugin.Config.AllowedAutoSniperCount == 0 ? ChatColors.Red : ChatColors.Orange)}{_plugin.Config.AllowedAutoSniperCount} por time");

            player.PrintToChat(" ");
            player.PrintToChat(
                $"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} Digite {ChatColors.Red}!settings{ChatColors.Default} para ver estas configurações novamente");
        }
        
        // Ad print removed - plugin rebranded to Pitu
        
    }
}
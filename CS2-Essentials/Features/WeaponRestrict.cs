using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using CSSharpUtils.Extensions;
using CSSharpUtils.Utils;

namespace hvhgg_essentials.Features;

public class WeaponRestrict
{
    private readonly Plugin _plugin;
    
    // Weapon definitions with ItemDefinition enum and prices for refund
    private readonly Dictionary<string, (ItemDefinition itemDef, int price)> _restrictedWeapons = new()
    {
        { "weapon_ssg08", (ItemDefinition.SSG_08, 1700) },
        { "weapon_awp", (ItemDefinition.AWP, 4750) },
        { "weapon_scar20", (ItemDefinition.SCAR_20, 5000) },
        { "weapon_g3sg1", (ItemDefinition.G3SG1, 5000) },
    };
    
    // Cooldown for warning messages to avoid spam (uses SteamID for stability)
    private readonly Dictionary<ulong, float> _lastWarningTime = new();
    private const float WarningCooldown = 3f;
    
    // ConVars for runtime configuration
    public static readonly FakeConVar<int> hvh_restrict_awp = new("hvh_restrict_awp", "Restrict awp to X per team (-1 = unlimited)", -1, ConVarFlags.FCVAR_REPLICATED, new RangeValidator<int>(-1, int.MaxValue));
    public static readonly FakeConVar<int> hvh_restrict_scout = new("hvh_restrict_scout", "Restrict scout to X per team (-1 = unlimited)", -1, ConVarFlags.FCVAR_REPLICATED, new RangeValidator<int>(-1, int.MaxValue));
    public static readonly FakeConVar<int> hvh_restrict_auto = new("hvh_restrict_auto", "Restrict autosniper to X per team (-1 = unlimited)", -1, ConVarFlags.FCVAR_REPLICATED, new RangeValidator<int>(-1, int.MaxValue));

    public WeaponRestrict(Plugin plugin)
    {
        _plugin = plugin;
        _plugin.RegisterFakeConVars(this);
        
        // Load initial values from config
        hvh_restrict_awp.Value = _plugin.Config.AllowedAwpCount;
        hvh_restrict_scout.Value = _plugin.Config.AllowedScoutCount;
        hvh_restrict_auto.Value = _plugin.Config.AllowedAutoSniperCount;
    }
    
    /// <summary>
    /// PRIMARY BLOCKING HOOK - Called BEFORE a player can pick up or use a weapon.
    /// This is the most reliable way to prevent weapon usage.
    /// </summary>
    public HookResult OnWeaponCanUse(DynamicHook hook)
    {
        try
        {
            var weaponServices = hook.GetParam<CCSPlayer_WeaponServices>(0);
            var weapon = hook.GetParam<CBasePlayerWeapon>(1);
            
            if (weaponServices?.Pawn?.Value?.Controller?.Value == null)
                return HookResult.Continue;
            
            var player = new CCSPlayerController(weaponServices.Pawn.Value.Controller.Value.Handle);
            
            if (!player.IsPlayer())
                return HookResult.Continue;

            var weaponName = weapon.DesignerName;
            
            // Not a restricted weapon
            if (!_restrictedWeapons.ContainsKey(weaponName))
                return HookResult.Continue;

            var limit = GetWeaponLimit(weaponName);
            
            // -1 means unlimited
            if (limit == -1)
                return HookResult.Continue;
            
            // Check if this player ALREADY owns this weapon type
            // If yes, allow them to use their own weapon (e.g., after dropping and picking up)
            if (PlayerOwnsWeaponType(player, weaponName, weapon))
                return HookResult.Continue;

            // Count how many of this weapon type the team currently has
            // Exclude the weapon being picked up from count (it's not owned by anyone yet)
            var teamCount = CountWeaponsInTeam(weaponName, player.Team, weapon);
            
            // If team is at or over limit, block
            if (teamCount >= limit)
            {
                ShowWarning(player, weaponName, limit);
                
                // If this weapon was just purchased (not picked up from ground), remove it
                if (IsNewlyPurchased(weapon))
                {
                    Server.NextFrame(() => SafeRemoveWeapon(weapon));
                }
                
                hook.SetReturn(false);
                return HookResult.Handled;
            }
            
            return HookResult.Continue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Pitu] Error in OnWeaponCanUse: {ex.Message}");
            return HookResult.Continue;
        }
    }
    
    /// <summary>
    /// BACKUP HANDLER - Called AFTER a purchase is made.
    /// Acts as a safety net if OnWeaponCanUse didn't catch it.
    /// </summary>
    public HookResult OnItemPurchase(EventItemPurchase ev, GameEventInfo info)
    {
        try
        {
            var player = ev.Userid;
            
            if (player == null || !player.IsPlayer())
                return HookResult.Continue;

            // Normalize weapon name (event may not include "weapon_" prefix)
            var weaponName = ev.Weapon;
            if (!weaponName.StartsWith("weapon_"))
                weaponName = "weapon_" + weaponName;

            if (!_restrictedWeapons.ContainsKey(weaponName))
                return HookResult.Continue;

            var limit = GetWeaponLimit(weaponName);
            
            if (limit == -1)
                return HookResult.Continue;

            // At this point, the weapon is already in the player's inventory
            // Count all weapons of this type in the team
            var teamCount = CountWeaponsInTeam(weaponName, player.Team);
            
            Console.WriteLine($"[Pitu] Purchase check: {player.PlayerName} bought {weaponName}. Team count: {teamCount}/{limit}");
            
            // If over limit, remove and refund
            if (teamCount > limit)
            {
                ShowWarning(player, weaponName, limit);
                RefundWeapon(player, weaponName);
                
                // Remove on next frame to ensure weapon is fully registered
                Server.NextFrame(() => player.RemoveItemByDesignerName(weaponName, true));
            }
            
            return HookResult.Continue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Pitu] Error in OnItemPurchase: {ex.Message}");
            return HookResult.Continue;
        }
    }
    
    /// <summary>
    /// TEAM SWITCH HANDLER - Called when a player changes teams.
    /// In TDM, removes restricted weapons if new team is at limit.
    /// </summary>
    public HookResult OnPlayerTeam(EventPlayerTeam ev, GameEventInfo info)
    {
        try
        {
            var player = ev.Userid;
            if (player == null || !player.IsPlayer())
                return HookResult.Continue;
            
            var newTeam = (CsTeam)ev.Team;
            
            // Skip if going to spectator or unassigned
            if (newTeam != CsTeam.Terrorist && newTeam != CsTeam.CounterTerrorist)
                return HookResult.Continue;
            
            // Check on next frame to ensure team change is complete
            Server.NextFrame(() => CheckWeaponsAfterTeamChange(player, newTeam));
            
            return HookResult.Continue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Pitu] Error in OnPlayerTeam: {ex.Message}");
            return HookResult.Continue;
        }
    }
    
    /// <summary>
    /// Check if player has restricted weapons that exceed limit on new team.
    /// </summary>
    private void CheckWeaponsAfterTeamChange(CCSPlayerController player, CsTeam newTeam)
    {
        if (!player.IsValid || !player.IsPlayer())
            return;
        
        foreach (var (weaponName, _) in _restrictedWeapons)
        {
            var limit = GetWeaponLimit(weaponName);
            if (limit == -1)
                continue;
            
            // Check if player has this weapon
            if (!PlayerHasWeapon(player, weaponName))
                continue;
            
            // Count weapons in the NEW team (excluding this player's weapon)
            var teamCount = CountWeaponsInTeamExcludingPlayer(weaponName, newTeam, player);
            
            // If team is already at or over limit, remove this player's weapon
            if (teamCount >= limit)
            {
                Console.WriteLine($"[Pitu] Team switch: {player.PlayerName} has {weaponName} but {newTeam} already at limit ({teamCount}/{limit})");
                ShowWarning(player, weaponName, limit);
                player.RemoveItemByDesignerName(weaponName, true);
            }
        }
    }
    
    /// <summary>
    /// Simple check if player has a specific weapon type.
    /// </summary>
    private bool PlayerHasWeapon(CCSPlayerController player, string weaponName)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn?.WeaponServices?.MyWeapons == null)
            return false;
        
        var targetItemDef = (ushort)_restrictedWeapons[weaponName].itemDef;
        
        foreach (var weaponHandle in pawn.WeaponServices.MyWeapons)
        {
            if (!weaponHandle.IsValid || weaponHandle.Value == null)
                continue;
            
            if (weaponHandle.Value.AttributeManager.Item.ItemDefinitionIndex == targetItemDef)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Count weapons in team, excluding a specific player's weapons.
    /// </summary>
    private int CountWeaponsInTeamExcludingPlayer(string weaponName, CsTeam team, CCSPlayerController excludePlayer)
    {
        var targetItemDef = (ushort)_restrictedWeapons[weaponName].itemDef;
        var count = 0;
        
        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsValid || !player.IsPlayer() || player.TeamNum != (byte)team)
                continue;
            
            // Skip the player we're checking for
            if (player.SteamID == excludePlayer.SteamID)
                continue;
            
            var pawn = player.PlayerPawn.Value;
            if (pawn?.WeaponServices?.MyWeapons == null)
                continue;
            
            foreach (var weaponHandle in pawn.WeaponServices.MyWeapons)
            {
                if (!weaponHandle.IsValid || weaponHandle.Value == null)
                    continue;
                
                if (weaponHandle.Value.AttributeManager.Item.ItemDefinitionIndex == targetItemDef)
                    count++;
            }
        }
        
        return count;
    }
    
    #region Helper Methods
    
    /// <summary>
    /// Check if the player already owns a weapon of this type.
    /// This allows a player to pick up their OWN dropped weapon.
    /// </summary>
    private bool PlayerOwnsWeaponType(CCSPlayerController player, string weaponName, CBasePlayerWeapon excludeWeapon)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn?.WeaponServices?.MyWeapons == null)
            return false;
        
        var targetItemDef = (ushort)_restrictedWeapons[weaponName].itemDef;
        
        foreach (var weaponHandle in pawn.WeaponServices.MyWeapons)
        {
            if (!weaponHandle.IsValid || weaponHandle.Value == null || !weaponHandle.Value.IsValid)
                continue;
            
            // Skip the weapon being picked up
            if (weaponHandle.Value.Handle == excludeWeapon.Handle)
                continue;
            
            if (weaponHandle.Value.AttributeManager.Item.ItemDefinitionIndex == targetItemDef)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Count weapons of specified type that are OWNED by players in the team.
    /// Optionally exclude a specific weapon entity from the count.
    /// </summary>
    private int CountWeaponsInTeam(string weaponName, CsTeam team, CBasePlayerWeapon? excludeWeapon = null)
    {
        var targetItemDef = (ushort)_restrictedWeapons[weaponName].itemDef;
        var count = 0;
        
        foreach (var player in Utilities.GetPlayers())
        {
            // Skip invalid players or players on different teams
            if (!player.IsValid || !player.IsPlayer() || player.TeamNum != (byte)team)
                continue;
            
            var pawn = player.PlayerPawn.Value;
            if (pawn?.WeaponServices?.MyWeapons == null)
                continue;
            
            foreach (var weaponHandle in pawn.WeaponServices.MyWeapons)
            {
                if (!weaponHandle.IsValid || weaponHandle.Value == null || !weaponHandle.Value.IsValid)
                    continue;
                
                // Skip the weapon being evaluated
                if (excludeWeapon != null && weaponHandle.Value.Handle == excludeWeapon.Handle)
                    continue;
                
                if (weaponHandle.Value.AttributeManager.Item.ItemDefinitionIndex == targetItemDef)
                    count++;
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// Get the limit for a specific weapon type from ConVars.
    /// </summary>
    private int GetWeaponLimit(string weaponName)
    {
        return weaponName switch
        {
            "weapon_awp" => hvh_restrict_awp.Value,
            "weapon_ssg08" => hvh_restrict_scout.Value,
            "weapon_scar20" or "weapon_g3sg1" => hvh_restrict_auto.Value,
            _ => -1
        };
    }
    
    /// <summary>
    /// Check if a weapon was just purchased (created within the last 100ms).
    /// </summary>
    private bool IsNewlyPurchased(CBasePlayerWeapon weapon)
    {
        return Math.Abs(weapon.CreateTime - Server.CurrentTime) < 0.1f;
    }
    
    /// <summary>
    /// Safely remove a weapon entity on the next frame.
    /// </summary>
    private void SafeRemoveWeapon(CBasePlayerWeapon weapon)
    {
        try
        {
            if (weapon.IsValid)
            {
                weapon.Remove();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Pitu] Error removing weapon: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Refund the purchase price to the player.
    /// </summary>
    private void RefundWeapon(CCSPlayerController player, string weaponName)
    {
        if (player.InGameMoneyServices != null && _restrictedWeapons.TryGetValue(weaponName, out var weaponInfo))
        {
            player.InGameMoneyServices.Account += weaponInfo.price;
        }
    }
    
    /// <summary>
    /// Show a warning message to the player with cooldown to prevent spam.
    /// </summary>
    private void ShowWarning(CCSPlayerController player, string weaponName, int limit)
    {
        var steamId = player.SteamID;
        var currentTime = Server.CurrentTime;
        
        // Check cooldown using SteamID (stable identifier)
        if (_lastWarningTime.TryGetValue(steamId, out var lastTime) && 
            currentTime - lastTime < WarningCooldown)
        {
            return;
        }
        
        _lastWarningTime[steamId] = currentTime;
        
        // Clean up old entries periodically (prevent memory growth)
        if (_lastWarningTime.Count > 100)
        {
            var keysToRemove = _lastWarningTime
                .Where(kv => currentTime - kv.Value > 60f)
                .Select(kv => kv.Key)
                .ToList();
            foreach (var key in keysToRemove)
                _lastWarningTime.Remove(key);
        }
        
        var friendlyName = weaponName.Replace("weapon_", "").ToUpper();
        player.PrintToChat($"{ChatUtils.FormatMessage(_plugin.Config.ChatPrefix)} {ChatColors.Red}{friendlyName}{ChatColors.Default} está limitada a {ChatColors.Green}{limit}{ChatColors.Default} por time!");
    }
    
    #endregion
}
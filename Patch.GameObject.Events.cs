using System;
using System.Collections.Generic;
using System.Linq;

namespace HawkSoft.BetterAbilityBar {

  using QudEvent = XRL.World.Event;
  using QudGameObject = XRL.World.GameObject;
  using ActivatedAbilities = XRL.World.Parts.ActivatedAbilities;
  using static HawkSoft.BetterAbilityBar.Constants;

  /// <summary>
  /// Patches for various methods of `GameObject` to catch ability usages.
  /// Unfortunately, this seems to be the best hooking point I could find, as all abilities
  /// are executed through the global event system.
  /// </summary>
  public static class Patch_GameObject_Events {

    public struct State {
      public string eventID;
    }

    public static IEnumerable<PatchData> Patches {
      get {
        var typePatch = typeof(Patch_GameObject_Events);
        var typeEvent = typeof(QudEvent);
        
        var methodPrefix = new MethodData(typePatch, Sources.Prefix);
        var methodPostfix = new MethodData(typePatch, Sources.Postfix);
        var builder = MethodData.Builder(typeof(QudGameObject));

        yield return new PatchData(builder.Method(Targets.FireEvent, typeEvent), methodPrefix, methodPostfix);
        yield return new PatchData(builder.Method(Targets.FireEventDirect, typeEvent), methodPrefix, methodPostfix);
        yield return new PatchData(builder.Method(Targets.BroadcastEvent, typeEvent), methodPrefix, methodPostfix);
      }
    }

    public static bool Prefix(out State __state, QudEvent E) {
      // Events can mutate under your nose, so I need to store the original event ID in case this `Event` instance
      // mysteriously changes into a different one.
      __state = new State { eventID = E.ID };
      return true;
    }

    public static void Postfix(QudGameObject __instance, State __state) {
      if (!__instance.IsPlayer()) return;

      var game = Mod.Game;
      var activatedAbilities = game.Player.Body?.GetPart(nameof(ActivatedAbilities)) as ActivatedAbilities;
      if (activatedAbilities == null) return;

      // No easy access to the command that triggers abilities.
      // Have to search through all the player's abilities for a match instead.
      var ability = activatedAbilities.AbilityByGuid.Values
        .FirstOrDefault(v => v.Command == __state.eventID);
      
      if (ability == null) return;

      var data = AbilityUsageGameState.Instance.Data;
      var usage = default(AbilityUsageEntry);
      if (data.TryGetValue(ability.Command, out usage)) {
        usage.AddEntry(game.Turns);
        GameLog.Info($"Updated `AbilityUsageEntry` for {ability.DisplayName}; avg = {usage.Average}");
      }
      else {
        usage = new AbilityUsageEntry();
        usage.AddEntry(game.Turns);
        data[ability.Command] = usage;
        GameLog.Info($"Created `AbilityUsageEntry` for {ability.DisplayName}; avg = {usage.Average}");
      }
    }

  }

}
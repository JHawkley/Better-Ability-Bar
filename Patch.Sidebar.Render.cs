using System;
using System.Collections.Generic;
using System.Linq;

namespace HawkSoft.BetterAbilityBar {

  using ScreenBuffer = ConsoleLib.Console.ScreenBuffer;
  using Keyboard = ConsoleLib.Console.Keyboard;
  using MessageQueue = XRL.Messages.MessageQueue;
  using AbilityManager = XRL.UI.AbilityManager;
  using Sidebar = XRL.UI.Sidebar;
  using ActivatedAbilities = XRL.World.Parts.ActivatedAbilities;
  using ActivatedAbilityEntry = XRL.World.Parts.ActivatedAbilityEntry;
  using static HawkSoft.BetterAbilityBar.Constants;

  /// <summary>
  /// Patches the `Sidebar` to re-render the ability text the way I want it after it is finished rendering.
  /// </summary>
  public static class Patch_Sidebar_Render {

    public struct State {
      public bool shouldRender;
      public string originalText;
    }

    public static IEnumerable<PatchData> Patches {
      get {
        var typePatch = typeof(Patch_Sidebar_Render);

        yield return new PatchData(
          new MethodData(typeof(Sidebar), Targets.Render, typeof(ScreenBuffer)),
          new MethodData(typePatch, Sources.Prefix),
          new MethodData(typePatch, Sources.Postfix)
        );
      }
    }

    public static bool Prefix(out State __state) {
      // Okay, I have no idea...  It's a hell of a heisenbug.
      // Sometimes the `Postfix` will not complete.  No errors; it just stops and doesn't assign the new text I want.
      // This causes momentary flickers of the old text to reassert itself periodically.  But, if I store the previous
      // version of the text and replace `AbilityText` early, after the vanilla render but before I generate my new
      // text, it hides this issue.
      //
      // Is the render thread getting forcibly aborted if it takes too long?  That would be insanity!

      // There are some states that the vanilla renderer does not perform an update during.  We'll respect this and
      // indicate we need to abort our render by setting `__state.shouldRender` to `false`.

      __state = new State {
        shouldRender = GameManager.bDraw != 23 && !Keyboard.bAlt && GameManager.bDraw != 24 && MessageQueue.bShowUnityTopInfo,
        originalText = Sidebar.AbilityText
      };
      return true;
    }


    public static void Postfix(ref State __state) {
      if (!__state.shouldRender) return;

      // Omfg, this is so dumb.  I don't know who to blame.
      Sidebar.AbilityText = __state.originalText;

      var activatedAbilities = Mod.Game.Player.Body?.GetPart(nameof(ActivatedAbilities)) as ActivatedAbilities;
      if (activatedAbilities == null) return;

      Sidebar.SB.Length = 0;
      RenderAllAbilities(activatedAbilities.AbilityByGuid);
      Sidebar.AbilityText = Sidebar.SB.ToString();
    }

    private static void RenderAllAbilities(Dictionary<Guid, ActivatedAbilityEntry> abilityByGuid) {
      if (abilityByGuid == null || abilityByGuid.Count == 0) return;

      foreach (var ability in abilityByGuid.Values.OrderBy(v => v, AbilityComparer.Instance))
        RenderSingleAbility(ability);
    }

    private static void RenderSingleAbility(ActivatedAbilityEntry activatedAbilityEntry) {
      // Pretty much a copy-paste from the disassembled code.
      string value = "<color=white>";
      string value2 = "yellow";
      if (activatedAbilityEntry.Cooldown > 0) {
        value = "<color=grey>";
        Sidebar.SB.Append(value);
        Sidebar.FormatToRTF(activatedAbilityEntry.DisplayName, Sidebar.SB, "FF");
        Sidebar.SB.Append(" [");
        Sidebar.SB.Append((int) Math.Ceiling((double) ((float) activatedAbilityEntry.Cooldown / 10f)));
        Sidebar.SB.Append("]");
        value2 = "grey";
      }
      else {
        if (!activatedAbilityEntry.Enabled) {
          value = "<color=grey>";
          value2 = "grey";
        }
        if (activatedAbilityEntry.Toggleable && !activatedAbilityEntry.ToggleState) {
          value = "<color=red>";
        }
        if (activatedAbilityEntry.Toggleable && activatedAbilityEntry.ToggleState) {
          value = "<color=green>";
        }
        Sidebar.SB.Append(value);
        Sidebar.FormatToRTF(activatedAbilityEntry.DisplayName, Sidebar.SB, "FF");
      }
      Sidebar.SB.Append("</color>");
      if (!string.IsNullOrEmpty(activatedAbilityEntry.Command) && AbilityManager.commandToKey.ContainsKey(activatedAbilityEntry.Command)) {
        Sidebar.SB.Append(" <<color=");
        Sidebar.SB.Append(value2);
        Sidebar.SB.Append(">");
        Keyboard.MetaToString(AbilityManager.commandToKey[activatedAbilityEntry.Command], Sidebar.SB);
        Sidebar.SB.Append("</color>>");
      }
      Sidebar.SB.Append("    ");
    }

  }

}
using System;
using System.Collections.Generic;
using Harmony;

namespace HawkSoft.BetterAbilityBar {

  using static HawkSoft.BetterAbilityBar.Constants;

  /// <summary>
  /// Handles the patching of the game.
  /// </summary>
  public static class Mod {

    public static XRL.Core.XRLCore Core => XRL.Core.XRLCore.Core;
    public static XRL.XRLGame Game => XRL.Core.XRLCore.Core.Game;

    private static IEnumerable<PatchData> Patches {
      get {
        foreach (var patch in Patch_GameObject_Events.Patches) yield return patch;
        foreach (var patch in Patch_Sidebar_Render.Patches) yield return patch;
      }
    }

    static Mod() {
      BuildLog.Info("Starting initialization.");
      if (!HarmonyInjector.Injector.HarmonyInjected) {
        BuildLog.Error("Cannot initialize: not currently in a Harmony-injected context.");
        return;
      }

      BuildLog.Info("Creating local Harmony instance...");
      harmony = HarmonyInstance.Create(ModID);

      try {
        // Begin applying patches.
        foreach (var patchData in Patches) {
          BuildLog.Info($"Applying: {patchData}");
          patchData.Patch(harmony);
        }

        BuildLog.Info("Initialization successful.");
      }
      catch (Exception initEx) {
        BuildLog.Error($"Exception of type `{initEx.GetType().FullName}` was raised during initialization.");
        BuildLog.Error(initEx);

        // Unpatch seems to fail with the current version of
        BuildLog.Info("Reversing all Harmony patches that were applied by this mod before the error.");
        foreach (var patchData in Patches) patchData.Unpatch(harmony);
        
        BuildLog.Info("Initialization failed.");
        throw;
      }
    }

    private static readonly HarmonyInstance harmony;

  }
}
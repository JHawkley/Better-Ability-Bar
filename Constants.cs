using System;

namespace HawkSoft.BetterAbilityBar {

  /// <summary>
  /// Constants used throughout the mod.
  /// </summary>
  public static class Constants {

    // The new version of CoQ comes with a new logging system based on
    // a package called NLog.  For whatever reason, it does not appear
    // to work when called from mods, and I can't figure out why.
    // So, if that ever changes, this can be used to switch between
    // the two logging systems.
    public const bool UseNewLogging = false;

    public const bool PerformDebugging = false;
    public const int MaxAbilityUsageEntries = 10;

    public const string ModID = "HawkSoft.BetterAbilityBar";
    public const string AbilityUsageKey = "abilityUsageGameState";

    public static readonly object[] EmptyArgs = new object[0];

    /// <summary>
    /// Constants for method-names that are the targets of patches.
    /// </summary>
    public static class Targets {
      public const string FireEvent = "FireEvent";
      public const string FireEventDirect = "FireEventDirect";
      public const string BroadcastEvent = "BroadcastEvent";
      public const string Render = "Render";
      public const string PlayerBody = "PlayerBody";
    }

  }

}
namespace HawkSoft.BetterAbilityBar {
  /// <summary>
  /// Lets you know it's started up.
  /// </summary>
  public static class Mod {

    public static XRL.Core.XRLCore Core => XRL.Core.XRLCore.Core;
    public static XRL.XRLGame Game => XRL.Core.XRLCore.Core.Game;

    static Mod() {
      BuildLog.Info("Starting initialization.");
    }

  }
}
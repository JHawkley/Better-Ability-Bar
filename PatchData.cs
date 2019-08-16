using System;
using Harmony;

namespace HawkSoft.BetterAbilityBar {

  /// <summary>
  /// Data structure for a patch to be applied.
  /// </summary>
  public class PatchData {

    private string PatchType {
      get {
        if (prefix.IsDefined && postfix.IsDefined) return "Full Patch";
        if (prefix.IsDefined) return "Prefix Patch";
        if (postfix.IsDefined) return "Postfix Patch";
        return "Null Patch";
      }
    }

    public PatchData(MethodData target, IMethodData prefix = null, IMethodData postfix = null) {
      if (target == null) throw new ArgumentNullException(nameof(target));

      this.target = target;
      this.prefix = prefix ?? MethodData.Empty;
      this.postfix = postfix ?? MethodData.Empty;
    }

    public PatchData Patch(HarmonyInstance harmony) {
      harmony.Patch(target.Info, prefix.Harmonized, postfix.Harmonized);
      return this;
    }

    public void Unpatch(HarmonyInstance harmony) {
      // Unpatch in reverse order; postfix then prefix.
      // Unpatching still seems to be a somewhat janky feature in Harmony and may be
      // improved in version 2.  For now, we'll just wrap it in `try-catch` blocks.
      try { if (postfix.IsDefined) harmony.Unpatch(target.Info, postfix.Info); } catch { }
      try { if (prefix.IsDefined) harmony.Unpatch(target.Info, prefix.Info); } catch { }
    }

    public override string ToString() => $"{PatchType} for `{target}`";

    public readonly MethodData target;
    public readonly IMethodData prefix;
    public readonly IMethodData postfix;

  }

}
using System;
using System.Linq;
using System.Reflection;
using Harmony;

namespace HawkSoft.BetterAbilityBar {

  /// <summary>
  /// Interface for describing a method to be used in patching.
  /// </summary>
  public interface IMethodData {
    bool IsDefined { get; }
    MethodInfo Info { get; }
    HarmonyMethod Harmonized { get; }
  }

  /// <summary>
  /// Simplifies the building of `MethodData` objects when multiple methods on a single type are needed.
  /// </summary>
  public class MethodDataBuilder {
    public MethodDataBuilder(Type type) { this.type = type; }
    public MethodData Method(string method) => new MethodData(type, method);
    public MethodData Method(string method, params Type[] arity) => new MethodData(type, method, arity);
    private readonly Type type;
  }

  /// <summary>
  /// Data structure for a method to be used in patching.
  /// </summary>
  public class MethodData : IMethodData {

    /// <summary>
    /// Null-Object for MethodData.
    /// </summary>
    private class EmptyMethodData : IMethodData {
      public bool IsDefined => false;
      public MethodInfo Info => null;
      public HarmonyMethod Harmonized => null;
      public override string ToString() => "<MethodData.Empty>";
    }

    public static IMethodData Empty = new EmptyMethodData();

    public bool IsDefined => true;

    public MethodInfo Info {
      get {
        if (methodInfo != null) return methodInfo;

        methodInfo
          = arity == null
          ? type.GetMethod(method)
          : type.GetMethod(method, arity);
        
        return methodInfo;
      }
    }

    public HarmonyMethod Harmonized {
      get {
        if (harmonyMethod != null) return harmonyMethod;

        harmonyMethod = new HarmonyMethod(Info);
        return harmonyMethod;
      }
    }

    public MethodData(Type type, string method, params Type[] arity) {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (method == null) throw new ArgumentNullException(nameof(method));

      this.type = type;
      this.method = method;
      this.arity = arity;
    }

    public MethodData(Type type, string method) : this(type, method, default(Type[])) {}

    public override string ToString() {
      if (arity == null) return $"{type.FullName}::{method}";
      return $"{type.FullName}::{method}({String.Join(", ", arity.Select(t => t.Name).ToArray())})";
    }

    public static MethodDataBuilder Builder(Type type) => new MethodDataBuilder(type);

    public readonly Type type;
    public readonly string method;
    public readonly Type[] arity;

    private MethodInfo methodInfo = null;
    private HarmonyMethod harmonyMethod = null;

  }

}
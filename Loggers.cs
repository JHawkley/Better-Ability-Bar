#pragma warning disable CS0162,IDE0060

using System;

namespace HawkSoft.BetterAbilityBar {

  using UnityDebug = UnityEngine.Debug;
  using QudDebug = global::Logger;
  using static HawkSoft.BetterAbilityBar.Constants;

  /// <summary>
  /// The debug logger for game-mode.
  /// Will be noops unless `Constants.PerformDebugging` is `true`.
  /// </summary>
  public static class GameLog {

    public static void Info(string message) {
      if (!PerformDebugging) return;
      if (UseNewLogging) QudDebug.gameLog.Info(Format(message), EmptyArgs);
      else UnityDebug.Log(Format(message, "Info"));
    }

    public static void Error(string message) {
      if (!PerformDebugging) return;
      if (UseNewLogging) QudDebug.gameLog.Error(Format(message), EmptyArgs);
      else UnityDebug.LogError(Format(message, "Error"));
    }

    public static void Error(Exception exception) {
      if (!PerformDebugging) return;
      Error(exception.ToLogMessage());
    }

    private static string Format(object message) =>
      $"[{ModID}] {message}";

    private static string Format(object message, string level) =>
      $"[{ModID}::Game {level}] {message}";

  }

  /// <summary>
  /// The debug logger for mod initialization.
  /// </summary>
  public static class BuildLog {

    public static void Info(string message) {
      if (UseNewLogging) QudDebug.buildLog.Info(Format(message), EmptyArgs);
      else UnityDebug.Log(Format(message, "Info"));
    }

    public static void Error(string message) {
      if (UseNewLogging) QudDebug.buildLog.Error(Format(message), EmptyArgs);
      else UnityDebug.LogError(Format(message, "Error"));
    }

    public static void Error(Exception exception) {
      Error(exception.ToLogMessage());
    }

    private static string Format(object message) =>
      $"[{ModID}] {message}";

    private static string Format(object message, string level) =>
      $"[{ModID}::Build {level}] {message}";

  }

}
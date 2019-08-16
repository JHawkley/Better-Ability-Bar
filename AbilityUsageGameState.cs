using System;
using System.Collections.Generic;
using System.Linq;

namespace HawkSoft.BetterAbilityBar {

  using GamestateSingleton = XRL.GamestateSingleton;
  using IGamestateSingleton = XRL.IGamestateSingleton;
  using IObjectGamestateCustomSerializer = XRL.IObjectGamestateCustomSerializer;
  using SerializationReader = XRL.World.SerializationReader;
  using SerializationWriter = XRL.World.SerializationWriter;
  using ActivatedAbilityEntry = XRL.World.Parts.ActivatedAbilityEntry;
  using static HawkSoft.BetterAbilityBar.Constants;

  /// <summary>
  /// Handles save file serialization of ability usage.
  /// </summary>
  [GamestateSingleton(AbilityUsageKey)]
  public class AbilityUsageGameState : IGamestateSingleton, IObjectGamestateCustomSerializer {

    public Dictionary<string, AbilityUsageEntry> Data { get; } = new Dictionary<string, AbilityUsageEntry>();

    public static AbilityUsageGameState Instance {
      get {
        var game = Mod.Game;
        var inst = game.GetObjectGameState(AbilityUsageKey) as AbilityUsageGameState;
        if (inst != null) return inst;

        inst = new AbilityUsageGameState();
        game.SetObjectGameState(AbilityUsageKey, inst);
        return inst;
      }
    }

    public IGamestateSingleton GameLoad(SerializationReader reader) {
      var count = reader.ReadInt32();
      for (int i = 0; i < count; i++)
        Data.Add(reader.ReadString(), AbilityUsageEntry.Load(reader));
      return this;
    }

    public void GameSave(SerializationWriter writer) {
      writer.Write(Data.Count);
      foreach (var kvp in Data) {
        writer.Write(kvp.Key);
        kvp.Value.Save(writer);
      }
    }

    public void init() {}
    public void worldBuild() {}
  }

  /// <summary>
  /// Data structure representing the player's ability usage.
  /// </summary>
  public class AbilityUsageEntry {

    public long Average { get; private set; } = 0L;

    public static AbilityUsageEntry Load(SerializationReader reader) {
      var instance = new AbilityUsageEntry();

      instance.Average = reader.ReadInt64();
      var count = reader.ReadInt32();
      for (var i = 0; i < count; i++)
        instance.history.Add(reader.ReadInt64());

      return instance;
    }

    public void Save(SerializationWriter writer) {
      writer.Write(Average);
      writer.Write(history.Count);
      for (int i = 0, len = history.Count; i < len; i++)
        writer.Write(history[i]);
    }

    public void AddEntry(long turns) {
      history.Add(turns);
      if (history.Count > MaxAbilityUsageEntries) history.RemoveAt(0);
      Average = AverageList(history);
    }

    // The list caps out at a fixed number of items.
    // If there are not enough items in the list, then it is like the missing items were all 0.
    // This will allow a more gentle rise in the abilities list.
    private static long AverageList(List<long> list) => list.Sum() / MaxAbilityUsageEntries;

    private readonly List<long> history = new List<long>(MaxAbilityUsageEntries + 1);

  }

  /// <summary>
  /// Comparer that performs the sorting of abilities in the sidebar.
  /// </summary>
  public class AbilityComparer : IComparer<ActivatedAbilityEntry> {

    public static AbilityComparer Instance {
      get {
        if (instance == null)
          instance = new AbilityComparer();
        return instance;
      }
    }

    private static Dictionary<string, AbilityUsageEntry> AbilityUsage =>
      AbilityUsageGameState.Instance.Data;

    public int Compare(ActivatedAbilityEntry a, ActivatedAbilityEntry b) {
      // Sort abilities on cooldown up, prioritizing cooldowns closest to expiring.
      var aOnCooldown = a.Cooldown > 0;
      var bOnCooldown = b.Cooldown > 0;
      if (aOnCooldown != bOnCooldown) return aOnCooldown ? -1 : 1;
      if (aOnCooldown && bOnCooldown) {
        var diff = a.Cooldown - b.Cooldown;
        if (diff != 0) return diff;
      }

      // Sort toggleables down to the end.  Who cares about these?
      if (a.Toggleable != b.Toggleable) return b.Toggleable ? -1 : 1;

      // Sort by the most commonly used skills.
      var aUsage = GetAverageUsage(a.Command);
      var bUsage = GetAverageUsage(b.Command);
      if (aUsage != bUsage) return aUsage > bUsage ? -1 : 1;

      // Finally, sort by name.
      return String.Compare(a.DisplayName, b.DisplayName);
    }

    private static long GetAverageUsage(string command) {
      AbilityUsageEntry usage;
      return AbilityUsage.TryGetValue(command, out usage) ? usage.Average : long.MinValue;
    }

    private static AbilityComparer instance = null;

  }

}
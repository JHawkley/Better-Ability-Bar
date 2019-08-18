using System;
using System.Linq;
using System.Text;

namespace HawkSoft.BetterAbilityBar {

  public static class Tools {

    /// <summary>
    /// Describes an exception in a log-friendly way.
    /// </summary>
    /// <param name="exception">The exception to build a message for.</param>
    /// <returns>A log-friendly exception message.</returns>
    public static string ToLogMessage(this Exception exception) {
      var sb = new StringBuilder();
      sb.Append("Exception report...");
      sb.AppendLine().AppendLine();
      sb.Append(exception.ToString());

      if (exception.Data.Count > 0) {
        // List out the custom data stored in the exception.
        sb.AppendLine().AppendLine();
        sb.Append("Custom exception data:");

        var de = exception.Data.GetEnumerator();
        while (de.MoveNext()) {
          // Indent the data, if it has new-lines.
          var value = de.Value.ToString().Replace("\n", "\n    ");
          var data = $"  {de.Key.ToString()} => {value}";
          sb.AppendLine().Append(data);
        }
      }

      return sb.ToString();
    }

  }

}
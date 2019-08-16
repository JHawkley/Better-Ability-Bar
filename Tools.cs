using System;

namespace HawkSoft.BetterAbilityBar {

  public static class Tools {

    /// <summary>
    /// Describes an exception in a log-friendly way.
    /// </summary>
    public static string ToLogMessage(this Exception exception) => String.Concat(
      "Exception report...",
      "\r\n\r\n",
      exception.ToString()
    );

  }

}
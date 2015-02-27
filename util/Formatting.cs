﻿using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NMaier.SimpleDlna.Utilities
{
  public static class Formatting
  {
    private readonly static Regex sanitizer = new Regex(
      @"\b(?:the|an?|ein(?:e[rs]?)?|der|die|das)\b",
      RegexOptions.IgnoreCase | RegexOptions.Compiled
      );

    private readonly static Regex trim = new Regex(
      @"\s+|^[._+)}\]-]+|[._+({\[-]+$",
      RegexOptions.Compiled
      );

    private readonly static Regex trimmore =
      new Regex(@"^[^\d\w]+|[^\d\w]+$", RegexOptions.Compiled);

    private readonly static Regex respace =
      new Regex(@"[.+]+", RegexOptions.Compiled);

    public static string FormatFileSize(this long size)
    {
      if (size < 900) {
        return string.Format("{0} B", size);
      }
      var ds = size / 1024.0;
      if (ds < 900) {
        return string.Format("{0:F2} KB", ds);
      }
      ds /= 1024.0;
      if (ds < 900) {
        return string.Format("{0:F2} MB", ds);
      }
      ds /= 1024.0;
      if (ds < 900) {
        return string.Format("{0:F3} GB", ds);
      }
      ds /= 1024.0;
      if (ds < 900) {
        return string.Format("{0:F3} TB", ds);
      }
      ds /= 1024.0;
      return string.Format("{0:F4} PB", ds);
    }

    public static string StemCompareBase(this string name)
    {
      if (name == null) {
        throw new ArgumentNullException("name");
      }

      var san = trimmore.Replace(
        sanitizer.Replace(name, string.Empty),
        string.Empty).Trim();
      if (string.IsNullOrWhiteSpace(san)) {
        return name;
      }
      return san.StemNameBase();
    }

    public static string StemNameBase(this string name)
    {
      if (name == null) {
        throw new ArgumentNullException("name");
      }

      if (!name.Contains(" ")) {
        name = name.Replace('_', ' ');
        if (!name.Contains(" ")) {
          name = name.Replace('-', ' ');
        }
        name = respace.Replace(name, " ");
      }
      var ws = name;
      var wsprev = name;
      do {
        wsprev = ws;
        ws = trim.Replace(wsprev.Trim(), " ").Trim();
      }
      while (wsprev != ws);
      if (string.IsNullOrWhiteSpace(ws)) {
        return name;
      }
      return ws;
    }

    public static string GetSystemName()
    {
      var buf = Marshal.AllocHGlobal(8192);
      // This is a hacktastic way of getting sysname from uname ()
      if (SafeNativeMethods.uname(buf) != 0) {
        throw new ArgumentException("Failed to get uname");
      }
      var rv = Marshal.PtrToStringAnsi(buf);
      Marshal.FreeHGlobal(buf);
      return rv;
    }
  }
}

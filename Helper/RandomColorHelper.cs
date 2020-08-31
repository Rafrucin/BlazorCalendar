using System;

namespace BlazorCalendar.Helpers
{
 public class RandomColorHelper
 {
  private static Random random = new Random();
  private static string[] colorClasses = new[]{"magenta", "yellow", "pink-red", "red-orange", "yellow-green"};

  public static string GetRandomColorString()
  {
   return colorClasses[random.Next(0,colorClasses.Length)];
  }
 }
}


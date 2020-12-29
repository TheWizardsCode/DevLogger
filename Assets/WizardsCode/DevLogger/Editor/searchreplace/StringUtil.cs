using System.Text;
using System;

namespace sr
{
  public class StringUtil
  {
    public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
    {
        StringBuilder sb = new StringBuilder();

        int previousIndex = 0;
        int index = str.IndexOf(oldValue, comparison);
        while (index != -1)
        {
            sb.Append(str.Substring(previousIndex, index - previousIndex));
            sb.Append(newValue);
            index += oldValue.Length;

            previousIndex = index;
            index = str.IndexOf(oldValue, index, comparison);
        }
        sb.Append(str.Substring(previousIndex));

        return sb.ToString();
    }

    public static string Prettify(Conditional c )
    {
      switch(c)
      {
        case Conditional.Equals:
        return "==";
        case Conditional.NotEquals:
        return "!=";
        case Conditional.Any:
        return "Any";
      }
      throw new Exception("Conditional:"+c+" unknown.");
    }

    public static string Unescape(string str)
    {
      if(str == null)
      {
        return "";
      }
      str = str.Replace("\\\\", "\\");
      str = str.Replace("\\r", "\r");
      str = str.Replace("\\n", "\n");
      return str;
    }

    public static string Escape(string str)
    {
      if(str == null)
      {
        return "";
      }
      str = str.Replace("\\", "\\\\");
      str = str.Replace("\r", "\\r");
      str = str.Replace("\n", "\\n");
      return str;
    }

  }
}
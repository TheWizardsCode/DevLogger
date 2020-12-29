namespace sr
{
  public class SRVersion
  {
    public static string BuildNumber = "4";
    public static int MajorVersion = 1;
    public static string MinorVersion = "10";
    public static string GetVersion()
    {
      return string.Format("{0}.{1}.{2}", MajorVersion, MinorVersion, BuildNumber);
    }
  } 
}
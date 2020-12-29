using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;

namespace sr
{
  /**
   * Utility class for serializing or deserializing classes using C#'s built-in
   * serialization formats.
   */
  public class SerializationUtil
  {
    public static void Serialize(string path, Object o)
    {
      string dirPath = Path.GetDirectoryName(path);
      if(!Directory.Exists(dirPath))
      {
        Directory.CreateDirectory(dirPath);
      }
      FileStream fs = new FileStream(path, FileMode.Create);
      BinaryFormatter formatter = new BinaryFormatter();
      try
      {
        formatter.Serialize(fs, o);
      }
      catch(SerializationException e)
      {
        UnityEngine.Debug.Log("Failed to serialize:"+ e.Message);
        throw;
      }
      finally
      {
        fs.Close();
      }
    }

    public static System.Object Copy(Object o)
    {
      using(MemoryStream ms = new MemoryStream())
      {
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
          formatter.Serialize(ms, o);
          ms.Position = 0;
          return formatter.Deserialize(ms);
        }
        catch(SerializationException e)
        {
          UnityEngine.Debug.Log("Failed to serialize:"+ e.Message);
          throw;
        }

      }
    }

    public static System.Object Deserialize(string path)
    {
      if(File.Exists(path))
      {
        FileStream fs = new FileStream(path, FileMode.Open);
        try
        {
          BinaryFormatter formatter = new BinaryFormatter();
          return formatter.Deserialize(fs);
        }
        catch(SerializationException e)
        {
          UnityEngine.Debug.Log("[SerializationUtil] failed to deserialize:"+e.Message);
          throw;
        }
        finally
        {
          fs.Close();
        }
      }else{
        return null;
      }
    }
  }
}
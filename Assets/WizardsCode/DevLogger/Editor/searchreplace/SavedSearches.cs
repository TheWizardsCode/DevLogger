using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

namespace sr
{
  /**
   * A singleton/serializable representation for getting the user's
   * currently saved searches.
   */
  [System.Serializable]
  public class SavedSearches
  {
    public List<SavedSearch> searches = new List<SavedSearch>();

    static string path = "";

    public static void initSearchPath()
    { 
      path = Application.persistentDataPath+"/searchreplace/savedSearches";
    }

    public static SavedSearches PopulateFromDisk()
    {
      SavedSearches ss = null;
      initSearchPath();
      try{
        ss = (SavedSearches)SerializationUtil.Deserialize(path);
        if(ss != null)
        {
          ss.OnDeserialization();
        }

      }catch(System.Exception ex)
      {
        Debug.LogError("An error occurred deserializing saved searches. If this issue persists please report the problem to support@enemyhideout.com. Error: "+ ex);
      }
      if(ss == null)
      {
        ss = new SavedSearches();
        ss.OnDeserialization();
        return ss;
      }else{
        return ss;
      }
    }

    public void OnDeserialization()
    {
      //nothing for now?
    }

    public void AddSearch(SavedSearch search)
    {
      searches.Insert(0, search);
      Persist();
    }

    public void Persist()
    {
      SerializationUtil.Serialize(path, this);
    }
  }
}
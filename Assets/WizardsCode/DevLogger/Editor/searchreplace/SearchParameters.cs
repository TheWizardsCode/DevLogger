using UnityEngine;
using UnityEditor;

namespace sr
{

  public class SearchParameters
  {
    public SerializedProperty prop;
    public SearchAssetData assetBeingSearched;
    
    public string Message { get; private set;}

    public SearchParameters()
    {
      Message = "";
    }

    public void ActionTaken(string message)
    {
      Message = message;
    }

    public override string ToString()
    {
      return "[SearchParameters prop=" + prop.propertyPath + ", asset=" + assetBeingSearched  +"]";
    }
  }
}
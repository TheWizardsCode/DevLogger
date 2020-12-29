using UnityEngine;
namespace sr
{
  /** 
   * A serialized representation of two ObjectIDs. Used when searching for 
   * prefab instances.
   */
  [System.Serializable]
  public class ObjectIDTuple
  {
    public ObjectID First;
    public ObjectID Second;

    public ObjectIDTuple(ObjectID f, ObjectID s)
    {
      First = f;
      Second = s;
    }

    public UnityEngine.Object FirstObj()
    {
      return First == null ? null : First.obj;
    }

    public UnityEngine.Object SecondObj()
    {
      return Second == null ? null : Second.obj;
    }

    public ObjectIDTuple()
    {
      First = new ObjectID();
      Second = new ObjectID();
    }

    public void Swap()
    {
      ObjectID tmp = First;
      First = Second;
      Second = tmp;
    }

    public void OnDeserialization()
    {
      if(First == null)
      {
        First = new ObjectID();
      }
      First.OnDeserialization();
      if(Second == null)
      {
        Second = new ObjectID();
      }
      Second.OnDeserialization();
    }
  }

}
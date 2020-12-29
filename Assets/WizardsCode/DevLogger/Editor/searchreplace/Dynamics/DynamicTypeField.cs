using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace sr
{
  /**
   * Provides an interface to DynamicTypeData objects to Search Items.
   * This negotiates the type of an object within a Search Item and sets the 
   * typeData member variable to the correct subclass.
   */
  [System.Serializable]
  public class DynamicTypeField
  {
    // The searchItem that this field is inside.
    [System.NonSerialized]
    public SearchItem searchItem;

    // The type that we are currently interested in.
    [System.NonSerialized]
    public Type type;

    // A hash of types to the fields that can display information about them.
    [System.NonSerialized]
    Dictionary<Type, DynamicTypeData> typeHash;

    // The current DTD that can handle the type we are interested in.
    [System.NonSerialized] 
    public DynamicTypeData typeData;

    // Whether we should show more advanced options for the given search.
    public bool showMoreOptions = true;

    // Note: These are all serialized as part of this class, and initialized
    // upon deserialization. Why? Because we can remember previously saved values
    // as the user changes search values.
    DynamicTypeString dynamicString;
    DynamicTypeObject dynamicObject;
    DynamicTypeFloat dynamicFloat;
    DynamicTypeInt dynamicInt;
    DynamicTypeLong dynamicLong;
    DynamicTypeDouble dynamicDouble;
    DynamicTypeBool dynamicBool;
    DynamicTypeChar dynamicChar;
    DynamicTypeVector2 dynamicVector2;
    DynamicTypeVector3 dynamicVector3;
    DynamicTypeVector4 dynamicVector4;
    DynamicTypeRect dynamicRect;
    DynamicTypeColor dynamicColor;
    DynamicTypeQuaternion dynamicQuat;
    DynamicTypeEnum dynamicEnum;
    DynamicTypeCollection dynamicCollection;
    DynamicCustomData dynamicCustomData;
    DynamicTypePropSheet dynamicTypePropSheet;
    DynamicTypeClass dynamicTypeClass;

    public int depth;

    // Initializes our DTDs and creates our typeHash. All these null checks
    // deal with the limitations of C#'s serialization subsystem.
    public void OnDeserialization()
    {
      typeHash = new Dictionary<Type, DynamicTypeData>();

      if(dynamicString == null)
      {
        dynamicString = new DynamicTypeString();
      }
      dynamicString.parent = this;
      dynamicString.OnDeserialization();
      typeHash[typeof(string)] = dynamicString;

      if(dynamicObject == null)
      {
        dynamicObject = new DynamicTypeObject();
      }
      dynamicObject.parent = this;
      dynamicObject.OnDeserialization();
      typeHash[typeof(UnityEngine.Object)] = dynamicObject;

      if(dynamicFloat == null)
      {
        dynamicFloat = new DynamicTypeFloat();
      }
      dynamicFloat.parent = this;
      dynamicFloat.OnDeserialization();

      typeHash[typeof(float)] = dynamicFloat;

      if(dynamicInt == null)
      {
        dynamicInt = new DynamicTypeInt();
      }
      dynamicInt.parent = this;
      dynamicInt.OnDeserialization();
      typeHash[typeof(int)] = dynamicInt;
      typeHash[typeof(uint)] = dynamicInt;
      typeHash[typeof(short)] = dynamicInt;
      typeHash[typeof(ushort)] = dynamicInt;
      typeHash[typeof(byte)] = dynamicInt;
      typeHash[typeof(sbyte)] = dynamicInt;

      if(dynamicLong == null)
      {
        dynamicLong = new DynamicTypeLong();
      }
      dynamicLong.parent = this;
      dynamicLong.OnDeserialization();
      typeHash[typeof(long)] = dynamicLong;
      typeHash[typeof(ulong)] = dynamicLong;

      if(dynamicDouble == null)
      {
        dynamicDouble = new DynamicTypeDouble();
      }
      dynamicDouble.parent = this;
      dynamicDouble.OnDeserialization();
      typeHash[typeof(double)] = dynamicDouble;


      if(dynamicBool == null)
      {
        dynamicBool = new DynamicTypeBool();
      }
      dynamicBool.parent = this;
      dynamicBool.OnDeserialization();
      typeHash[typeof(bool)] = dynamicBool;

      if(dynamicChar == null)
      {
        dynamicChar = new DynamicTypeChar();
      }
      dynamicChar.parent = this;
      dynamicChar.OnDeserialization();
      typeHash[typeof(char)] = dynamicChar;

      if(dynamicVector2 == null)
      {
        dynamicVector2 = new DynamicTypeVector2();
      }
      dynamicVector2.parent = this;
      dynamicVector2.OnDeserialization();
      typeHash[typeof(Vector2)] = dynamicVector2;

 
      if(dynamicVector3 == null)
      {
        dynamicVector3 = new DynamicTypeVector3();
      }
      dynamicVector3.parent = this;
      dynamicVector3.OnDeserialization();
      typeHash[typeof(Vector3)] = dynamicVector3;

      if(dynamicVector4 == null)
      {
        dynamicVector4 = new DynamicTypeVector4();
      }
      dynamicVector4.parent = this;
      dynamicVector4.OnDeserialization();
      typeHash[typeof(Vector4)] = dynamicVector4;

      if(dynamicRect == null)
      {
        dynamicRect = new DynamicTypeRect();
      }
      dynamicRect.parent = this;
      dynamicRect.OnDeserialization();
      typeHash[typeof(Rect)] = dynamicRect;

      if(dynamicColor == null)
      {
        dynamicColor = new DynamicTypeColor();
      }
      dynamicColor.parent = this;
      dynamicColor.OnDeserialization();
      typeHash[typeof(Color)] = dynamicColor;
      typeHash[typeof(Color32)] = dynamicColor;

      if(dynamicQuat == null)
      {
        dynamicQuat = new DynamicTypeQuaternion();
      }
      dynamicQuat.parent = this;
      dynamicQuat.OnDeserialization();
      typeHash[typeof(Quaternion)] = dynamicQuat;

      if(dynamicEnum == null)
      {
        dynamicEnum = new DynamicTypeEnum();
      }
      dynamicEnum.parent = this;
      dynamicEnum.OnDeserialization();
      typeHash[typeof(Enum)] = dynamicEnum;

      if(dynamicCollection == null)
      {
        dynamicCollection = new DynamicTypeCollection();
      }
      dynamicCollection.parent = this;
      dynamicCollection.OnDeserialization();
      typeHash[typeof(Array)] = dynamicCollection;
      typeHash[typeof(List<>)] = dynamicCollection;
      typeHash[typeof(Dictionary<int, string>)] = dynamicCollection;

      if(dynamicCustomData == null)
      {
        dynamicCustomData = new DynamicCustomData();
      }
      dynamicCustomData.parent = this;
      dynamicCustomData.OnDeserialization();

      if(dynamicTypePropSheet == null)
      {
        dynamicTypePropSheet = new DynamicTypePropSheet();
      }
      dynamicTypePropSheet.parent = this;
      typeHash[typeof(PropSheet<>)] = dynamicTypePropSheet;

      dynamicTypePropSheet.OnDeserialization();
      
      if(dynamicTypeClass == null)
      {
        dynamicTypeClass = new DynamicTypeClass();
      }
      dynamicTypeClass.parent = this;
      typeHash[typeof(Type)] = dynamicTypeClass;

      dynamicTypeClass.OnDeserialization();

    }

    public DynamicTypeData GetDTDFor(Type type)
    {
      DynamicTypeData retVal = null;
      typeHash.TryGetValue(type, out retVal);
      return retVal;
    }

    /**
     * When a user takes an action that changes the search, it may change the type.
     * This means we'll need to re-initialize the type of search we're doing.
     * The search item will create an InitializationContext which this class can
     * use to negotiate what DTD to use via various tests and reflection.
     */
    public void SetType(InitializationContext ic)
    {
      Type t = ic.fieldData.fieldType;
      if(t == type && !ic.forceUpdate)
      {
        return; //return early.
      }
      type = t;
      //Save the initial type, but check inheritance to find our editor.
      Type fieldType = type;
      if( typeof(UnityEngine.Object).IsAssignableFrom(type) )
      {
        fieldType = typeof(UnityEngine.Object); 
      }

      if( typeof(System.Enum).IsAssignableFrom(type) )
      {
        fieldType = typeof(System.Enum); 
      }
      if(type.IsArray)
      {
        fieldType = typeof(Array);
      }
      if(type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
      {
        fieldType = typeof(List<>);
      }
      if(type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(PropSheet<>)))
      {
        fieldType = typeof(PropSheet<>);
      }

      typeData = GetDTDFor(fieldType);
      if(typeData == null && type.IsSerializable)
      {
        if(type.ToString().IndexOf("System.") == 0)
        {
          // Debug.Log("[DynamicTypeField] ignoring system type..."+type);
          return;
        }else{
          //Failed finding field!
          // Debug.Log("[DynamicTypeField] Failed finding field for:"+type);
          // Debug.Log("[DynamicTypeField] using custom data.");
          typeData = dynamicCustomData;
        }
      }
      // Debug.Log("[DynamicTypeField] type:"+typeData + " for field:"+fieldType + " fieldData.type:"+t);
      typeData.OnSelect(ic);
    }

    /**
     * The following glue code connects to the similarly named functions in 
     * DynamicTypeData. For more info on these functions check out the DTD
     * comments.
     */
    public void Draw(SearchOptions options) 
    {
      if(typeData != null)
      {
        typeData.showMoreOptions = showMoreOptions;
        typeData.Draw(options);
      }else{
        EditorGUILayout.LabelField(" ", "Cannot search type "+type);
      }
    }

    public bool ValueEquals(SerializedProperty prop)
    {
      if(typeData != null)
      {
        return typeData.ValueEquals(prop);
      }
      return false; 
    }

    public bool ValueEqualsWithConditional(SerializedProperty prop)
    {
      if(typeData != null)
      {
        return typeData.ValueEqualsWithConditional(prop);
      }
      return false; 
    }

    public void SearchProperty(SearchJob job, SearchItem item, SerializedProperty prop)
    {
      if(typeData != null)
      {
        typeData.SearchProperty(job, item, prop);
      }
    }

    public virtual void SearchGameObject( SearchJob job, SearchItem item, GameObject go)
    {
      if(typeData != null)
      {
        typeData.SearchGameObject(job, item, go);
      }
    }

    public virtual void SearchObject( SearchJob job, SearchItem item, UnityEngine.Object obj)
    {
      if(typeData != null)
      {
        typeData.SearchObject(job, item, obj);
      }
    }

    public void OnSearchBegin()
    {
      if(typeData != null)
      {
        typeData.OnSearchBegin();
      }
    }

    public void OnSearchEnd(SearchJob job, SearchItem item)
    {
      if(typeData != null)
      {
        typeData.OnSearchEnd(job, item);
      }
    }

    public void OnAssetSearchBegin(SearchJob job, SearchItem item)
    {
      if(typeData != null)
      {
        typeData.OnAssetSearchBegin(job, item);
      }
    }

    public void OnAssetSearchEnd(SearchJob job, SearchItem item)
    {
      if(typeData != null)
      {
        typeData.OnAssetSearchEnd(job, item);
      }
    }

    public bool IsValid()
    {
      if(typeData != null)
      {
        return typeData.IsValid();
      }
      return false;
    }

    public bool IsReplaceValid()
    {
      if(typeData != null)
      {
        return IsValid() && typeData.IsReplaceValid();
      }
      return false;
    }

    public string StringValue()
    {
      if(typeData != null)
      {
        return typeData.StringValue();
      }
      return string.Empty;
    }

    public string StringValueFor(SerializedProperty prop)
    {
      if(typeData != null)
      {
        return typeData.StringValueFor(prop);
      }
      return string.Empty;
    }

    public string StringValueWithConditional()
    {
      if(typeData != null)
      {
        return typeData.StringValueWithConditional();
      }
      return string.Empty;
    }

    public string GetWarning()
    {
      if(typeData != null)
      {
        return typeData.GetWarning();
      }
      return string.Empty;
    }

    public SerializedPropertyType PropertyType()
    {
      if(typeData != null)
      {
        return typeData.PropertyType();
      }
      return SerializedPropertyType.ArraySize; //Since there is no 'None' Nullable type maybe?
    }

    public virtual bool hasAdvancedOptions()
    {
      if(typeData != null)
      {
        // Debug.Log("[DynamicTypeField] type data!");
        return typeData.hasAdvancedOptions();
      }
      return false; 
    }


    public void ReplaceProperty(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(typeData != null)
      {
        typeData.ReplaceProperty(job, prop, result);
      }
    }

    public GameObject GetGameObject() 
    {
      if(typeData == dynamicObject)
      {
        UnityEngine.Object obj = dynamicObject.searchValue.obj;
        if(obj is GameObject)
        {
          return (GameObject) obj;
        }
      }
      return null;
    }




  }
}
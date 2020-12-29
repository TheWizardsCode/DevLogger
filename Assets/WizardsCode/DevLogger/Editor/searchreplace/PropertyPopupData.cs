using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Text;

namespace sr
{
  /**
   * Provides a popup (ie a dropdown) field that shows a list of searchable properties
   * on an object. This is used by Search Items to parse a given object from a 
   * user into a collection of searchable properties, and then displays the ui
   * for them. Also serializes them.
   */
  [System.Serializable]
  public class PropertyPopupData
  {
    [System.NonSerialized]
    public int index = 0;

    // Because we care less about index and more about the property string
    // this how we determine the current index of the properties.
    public string currentValue = "";
    [System.NonSerialized]
    public FieldData fieldData;

    [System.NonSerialized]
    protected string[] options;
    [System.NonSerialized]
    public string label;

    [System.NonSerialized]
    Dictionary<string, FieldData> fieldHash;

    public void SetType(UnityEngine.Object obj)
    {
      Type s = getTypeFromObject(obj);
      Type script = s;
      fieldHash = new Dictionary<string, FieldData>();
      options = null;
      List<string> fieldNamesList = new List<string>();

      if(obj is GameObject)
      {
        GameObject go = (GameObject) obj;
        Component[] components = go.GetComponents<Component>();
        
        //Remove dupes.
        HashSet<Type> typeSet = new HashSet<Type>();
        List<Component> cList = new List<Component>();
        foreach(Component c in components)
        {
          Type t = getTypeFromObject(c);
          if(!typeSet.Contains(t))
          {
            typeSet.Add(t);
            cList.Add(c);
          }
        }

        foreach(Component c in cList)
        {
          if(c != null)
          {
            Type ct = getTypeFromObject(c);
            string prefix = ct.Name + "/";
            populateOptions(prefix, c, ct, fieldNamesList);
          }
        }
      }

      //Add our properties after the component properties.
      populateOptions("", obj, script, fieldNamesList);

      options = fieldNamesList.ToArray();
      bool foundValue = false;
      for(int i=0; i < fieldNamesList.Count; i++)
      {
        string fieldName = fieldNamesList[i]; 
        if(fieldName == currentValue)
        {
          index = i;
          fieldData = fieldHash[currentValue];
          foundValue = true;
        }
      }
      if(!foundValue)
      {
        if(HasOptions())
        {
          index = 0;
          currentValue = options[index];
          fieldData = fieldHash[currentValue];
        }
      }
    }


    void populateOptions(string prefix, UnityEngine.Object obj, Type objectType, List<string> fieldNamesList)
    {
      if(objectType != null)
      {
        // Debug.Log("[PropertyPopupData] objectType:"+objectType);

        List<FieldData> customFields = FieldDataUtil.GetCustomFieldsForType(objectType);
        foreach(FieldData customField in customFields)
        {
          string customFieldName = prefix + customField.displayName;
          fieldNamesList.Add(customFieldName);
          fieldHash[customFieldName] = customField;
        }

        if(typeof(MonoBehaviour).IsAssignableFrom(objectType))
        {
          // Debug.Log("[PropertyPopupData] is monobehaviour!");
        }else{
          // Debug.Log("[PropertyPopupData] not a monobehaviour...use objectType instead?");

          //This kind of works...but code gen seems to make a better output.
          // only do this if we have no results for codegen.
          if(customFields.Count == 0)
          {

            PropertyInfo[] properties = objectType.GetProperties();
            Dictionary<string, PropertyInfo> propertyMap = new Dictionary<string, PropertyInfo>();
            foreach(PropertyInfo info in properties)
            {
              if(info.Name == "gameObject")
              {
                continue;
              }
              // Debug.Log("[PropertyPopupData] property:"+info.Name);
              propertyMap[info.Name] = info;
            }
            SerializedObject sObj = new SerializedObject(obj);
            SerializedProperty iterator = sObj.GetIterator();
            if(iterator.Next(true))
            {
              bool hasMoreProperties = false;
              do{
                string propName = iterator.name;
                propName = propName.Replace("m_", ""); //TODO: this is bad!
                string firstLetter = propName.Substring(0,1);
                propName = propName.Remove(0,1);
                firstLetter = firstLetter.ToLower();
                propName = firstLetter + propName;
                PropertyInfo pInfo = null;
                if(propertyMap.TryGetValue(propName, out pInfo))
                {
                  FieldData data = new FieldData(pInfo.PropertyType, objectType, iterator.name, propName);
                  string fieldName = prefix + propName;
                  fieldNamesList.Add(fieldName);
                  fieldHash[fieldName] = data;

                }
                hasMoreProperties = iterator.Next(false);
              }while(hasMoreProperties);
            }
          }
        }
        IEnumerable<FieldInfo> fields = null;

        fields = FieldDataUtil.GetAllFields(objectType);
        int i = 0;
        foreach(FieldInfo info in fields)
        { 
          // Debug.Log("[PropertyPopupData] field:"+info.Name + " :  "+ info.FieldType + " public? "+ info.IsPublic);
          StringBuilder sb = new StringBuilder();
          foreach(var attrib in info.GetCustomAttributes(true))
          {
            sb.Append(attrib+" ");
          }
          string attributesString = sb.ToString();
          // Debug.Log("[PropertyPopupData] "+info.Name+" "+attributesString);

          if(info.GetCustomAttributes(typeof(SerializeField), true).Length == 0 && (info.IsPrivate || info.IsFamily || info.IsFamilyOrAssembly))
          {
            // Debug.Log("[PropertyPopupData] ignoring because its private or family, but no SerializeField attribute."+info.Name);
            continue;
          }

          if(info.FieldType.IsInterface)
          {
            // Debug.Log("[PropertyPopupData] ignoring because its an interface:"+info.Name);
            continue;
          }

          // if(info.IsPublic)
          // {
            if(info.IsNotSerialized)
            {
              // Debug.Log("[PropertyPopupData] ignoring because isNotSerialized:"+info.Name);
              continue;
            }
            // Debug.Log("[PropertyPopupData] "+info.Name+" type is serailizable:"+info.FieldType.IsSerializable);
            if(!info.FieldType.IsSerializable)
            {
              // Debug.Log("[PropertyPopupData] type is not serializable:"+info.Name);
              if(!typeof(UnityEngine.Object).IsAssignableFrom(info.FieldType))
              {
                // probably not the most ideal of things here.
                if(attributesString.IndexOf("UsedByNativeCodeAttribute") > -1)
                {
                  // Debug.Log("[PropertyPopupData] good:"+info.Name);
                  //goodness!
                }if(FieldDataUtil.isWhitelisted(info.FieldType))
                {
                  // Internal unity type that is not marked as serializable, but actually is and there's no way around this.
                }else{
                  // Debug.Log("[PropertyPopupData] type isn't serializable! ignoring:"+info.Name);  
                  continue;
                }
              }else{
                // Debug.Log("[PropertyPopupData] "+info.Name + " is assignable from object!");
              }
            }else{
              // The type is serializable! but...that doesn't mean unity likes it!
              // Debug.Log("[PropertyPopupData] is serializable:"+info.Name +"  " + info.FieldType.IsGenericType);
              if(info.FieldType.IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(info.FieldType.GetGenericTypeDefinition()))
              {
                // Debug.Log("[PropertyPopupData] its a dictionary forget it..."+info.Name);
                continue;
              }
              if(FieldDataUtil.isBlacklisted(info.FieldType))
              {
                continue;
              }

            }
          // }
          string key = prefix + info.Name + " ("+info.FieldType.Name+")";
          // Debug.Log("[PropertyPopupData] Making field for:"+key);
          FieldData data =  new FieldData(info.FieldType, objectType, info.Name, info.Name);
          fieldHash[key] = data;
          fieldNamesList.Add(key);
          i++;
        }
      }else{
        // Debug.Log("[PropertyPopupData] object type is null!");
      }
    }

    Type getTypeFromObject(UnityEngine.Object o)
    {
      if(o == null)
      {
        return null;
      }
      if(o is MonoBehaviour)
      {
        MonoScript m = MonoScript.FromMonoBehaviour((MonoBehaviour)o);
        if(m != null)
        {
          return m.GetClass();
        }else{
          return null;
        }
      }else if(o is MonoScript)
      {
        return ((MonoScript)o).GetClass();
      }else if(o is Component)
      {
        return o.GetType();
      }
      return o.GetType();
    }

    public bool HasOptions()
    {
      return options != null && options.Length > 0;
    }

    public Type GetFieldType()
    {
      return fieldData.fieldType;
    }

    public FieldData GetFieldInfo()
    {
      return fieldHash[currentValue];
    }

    public void Draw()
    {
      if(HasOptions())
      {
        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
        int newIndex = EditorGUILayout.Popup(label, index, options, GUILayout.MaxWidth(SRWindow.Instance.position.width - SRWindow.boxPad));
        EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

        if(newIndex != index)
        {
          index = newIndex;
          currentValue = options[index];
          fieldData = fieldHash[currentValue];
          SRWindow.Instance.PersistCurrentSearch();
        }
      }else{
        EditorGUILayout.LabelField(" ", "No properties found."); //the space aligns this field to be correctly aligned with accompanying object fields.
      }
    }


  }
}
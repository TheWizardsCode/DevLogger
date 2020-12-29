using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemWithSeparateProperty : ReplaceItem
  {

    ObjectID objID;

    PropertyPopupData searchProperty;

    [System.NonSerialized]
    InitializationContext initializationContext;

    [System.NonSerialized][HideInInspector]
    UnityEngine.Object draggedObj = null;

    ReplaceItemString replaceItemString;
    ReplaceItemFloat replaceItemFloat;
    ReplaceItemObject replaceItemObject;
    ReplaceItemInt replaceItemInt;
    ReplaceItemLong replaceItemLong;
    ReplaceItemDouble replaceItemDouble;
    ReplaceItemBool replaceItemBool;
    ReplaceItemChar replaceItemChar;
    ReplaceItemVector2 replaceItemVector2;
    ReplaceItemVector3 replaceItemVector3;
    ReplaceItemVector4 replaceItemVector4;
    ReplaceItemRect replaceItemRect;
    ReplaceItemColor replaceItemColor;
    ReplaceItemQuaternion replaceItemQuaternion;
    ReplaceItemEnum replaceItemEnum;

    [System.NonSerialized]
    ReplaceItem subItem;

    // A hash of types to the fields that can display information about them.
    [System.NonSerialized]
    Dictionary<Type, ReplaceItem> typeHash;


    public override void OnDeserialization()
    {
      popupLabel = "Replace Another Property";
      typeHash = new Dictionary<Type, ReplaceItem>();

      if(objID == null)
      {
        objID = new ObjectID();
      }
      objID.OnDeserialization();
      if(searchProperty == null) 
      {
        searchProperty = new PropertyPopupData();
      }
      searchProperty.label = "Property:";

      searchProperty.SetType(objID.obj);
      initializationContext = new InitializationContext(searchProperty.fieldData, objID.obj);
      initSubItem<ReplaceItemString>(new List<Type>(){ typeof(string) }, ref replaceItemString);
      initSubItem<ReplaceItemFloat>(new List<Type>(){ typeof(float) }, ref replaceItemFloat);
      initSubItem<ReplaceItemObject>(new List<Type>(){ typeof(UnityEngine.Object) }, ref replaceItemObject);
      initSubItem<ReplaceItemInt>(new List<Type>(){ typeof(int),typeof(uint),typeof(short),typeof(ushort),typeof(byte),typeof(sbyte), }, ref replaceItemInt);
      initSubItem<ReplaceItemDouble>(new List<Type>(){ typeof(double) }, ref replaceItemDouble);
      initSubItem<ReplaceItemBool>(new List<Type>(){ typeof(bool) }, ref replaceItemBool);
      initSubItem<ReplaceItemChar>(new List<Type>(){ typeof(char) }, ref replaceItemChar);
      initSubItem<ReplaceItemVector2>(new List<Type>(){ typeof(Vector2) }, ref replaceItemVector2);
      initSubItem<ReplaceItemVector3>(new List<Type>(){ typeof(Vector3) }, ref replaceItemVector3);
      initSubItem<ReplaceItemVector4>(new List<Type>(){ typeof(Vector4) }, ref replaceItemVector4);
      initSubItem<ReplaceItemRect>(new List<Type>(){ typeof(Rect) }, ref replaceItemRect);
      initSubItem<ReplaceItemColor>(new List<Type>(){ typeof(Color), typeof(Color32) }, ref replaceItemColor);
      initSubItem<ReplaceItemQuaternion>(new List<Type>(){ typeof(Quaternion) }, ref replaceItemQuaternion);
      initSubItem<ReplaceItemEnum>(new List<Type>(){ typeof(Enum) }, ref replaceItemEnum);

      type = null;
      if(objID.obj != null)
      {
        setType(initializationContext.fieldData.fieldType);
      }
    }

    void initSubItem<T>(IEnumerable<Type> types, ref T subItem) where T : ReplaceItem, new()
    {
      if(subItem == null)
      {
        subItem = new T();
      }
      subItem.OnDeserialization();
      foreach(Type t in types){
        typeHash[t] = subItem;
      }
    }

    public override void Draw()
    {
      if(dtd.parent.searchItem is SearchItemProperty)
      {
        SearchItemProperty propSearch = dtd.parent.searchItem as SearchItemProperty;
        if(objID.obj != propSearch.objID.obj)
        {
          ObjectUtil.ValidateAndAssign(propSearch.objID.obj, objID, searchProperty, ref initializationContext);
        }
      }else{
        showDragAndDrop();
      }
      searchProperty.Draw();
      if(searchProperty.HasOptions())
      { 
        initializationContext.updateFieldData(searchProperty.fieldData);
        // propertyCriteria.SetType(initializationContext);
        initializationContext.forceUpdate = false;

        // SearchOptions typeFieldOptions = options.Copy();
        // if(subsearch != null)
        // {
        //   typeFieldOptions.searchType = SearchType.Search;
        // }

        // propertyCriteria.Draw(typeFieldOptions);
        setType(initializationContext.fieldData.fieldType);
      
        if(subItem != null)
        {
          subItem.options = options;
          subItem.showMoreOptions = showMoreOptions;
          subItem.Draw();
        }else{
          EditorGUILayout.LabelField("Unsupported type:" + type);
        }
      }
    }

    void setType(Type t)
    {
      if(t != type || initializationContext.forceUpdate)
      {
        type = t;
        //Save the initial type, but check inheritance to find our editor.
        Type fieldType = type;
        if( typeof(UnityEngine.Object).IsAssignableFrom(type) )
        {
          fieldType = typeof(UnityEngine.Object);
        }else
        if( typeof(System.Enum).IsAssignableFrom(type) )
        {
          fieldType = typeof(System.Enum); 
        }

        subItem = null;
        if(typeHash.TryGetValue(fieldType, out subItem))
        {
          subItem.type = type;
        }
      }
    }

    void showDragAndDrop()
    {
      Rect r = EditorGUILayout.BeginHorizontal();
      Event e = Event.current;
      bool acceptingDrag = (e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && r.Contains(e.mousePosition);
      if(acceptingDrag)
      {
        if(DragAndDrop.objectReferences.Length == 1)
        {
          UnityEngine.Object firstObj = DragAndDrop.objectReferences[0];
          SRWindow.Instance.Repaint();
          DragAndDrop.AcceptDrag();
          DragAndDrop.visualMode = DragAndDropVisualMode.Link;
          if(e.type == EventType.DragPerform)
          {
            draggedObj = firstObj;
          }
        }else{
          DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        }
      }
      string dragText = null;
      if(objID.obj != null)
      {
        if(objID.obj is MonoScript)
        {
          MonoScript m = (MonoScript)objID.obj;
          dragText = "Currently searching "+m.GetClass().Name +"s.";

        }else{
          dragText = "Currently searching "+objID.obj.GetType().Name +"s.";
        }
        dragText += "\n(Drag an object here to change search)";
      }else{
        dragText = "Drag an object here.";
      }

      if(r.Contains(e.mousePosition) && DragAndDrop.visualMode == DragAndDropVisualMode.Link)
      {
        GUILayout.BeginVertical(SRWindow.searchBoxDragHighlight);
        GUILayout.Label(new GUIContent(dragText), SRWindow.richTextStyle);
        GUILayout.EndVertical();
      }else{
        GUILayout.BeginVertical(SRWindow.searchBox);
        GUILayout.Label(new GUIContent(dragText), SRWindow.richTextStyle);
        GUILayout.EndVertical();
      }
      EditorGUILayout.EndHorizontal();

      if(e.type == EventType.Layout && draggedObj != null && objID.obj != draggedObj)
      {
        ObjectUtil.ValidateAndAssign(draggedObj, objID, searchProperty, ref initializationContext);
        draggedObj = null;
      }
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(subItem != null)
      {
        UnityEngine.Object originalObj = prop.serializedObject.targetObject;
        // figure out how this object relates to our replace object.
        FieldData fd = initializationContext.fieldData;
        SerializedProperty replaceProp = null;
        if(fd.objectType == originalObj.GetType())
        {
          // easy enough! they match.
          replaceProp = prop.serializedObject.FindProperty(fd.fieldName);
        }else{
          GameObject go = ObjectUtil.GetGameObject(originalObj);
          if(go != null)
          {
            Component c = go.GetComponent(fd.objectType);
            if(c != null)
            {
              SerializedObject so = new SerializedObject(c);
              replaceProp = so.FindProperty(fd.fieldName);
            }
          }
        }
        if(replaceProp != null)
        {
          result.pathInfo = PathInfo.GetPathInfo(replaceProp.serializedObject.targetObject, job.assetData);
          result.strRep = subItem.StringValueFor(replaceProp);
          subItem.ReplaceInternal(job, replaceProp, result);
          replaceProp.serializedObject.ApplyModifiedProperties();
          // This is pretty slow. We may want to find a better solution.
          if(PrefabUtil.isInstanceModification(replaceProp))
          {
            PrefabUtility.RecordPrefabInstancePropertyModifications(replaceProp.serializedObject.targetObject);
          }
        }
      }
    }

    public override void SetParent(DynamicTypeData p)
    {
      base.SetParent(p);
      replaceItemString.SetParent(p);
    }

  }



}
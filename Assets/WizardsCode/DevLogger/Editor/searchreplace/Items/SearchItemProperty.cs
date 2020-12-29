using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * The search item for searching for a specific property on an object.
   */
  [System.Serializable]
  public class SearchItemProperty : SearchItem
  {
    public ObjectID objID;

    PropertyPopupData searchProperty; 
    DynamicTypeField propertyCriteria;

    [System.NonSerialized]
    InitializationContext initializationContext;

    [System.NonSerialized][HideInInspector]
    UnityEngine.Object draggedObj = null;

    public override void Draw(SearchOptions options)
    { 
      GUILayout.BeginHorizontal(SRWindow.searchBox); // 1
      GUILayout.BeginVertical(); 

      drawSubsearch();

      GUILayout.BeginHorizontal();
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


      if(propertyCriteria.hasAdvancedOptions())
      {
        bool newShowMoreOptions = EditorGUILayout.Toggle( showMoreOptions, SRWindow.optionsToggle, GUILayout.Width(15));
        if(newShowMoreOptions != showMoreOptions)
        {
          showMoreOptions = newShowMoreOptions;
          propertyCriteria.showMoreOptions = showMoreOptions;
          SRWindow.Instance.PersistCurrentSearch();
        }
      }
      GUILayout.EndHorizontal();

      if(objID.obj != null)
      {
        string typeInfo = "";
        if(searchProperty.HasOptions())
        {
          typeInfo = "Searching the "+searchProperty.fieldData.objectType.Name + " property <b>"+searchProperty.fieldData.fieldName + propertyCriteria.StringValueWithConditional()+"</b>";
        }

        string warningInfo = "";
        PrefabTypes pType = PrefabUtil.GetPrefabType(objID.obj);
        if(pType == PrefabTypes.NotAPrefab)
        {
          if(objID.obj is GameObject || objID.obj is Component)
          {
            warningInfo = "Referencing a scene component, your search will not be saved on scene change.";
            
          }else if(SRWindow.Instance.isSearchingInScene() && !SRWindow.Instance.isSearchingDependencies())
          {
            warningInfo = "Searching in a scene but this is not a scene object. No results will be found.";
          }
        }

        string ssw = subsearchWarning();
        if(ssw != string.Empty)
        {
          warningInfo += " "+ssw;
        }

         if(warningInfo.Length > 0)
        {
          EditorGUILayout.HelpBox(warningInfo, MessageType.Warning);
        }
        if(typeInfo.Length > 0)
        {
          EditorGUILayout.LabelField(typeInfo, SRWindow.richTextStyle);
        }
        searchProperty.Draw();
        if(searchProperty.HasOptions())
        { 
          initializationContext.updateFieldData(searchProperty.fieldData);
          propertyCriteria.SetType(initializationContext);
          initializationContext.forceUpdate = false;

          SearchOptions typeFieldOptions = options.Copy();
          if(subsearch != null)
          {
            typeFieldOptions.searchType = SearchType.Search;
          }

          propertyCriteria.Draw(typeFieldOptions);
        }
      }
      
      if(subsearch != null)
      {
        SearchItem child = (SearchItem)subsearch;
        child.Draw(options);
      }


      drawAddRemoveButtons();
      GUILayout.EndVertical(); 

      GUILayout.EndHorizontal(); // 1
      
      if(e.type == EventType.DragExited && draggedObj != null && objID.obj != draggedObj)
      { 
        ObjectUtil.ValidateAndAssign(draggedObj, objID, searchProperty, ref initializationContext);
        draggedObj = null;
        propertyCriteria.SetType(initializationContext);
      }
    }

    public void InitWithObject(UnityEngine.Object obj)
    {
      if(objID == null)
      {
        objID = new ObjectID();
      }
      OnDeserialization();
      if(ObjectUtil.ValidateAndAssign(obj, objID, searchProperty, ref initializationContext))
      {
        // The object may be valid, and we should assign it, but it may not have
        // any properties. If that is the case, we shouldn't call setType().
        if(initializationContext.fieldData != null)
        {
          // Set this value right now, otherwise our layout and repaint events will
          // have mismatching controls
          initializationContext.forceUpdate = true;
          propertyCriteria.SetType(initializationContext);
          initializationContext.forceUpdate = false;
        }
      }else{ 
        Debug.LogError("[Search + Replace] Could not initialize object.");
      }
    }


    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    {
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

      if(propertyCriteria == null)
      {
        propertyCriteria = new DynamicTypeField();
      }
      propertyCriteria.searchItem = this;
      propertyCriteria.OnDeserialization(); // sigh...this is getting old.
      propertyCriteria.showMoreOptions = showMoreOptions;

      searchProperty.SetType(objID.obj);
      initializationContext = new InitializationContext(searchProperty.fieldData, objID.obj);
      if(searchProperty.HasOptions())
      {
        propertyCriteria.SetType(initializationContext);
      }
      if(searchDepth == 0)
      {
        root = this;
      }
      OnDeserializeSubSearch();
    }


    public override void SearchProperty(SearchJob job, SerializedProperty prop)
    {
      // Debug.Log("[SearchItemProperty] Searching:"+prop.serializedObject.targetObject);
      //if the type of object we are searching matches the target object's type
      // then search the property. For example, if we're searching for MyBehaviour
      // require that this is searchable by that type.
      if(searchProperty.fieldData.objectType.IsAssignableFrom(prop.serializedObject.targetObject.GetType()))
      {
        job.assetData.searchExecuted = true;
        List<SerializedProperty> foundProps = searchProperty.fieldData.findProperties(prop);
        foreach(SerializedProperty foundProp in foundProps)
        {
          propertyCriteria.SearchProperty(job, this, foundProp);
        }
      }else{
        // Debug.Log("[SearchItemProperty] not assignable:"+searchProperty.fieldData.objectType + " to  "+ prop.serializedObject.targetObject);
      }
    }

    public override bool IsValid()
    {
      return objID.obj != null && searchProperty.HasOptions() && propertyCriteria.IsValid();
    }

    public override bool IsReplaceValid()
    {
      return IsValid() && propertyCriteria.IsReplaceValid();
    }

    public override string GetDescription()
    {
      return searchProperty.fieldData.objectType.Name + "." + searchProperty.fieldData.fieldName + propertyCriteria.StringValueWithConditional();
    }

    string subsearchWarning()
    {
      if(searchDepth > 0)
      {
        // This is the last search!
        SearchItem p = parent;
        SearchItemProperty child = this;
        if(child.subScope == SubsearchScope.SameObject)
        {
          //Check parent info.
          if(p is SearchItemProperty)
          {
            SearchItemProperty pItem = (SearchItemProperty)p;
            if(pItem.objID.obj != null)
            {
              Type pt = pItem.objID.obj.GetType();
              if(pItem.objID.obj is MonoScript)
              {
                pt = ((MonoScript)pItem.objID.obj).GetClass();
              }

              // We know this is a searchable type :)
              if(objID.obj != null)
              {
                Type ct = child.objID.obj.GetType();
                if(child.objID.obj is MonoScript)
                {
                  ct = ((MonoScript)child.objID.obj).GetClass();
                }
                if(pt.IsAssignableFrom(ct))
                {
                  //all good
                }else{
                  // Debug.Log("[SearchItemProperty] warn!!"+pt+" "+ ct);
                  return ct.Name +" does not inherit from "+ pt.Name+" but the scope is set to 'Same Object'. This search will probably yield no results.";
                }
              }
            }
          }
        }
      }
      return string.Empty;
    }

    AssetScope searchScope;

    public override void OnSearchBegin()
    {
      UnityEngine.Object o = objID.obj;
      if(o is Component || o is GameObject)
      {
        searchScope = AssetScope.Prefabs | AssetScope.Scenes;
      }else if(o is MonoScript)
      {
        //This could be better but for now we just search anything that can have a monoscript.
        searchScope = AssetScope.Prefabs | AssetScope.Scenes | AssetScope.ScriptableObjects;
      }else if(o is ScriptableObject)
      {
        searchScope = AssetScope.ScriptableObjects;
      }else if(o is Material)
      {
        searchScope = AssetScope.Materials;
      }else if(o is AnimationClip)
      {
        searchScope = AssetScope.Animations;
      }else if(o is UnityEditor.Animations.AnimatorController)
      {
        searchScope = AssetScope.Animators;
      }else if(o is Texture)
      {
        searchScope = AssetScope.Textures;
      }else if(o is AudioClip)
      {
        searchScope = AssetScope.AudioClips;
      }else{
        // Debug.Log("[SearchItemProperty] Unknown! Setting to all");
        searchScope = (AssetScope)~0; //All!
      }
    }

    public override bool caresAboutAsset(SearchJob job)
    {
      if(job.scope.projectScope != ProjectScope.EntireProject && job.scope.searchDependencies)
      {
        return true;
      }
      return (searchScope & job.assetData.assetScope) != AssetScope.None;
    }

  }
}
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace sr
{
  /**
   * Data model of an object that is searched. Has it already been searched?
   * etc.
   */
  public class IgnoredSearchAssetData : SearchAssetData
  {

    public IgnoredSearchAssetData(string path) : base(path)
    {
    }

  }
}
namespace sr
{
  [System.Flags]
  public enum AssetScope
  {
    None = 0x00,
    Prefabs = 0x01,
    Scenes = 0x02,
    ScriptableObjects = 0x04,
    Materials = 0x08,
    Animations = 0x10,
    Animators = 0x20,
    Textures = 0x40,
    AudioClips = 0x80

  }
}
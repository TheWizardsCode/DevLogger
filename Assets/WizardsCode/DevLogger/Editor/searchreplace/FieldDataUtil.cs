using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEditor.Animations;

namespace sr
{
  /**
   * A utility class including code generated from running custom reflection 
   * scripts against internal unity classes in order to provide better search
   * and replace functionality.
   */
  public class FieldDataUtil
  {

    static HashSet<Type> whiteListedTypesHash;
    static HashSet<Type> blackListedTypesHash;

    public static IEnumerable<FieldInfo> GetAllFields(Type t)
    {
      if (t == null)
          return Enumerable.Empty<FieldInfo>();
      BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ;
      return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
    }

    public static bool isWhitelisted(Type t)
    {
      if(whiteListedTypesHash == null)
      {
        whiteListedTypesHash = new HashSet<Type>();
        whiteListedTypesHash.Add(typeof(Vector2));
        whiteListedTypesHash.Add(typeof(Vector3));
        whiteListedTypesHash.Add(typeof(Vector4));
        whiteListedTypesHash.Add(typeof(Rect));
        whiteListedTypesHash.Add(typeof(Color));
        whiteListedTypesHash.Add(typeof(Color32));
        whiteListedTypesHash.Add(typeof(Quaternion));
      }
      return whiteListedTypesHash.Contains(t);
    }

    public static bool isBlacklisted(Type t)
    {
      if(blackListedTypesHash == null)
      {
        blackListedTypesHash = new HashSet<Type>();
        blackListedTypesHash.Add(typeof(Action));
      }
      return blackListedTypesHash.Contains(t);
    }

    public static List<FieldData> GetCustomFieldsForType(Type t)
    {
      List<FieldData> retVal = new List<FieldData>();
      if(typeof(UnityEngine.Material) == t)
      {
        retVal.Add(new FieldData(typeof(Shader), t,  "m_Shader", "Shader"));
        retVal.Add(new FieldData(typeof(string), t,  "m_ShaderKeywords", "Shader Keywords"));
        retVal.Add(new FieldData(typeof(uint), t,  "m_LightmapFlags", "Lightmap Flags"));
        retVal.Add(new FieldData(typeof(int), t,  "m_CustomRenderQueue", "Custom Render Queue"));
        retVal.Add(new FieldData(typeof(PropSheet<Texture>), t, "m_SavedProperties.m_TexEnvs.Array", "All Textures"));
        retVal.Add(new FieldData(typeof(PropSheet<float>), t, "m_SavedProperties.m_Floats.Array", "All Floats"));
        retVal.Add(new FieldData(typeof(PropSheet<Color>), t, "m_SavedProperties.m_Colors.Array", "All Colors"));
        retVal.Add(new FieldData(typeof(PropSheet<Texture>), t, "m_SavedProperties.m_TexEnvs.Array", "Specific Texture"));
        retVal.Add(new FieldData(typeof(PropSheet<float>), t, "m_SavedProperties.m_Floats.Array", "Specific Float"));
        retVal.Add(new FieldData(typeof(PropSheet<Color>), t, "m_SavedProperties.m_Colors.Array", "Specific Color"));
/*        retVal.Add(new FieldData(typeof(Color), t,  "m_SavedProperties.m_Colors.Array.data[1].second", "_Color"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[0].second.m_Texture", "_MainTex (Albedo)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[8].second.m_Texture", "_MetallicGlossMap (Metallic)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[1].second.m_Texture", "_BumpMap (Normal Map)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[3].second.m_Texture", "_ParallaxMap (Height Map)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[4].second.m_Texture", "_OcclusionMap (Occlusion)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[5].second.m_Texture", "_EmissionMap (Emission)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[6].second.m_Texture", "_DetailMask (Detail Mask)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[2].second.m_Texture", "_DetailNormalMap (Secondary Normal Map)"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_SavedProperties.m_TexEnvs.Array.data[7].second.m_Texture", "_DetailAlbedoMap (Detail Albedo x2)"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[3].second", "_Parallax (Height Map %)"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[6].second", "_BumpScale (Normal Map %)"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[7].second", "_OcclusionStrength (Occlusion %)"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[8].second", "_DetailNormalMapScale (Secondary Normal Map %)"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[0].second", "_SrcBlend"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[1].second", "_DstBlend"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[2].second", "_Cutoff"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[4].second", "_ZWrite"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[5].second", "_Glossiness"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[9].second", "_UVSec"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[10].second", "_Mode (Rendering Mode)"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SavedProperties.m_Floats.Array.data[11].second", "_Metallic"));
        retVal.Add(new FieldData(typeof(Color), t,  "m_SavedProperties.m_Colors.Array.data[0].second", "_EmissionColor"));*/
        return retVal;
      }

      if(typeof(UnityEngine.BoxCollider) == t)
      {
        retVal.Add(new FieldData(typeof(PhysicMaterial), t,  "m_Material", "Material"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_IsTrigger", "Is Trigger"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_Enabled", "Enabled"));
        retVal.Add(new FieldData(typeof(Vector3), t,  "m_Size", "Size"));
        retVal.Add(new FieldData(typeof(Vector3), t,  "m_Center", "Center"));
        return retVal;
      }
      if(typeof(UnityEngine.MeshRenderer) == t)
      {
        retVal.Add(new FieldData(typeof(bool), t,  "m_Enabled", "Enabled"));
        retVal.Add(new FieldData(typeof(ShadowCastingMode), t,  "m_CastShadows", "Cast Shadows"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_ReceiveShadows", "Receive Shadows"));
        retVal.Add(new FieldData(typeof(PropSheet<Material>), t, "m_Materials", "Materials"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_UseLightProbes", "Use Light Probes"));
        retVal.Add(new FieldData(typeof(int), t,  "m_ReflectionProbeUsage", "Reflection Probe Usage"));
        retVal.Add(new FieldData(typeof(Transform), t,  "m_ProbeAnchor", "Probe Anchor"));
        retVal.Add(new FieldData(typeof(LightmapParameters), t,  "m_LightmapParameters", "Lightmap Parameters"));
        return retVal;
      }
      if(typeof(UnityEngine.MeshFilter) == t)
      {
        retVal.Add(new FieldData(typeof(Mesh), t,  "m_Mesh", "Mesh"));
        return retVal;
      }
      if(typeof(UnityEngine.Transform) == t)
      {
        retVal.Add(new FieldData(typeof(Vector3), t,  "m_LocalPosition", "Local Position"));
        retVal.Add(new FieldData(typeof(Quaternion), t,  "m_LocalRotation", "Local Rotation"));
        retVal.Add(new FieldData(typeof(Vector3), t,  "m_LocalScale", "Local Scale"));
        return retVal;
      }
      if(typeof(UnityEngine.AudioSource) == t)
      {
        retVal.Add(new FieldData(typeof(bool), t,  "m_Enabled", "Enabled"));
        retVal.Add(new FieldData(typeof(UnityEngine.Audio.AudioMixerGroup), t, "OutputAudioMixerGroup", "Output Audio Mixer Group"));
        retVal.Add(new FieldData(typeof(AudioClip), t,  "m_audioClip", "Audio Clip"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_PlayOnAwake", "Play On Awake"));
        retVal.Add(new FieldData(typeof(float), t,  "m_Volume", "Volume"));
        retVal.Add(new FieldData(typeof(float), t,  "m_Pitch", "Pitch"));
        retVal.Add(new FieldData(typeof(bool), t,  "Loop", "Loop"));
        retVal.Add(new FieldData(typeof(bool), t,  "Mute", "Mute"));
        retVal.Add(new FieldData(typeof(bool), t,  "Spatialize", "Spatialize"));
        retVal.Add(new FieldData(typeof(int), t,  "Priority", "Priority"));
        retVal.Add(new FieldData(typeof(float), t,  "DopplerLevel", "Doppler Level"));
        retVal.Add(new FieldData(typeof(float), t,  "MinDistance", "Min Distance"));
        retVal.Add(new FieldData(typeof(float), t,  "MaxDistance", "Max Distance"));
        retVal.Add(new FieldData(typeof(float), t,  "Pan2D", "Pan 2D"));
        retVal.Add(new FieldData(typeof(AudioRolloffMode), t,  "rolloffMode", "Rolloff Mode"));
        retVal.Add(new FieldData(typeof(bool), t,  "BypassEffects", "Bypass Effects"));
        retVal.Add(new FieldData(typeof(bool), t,  "BypassListenerEffects", "Bypass Listener Effects"));
        retVal.Add(new FieldData(typeof(bool), t,  "BypassReverbZones", "Bypass Reverb Zones"));
        return retVal;
      }
      if(typeof(UnityEngine.TextMesh) == t)
      {
        retVal.Add(new FieldData(typeof(string), t,  "m_Text", "Text"));
        retVal.Add(new FieldData(typeof(float), t,  "m_OffsetZ", "Offset Z"));
        retVal.Add(new FieldData(typeof(float), t,  "m_CharacterSize", "Character Size"));
        retVal.Add(new FieldData(typeof(float), t,  "m_LineSpacing", "Line Spacing"));
        retVal.Add(new FieldData(typeof(TextAnchor), t,  "m_Anchor", "Anchor"));
        retVal.Add(new FieldData(typeof(TextAlignment), t,  "m_Alignment", "Alignment"));
        retVal.Add(new FieldData(typeof(float), t,  "m_TabSize", "Tab Size"));
        retVal.Add(new FieldData(typeof(int), t,  "m_FontSize", "Font Size"));
        retVal.Add(new FieldData(typeof(FontStyle), t,  "m_FontStyle", "Font Style"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_RichText", "Rich Text"));
        retVal.Add(new FieldData(typeof(Font), t,  "m_Font", "Font"));
        retVal.Add(new FieldData(typeof(Color), t,  "m_Color", "Color"));
        return retVal;
      }

      if(typeof(UnityEngine.Camera) == t)
      {
        retVal.Add(new FieldData(typeof(bool), t,  "m_Enabled", "Enabled"));
        retVal.Add(new FieldData(typeof(CameraClearFlags), t,  "m_ClearFlags", "Clear Flags"));
        retVal.Add(new FieldData(typeof(Color), t,  "m_BackGroundColor", "Back Ground Color"));
        retVal.Add(new FieldData(typeof(Rect), t,  "m_NormalizedViewPortRect", "Normalized View Port Rect"));
        retVal.Add(new FieldData(typeof(float), t,  "near clip plane", "Near clip plane"));
        retVal.Add(new FieldData(typeof(float), t,  "far clip plane", "Far clip plane"));
        retVal.Add(new FieldData(typeof(float), t,  "field of view", "Field of view"));
        retVal.Add(new FieldData(typeof(bool), t,  "orthographic", "Orthographic"));
        retVal.Add(new FieldData(typeof(float), t,  "orthographic size", "Orthographic size"));
        retVal.Add(new FieldData(typeof(float), t,  "m_Depth", "Depth"));
        retVal.Add(new FieldData(typeof(LayerMask), t,  "m_CullingMask", "Culling Mask"));
        retVal.Add(new FieldData(typeof(RenderingPath), t,  "m_RenderingPath", "Rendering Path"));
        retVal.Add(new FieldData(typeof(RenderTexture), t,  "m_TargetTexture", "Target Texture"));
        retVal.Add(new FieldData(typeof(int), t,  "m_TargetDisplay", "Target Display"));
        retVal.Add(new FieldData(typeof(int), t,  "m_TargetEye", "Target Eye"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_HDR", "HDR"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_OcclusionCulling", "Occlusion Culling"));
        retVal.Add(new FieldData(typeof(float), t,  "m_StereoConvergence", "Stereo Convergence"));
        retVal.Add(new FieldData(typeof(float), t,  "m_StereoSeparation", "Stereo Separation"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_StereoMirrorMode", "Stereo Mirror Mode"));
        return retVal;
      }

      if(typeof(UnityEngine.Light) == t)
      {
        retVal.Add(new FieldData(typeof(bool), t,  "m_Enabled", "Enabled"));
        retVal.Add(new FieldData(typeof(LightType), t,  "m_Type", "Type"));
        retVal.Add(new FieldData(typeof(Color), t,  "m_Color", "Color"));
        retVal.Add(new FieldData(typeof(float), t,  "m_Intensity", "Intensity"));
        retVal.Add(new FieldData(typeof(float), t,  "m_Range", "Range"));
        retVal.Add(new FieldData(typeof(float), t,  "m_SpotAngle", "Spot Angle"));
        retVal.Add(new FieldData(typeof(float), t,  "m_CookieSize", "Cookie Size"));
        //TODO
        //retVal.Add(new FieldData(typeof(ShadowSettings), t,  "m_Shadows", "Shadows"));
        retVal.Add(new FieldData(typeof(Texture), t,  "m_Cookie", "Cookie"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_DrawHalo", "Draw Halo"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_ActuallyLightmapped", "Actually Lightmapped"));
        retVal.Add(new FieldData(typeof(Flare), t,  "m_Flare", "Flare"));
        retVal.Add(new FieldData(typeof(LightRenderMode), t,  "m_RenderMode", "Render Mode"));
        retVal.Add(new FieldData(typeof(LayerMask), t,  "m_CullingMask", "Culling Mask"));
        retVal.Add(new FieldData(typeof(int), t,  "m_Lightmapping", "Lightmapping"));
        retVal.Add(new FieldData(typeof(float), t,  "m_BounceIntensity", "Bounce Intensity"));
        retVal.Add(new FieldData(typeof(float), t,  "m_ShadowRadius", "Shadow Radius"));
        retVal.Add(new FieldData(typeof(float), t,  "m_ShadowAngle", "Shadow Angle"));
        retVal.Add(new FieldData(typeof(Vector2), t,  "m_AreaSize", "Area Size"));
        return retVal;
      }

      if(typeof(UnityEngine.ParticleSystem) == t)
      {
        retVal.Add(new FieldData(typeof(float), t,  "lengthInSec", "Length In Sec"));
        retVal.Add(new FieldData(typeof(float), t,  "speed", "Speed"));
        retVal.Add(new FieldData(typeof(uint), t,  "randomSeed", "Random Seed"));
        retVal.Add(new FieldData(typeof(bool), t,  "looping", "Looping"));
        retVal.Add(new FieldData(typeof(bool), t,  "prewarm", "Prewarm"));
        retVal.Add(new FieldData(typeof(bool), t,  "playOnAwake", "Play On Awake"));
        retVal.Add(new FieldData(typeof(bool), t,  "moveWithTransform", "Move With Transform"));
        retVal.Add(new FieldData(typeof(int), t,  "scalingMode", "Scaling Mode"));
        retVal.Add(new FieldData(typeof(bool), t,  "InitialModule.enabled", "InitialModule.Enabled"));
        retVal.Add(new FieldData(typeof(float), t,  "InitialModule.randomizeRotationDirection", "InitialModule.Randomize Rotation Direction"));
        retVal.Add(new FieldData(typeof(float), t,  "InitialModule.gravityModifier", "InitialModule.Gravity Modifier"));
        retVal.Add(new FieldData(typeof(int), t,  "InitialModule.maxNumParticles", "InitialModule.Max Num Particles"));
        retVal.Add(new FieldData(typeof(bool), t,  "InitialModule.rotation3D", "InitialModule.Rotation 3D"));
        retVal.Add(new FieldData(typeof(bool), t,  "ShapeModule.enabled", "ShapeModule.Enabled"));
        retVal.Add(new FieldData(typeof(int), t,  "ShapeModule.type", "ShapeModule.Type"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.radius", "ShapeModule.Radius"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.angle", "ShapeModule.Angle"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.length", "ShapeModule.Length"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.boxX", "ShapeModule.Box X"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.boxY", "ShapeModule.Box Y"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.boxZ", "ShapeModule.Box Z"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.arc", "ShapeModule.Arc"));
        retVal.Add(new FieldData(typeof(int), t,  "ShapeModule.placementMode", "ShapeModule.Placement Mode"));
        retVal.Add(new FieldData(typeof(Mesh), t,  "ShapeModule.m_Mesh", "ShapeModule.Mesh"));
        retVal.Add(new FieldData(typeof(MeshRenderer), t,  "ShapeModule.m_MeshRenderer", "ShapeModule.Mesh Renderer"));
        retVal.Add(new FieldData(typeof(SkinnedMeshRenderer), t,  "ShapeModule.m_SkinnedMeshRenderer", "ShapeModule.Skinned Mesh Renderer"));
        retVal.Add(new FieldData(typeof(int), t,  "ShapeModule.m_MeshMaterialIndex", "ShapeModule.Mesh Material Index"));
        retVal.Add(new FieldData(typeof(float), t,  "ShapeModule.m_MeshNormalOffset", "ShapeModule.Mesh Normal Offset"));
        retVal.Add(new FieldData(typeof(bool), t,  "ShapeModule.m_UseMeshMaterialIndex", "ShapeModule.Use Mesh Material Index"));
        retVal.Add(new FieldData(typeof(bool), t,  "ShapeModule.m_UseMeshColors", "ShapeModule.Use Mesh Colors"));
        retVal.Add(new FieldData(typeof(bool), t,  "ShapeModule.randomDirection", "ShapeModule.Random Direction"));
        retVal.Add(new FieldData(typeof(bool), t,  "EmissionModule.enabled", "EmissionModule.Enabled"));
        retVal.Add(new FieldData(typeof(int), t,  "EmissionModule.m_Type", "EmissionModule.Type"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cnt0", "EmissionModule.Cnt 0"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cnt1", "EmissionModule.Cnt 1"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cnt2", "EmissionModule.Cnt 2"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cnt3", "EmissionModule.Cnt 3"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cntmax0", "EmissionModule.Cntmax 0"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cntmax1", "EmissionModule.Cntmax 1"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cntmax2", "EmissionModule.Cntmax 2"));
        retVal.Add(new FieldData(typeof(ushort), t,  "EmissionModule.cntmax3", "EmissionModule.Cntmax 3"));
        retVal.Add(new FieldData(typeof(float), t,  "EmissionModule.time0", "EmissionModule.Time 0"));
        retVal.Add(new FieldData(typeof(float), t,  "EmissionModule.time1", "EmissionModule.Time 1"));
        retVal.Add(new FieldData(typeof(float), t,  "EmissionModule.time2", "EmissionModule.Time 2"));
        retVal.Add(new FieldData(typeof(float), t,  "EmissionModule.time3", "EmissionModule.Time 3"));
        retVal.Add(new FieldData(typeof(byte), t,  "EmissionModule.m_BurstCount", "EmissionModule.Burst Count"));
        retVal.Add(new FieldData(typeof(bool), t,  "SizeModule.enabled", "SizeModule.Enabled"));
        retVal.Add(new FieldData(typeof(bool), t,  "RotationModule.enabled", "RotationModule.Enabled"));
        retVal.Add(new FieldData(typeof(bool), t,  "RotationModule.separateAxes", "RotationModule.Separate Axes"));
        retVal.Add(new FieldData(typeof(bool), t,  "ColorModule.enabled", "ColorModule.Enabled"));
        retVal.Add(new FieldData(typeof(bool), t,  "UVModule.enabled", "UVModule.Enabled"));
        retVal.Add(new FieldData(typeof(int), t,  "UVModule.tilesX", "UVModule.Tiles X"));
        retVal.Add(new FieldData(typeof(int), t,  "UVModule.tilesY", "UVModule.Tiles Y"));
        retVal.Add(new FieldData(typeof(int), t,  "UVModule.animationType", "UVModule.Animation Type"));
        retVal.Add(new FieldData(typeof(int), t,  "UVModule.rowIndex", "UVModule.Row Index"));
        retVal.Add(new FieldData(typeof(float), t,  "UVModule.cycles", "UVModule.Cycles"));
        retVal.Add(new FieldData(typeof(bool), t,  "UVModule.randomRow", "UVModule.Random Row"));
        retVal.Add(new FieldData(typeof(bool), t,  "VelocityModule.enabled", "VelocityModule.Enabled"));
        retVal.Add(new FieldData(typeof(bool), t,  "VelocityModule.inWorldSpace", "VelocityModule.In World Space"));
        retVal.Add(new FieldData(typeof(bool), t,  "InheritVelocityModule.enabled", "InheritVelocityModule.Enabled"));
        retVal.Add(new FieldData(typeof(int), t,  "InheritVelocityModule.m_Mode", "InheritVelocityModule.Mode"));
        retVal.Add(new FieldData(typeof(bool), t,  "ForceModule.enabled", "ForceModule.Enabled"));
        retVal.Add(new FieldData(typeof(bool), t,  "ForceModule.inWorldSpace", "ForceModule.In World Space"));
        retVal.Add(new FieldData(typeof(bool), t,  "ForceModule.randomizePerFrame", "ForceModule.Randomize Per Frame"));
        retVal.Add(new FieldData(typeof(bool), t,  "ExternalForcesModule.enabled", "ExternalForcesModule.Enabled"));
        retVal.Add(new FieldData(typeof(float), t,  "ExternalForcesModule.multiplier", "ExternalForcesModule.Multiplier"));
        retVal.Add(new FieldData(typeof(bool), t,  "ClampVelocityModule.enabled", "ClampVelocityModule.Enabled"));
        retVal.Add(new FieldData(typeof(bool), t,  "ClampVelocityModule.separateAxis", "ClampVelocityModule.Separate Axis"));
        retVal.Add(new FieldData(typeof(bool), t,  "ClampVelocityModule.inWorldSpace", "ClampVelocityModule.In World Space"));
        retVal.Add(new FieldData(typeof(float), t,  "ClampVelocityModule.dampen", "ClampVelocityModule.Dampen"));
        retVal.Add(new FieldData(typeof(bool), t,  "SizeBySpeedModule.enabled", "SizeBySpeedModule.Enabled"));
        retVal.Add(new FieldData(typeof(Vector2), t,  "SizeBySpeedModule.range", "SizeBySpeedModule.Range"));
        retVal.Add(new FieldData(typeof(bool), t,  "RotationBySpeedModule.enabled", "RotationBySpeedModule.Enabled"));
        retVal.Add(new FieldData(typeof(bool), t,  "RotationBySpeedModule.separateAxes", "RotationBySpeedModule.Separate Axes"));
        retVal.Add(new FieldData(typeof(Vector2), t,  "RotationBySpeedModule.range", "RotationBySpeedModule.Range"));
        retVal.Add(new FieldData(typeof(bool), t,  "ColorBySpeedModule.enabled", "ColorBySpeedModule.Enabled"));
        retVal.Add(new FieldData(typeof(Vector2), t,  "ColorBySpeedModule.range", "ColorBySpeedModule.Range"));
        retVal.Add(new FieldData(typeof(bool), t,  "CollisionModule.enabled", "CollisionModule.Enabled"));
        retVal.Add(new FieldData(typeof(int), t,  "CollisionModule.type", "CollisionModule.Type"));
        retVal.Add(new FieldData(typeof(int), t,  "CollisionModule.collisionMode", "CollisionModule.Collision Mode"));
        retVal.Add(new FieldData(typeof(Transform), t,  "CollisionModule.plane0", "CollisionModule.Plane 0"));
        retVal.Add(new FieldData(typeof(Transform), t,  "CollisionModule.plane1", "CollisionModule.Plane 1"));
        retVal.Add(new FieldData(typeof(Transform), t,  "CollisionModule.plane2", "CollisionModule.Plane 2"));
        retVal.Add(new FieldData(typeof(Transform), t,  "CollisionModule.plane3", "CollisionModule.Plane 3"));
        retVal.Add(new FieldData(typeof(Transform), t,  "CollisionModule.plane4", "CollisionModule.Plane 4"));
        retVal.Add(new FieldData(typeof(Transform), t,  "CollisionModule.plane5", "CollisionModule.Plane 5"));
        retVal.Add(new FieldData(typeof(float), t,  "CollisionModule.minKillSpeed", "CollisionModule.Min Kill Speed"));
        retVal.Add(new FieldData(typeof(float), t,  "CollisionModule.radiusScale", "CollisionModule.Radius Scale"));
        retVal.Add(new FieldData(typeof(LayerMask), t,  "CollisionModule.collidesWith", "CollisionModule.Collides With"));
        retVal.Add(new FieldData(typeof(int), t,  "CollisionModule.maxCollisionShapes", "CollisionModule.Max Collision Shapes"));
        retVal.Add(new FieldData(typeof(int), t,  "CollisionModule.quality", "CollisionModule.Quality"));
        retVal.Add(new FieldData(typeof(float), t,  "CollisionModule.voxelSize", "CollisionModule.Voxel Size"));
        retVal.Add(new FieldData(typeof(bool), t,  "CollisionModule.collisionMessages", "CollisionModule.Collision Messages"));
        retVal.Add(new FieldData(typeof(bool), t,  "CollisionModule.collidesWithDynamic", "CollisionModule.Collides With Dynamic"));
        retVal.Add(new FieldData(typeof(bool), t,  "CollisionModule.interiorCollisions", "CollisionModule.Interior Collisions"));
        retVal.Add(new FieldData(typeof(bool), t,  "SubModule.enabled", "SubModule.Enabled"));
        retVal.Add(new FieldData(typeof(ParticleSystem), t,  "SubModule.subEmitterBirth", "SubModule.Sub Emitter Birth"));
        retVal.Add(new FieldData(typeof(ParticleSystem), t,  "SubModule.subEmitterBirth1", "SubModule.Sub Emitter Birth 1"));
        retVal.Add(new FieldData(typeof(ParticleSystem), t,  "SubModule.subEmitterCollision", "SubModule.Sub Emitter Collision"));
        retVal.Add(new FieldData(typeof(ParticleSystem), t,  "SubModule.subEmitterCollision1", "SubModule.Sub Emitter Collision 1"));
        retVal.Add(new FieldData(typeof(ParticleSystem), t,  "SubModule.subEmitterDeath", "SubModule.Sub Emitter Death"));
        retVal.Add(new FieldData(typeof(ParticleSystem), t,  "SubModule.subEmitterDeath1", "SubModule.Sub Emitter Death 1"));
        return retVal;
      }

      if(typeof(UnityEngine.AnimationClip) == t)
      {
        retVal.Add(new RegexFieldData(typeof(string), t, "^.*\\.path$", "Paths", SerializedPropertyType.String));
        retVal.Add(new RegexFieldData(typeof(string), t, "^.*\\.attribute$", "Properties", SerializedPropertyType.String));
        return retVal;
      }


      if(typeof(UnityEngine.Animator) == t)
      {
        retVal.Add(new FieldData(typeof(RuntimeAnimatorController), t,  "m_Controller", "Animator Controller"));
        retVal.Add(new FieldData(typeof(Avatar), t,  "m_Avatar", "Avatar"));
        retVal.Add(new FieldData(typeof(bool), t, "m_Enabled", "Enabled"));
        retVal.Add(new FieldData(typeof(Avatar), t, "m_Avatar", "Avatar"));
        retVal.Add(new FieldData(typeof(RuntimeAnimatorController), t, "m_Controller", "Controller"));
        retVal.Add(new FieldData(typeof(AnimatorCullingMode), t, "m_CullingMode", "Culling Mode"));
        retVal.Add(new FieldData(typeof(AnimatorUpdateMode), t, "m_UpdateMode", "Update Mode"));
        retVal.Add(new FieldData(typeof(bool), t, "m_ApplyRootMotion", "Apply Root Motion"));
        retVal.Add(new FieldData(typeof(bool), t, "m_LinearVelocityBlending", "Linear Velocity Blending"));
        retVal.Add(new FieldData(typeof(string), t, "m_WarningMessage", "Warning Message"));
        retVal.Add(new FieldData(typeof(bool), t, "m_HasTransformHierarchy", "Has Transform Hierarchy"));
        retVal.Add(new FieldData(typeof(bool), t, "m_AllowConstantClipSamplingOptimization", "Allow Constant Clip Sampling Optimization"));
        return retVal;
      }

      if(typeof(UnityEngine.UI.Text) == t)
      {
        retVal.Add(new FieldData(typeof(string), t, "m_Text", "Text"));
        retVal.Add(new FieldData(typeof(int), t, "m_FontData.m_FontSize", "Font Size"));
        retVal.Add(new FieldData(typeof(Font), t, "m_FontData.m_Font", "Font"));
        retVal.Add(new FieldData(typeof(FontStyle), t, "m_FontData.m_FontStyle", "Font Style"));
        retVal.Add(new FieldData(typeof(TextAnchor), t  , "m_FontData.m_Alignment", "Font Alignment"));
        retVal.Add(new FieldData(typeof(HorizontalWrapMode), t  , "m_FontData.m_HorizontalOverflow", "Horizontal Overflow"));
        retVal.Add(new FieldData(typeof(VerticalWrapMode), t  , "m_FontData.m_VerticalOverflow", "Vertical Overflow"));
        retVal.Add(new FieldData(typeof(float), t  , "m_FontData.m_LineSpacing", "Line Spacing"));
        retVal.Add(new FieldData(typeof(bool), t  , "m_FontData.m_BestFit", "Best Fit"));
        retVal.Add(new FieldData(typeof(int), t  , "m_FontData.m_MinSize", "Min Size"));
        retVal.Add(new FieldData(typeof(int), t  , "m_FontData.m_MaxSize", "Max Size"));
        return retVal;
      }

      if(typeof(UnityEngine.GameObject) == t)
      {
        retVal.Add(new FieldData(typeof(string), t,  "m_Name", "Name"));
        retVal.Add(new FieldData(typeof(uint), t,  "m_Layer", "Layer"));
        retVal.Add(new FieldData(typeof(string), t,  "m_TagString", "Tag String"));
        retVal.Add(new FieldData(typeof(Texture2D), t,  "m_Icon", "Icon"));
        retVal.Add(new FieldData(typeof(uint), t,  "m_NavMeshLayer", "Nav Mesh Layer"));
        retVal.Add(new FieldData(typeof(uint), t,  "m_StaticEditorFlags", "Static Editor Flags"));
        retVal.Add(new FieldData(typeof(bool), t,  "m_IsActive", "Is Active"));
        return retVal;
      }

      if(typeof(UnityEngine.Texture2D) == t)
      {
        retVal.Add(new FieldData(typeof(string), t, "m_Name", "Name"));
        retVal.Add(new FieldData(typeof(int), t, "m_Width", "Width"));
        retVal.Add(new FieldData(typeof(int), t, "m_Height", "Height"));
        retVal.Add(new FieldData(typeof(int), t, "m_CompleteImageSize", "Image Size"));
        retVal.Add(new FieldData(typeof(TextureFormat), t, "m_TextureFormat", "Texture Format"));
        retVal.Add(new FieldData(typeof(int), t, "m_MipCount", "Mip Count"));
        retVal.Add(new FieldData(typeof(bool), t, "m_IsReadable", "Read-Write Enabled (Replacable)"));
        // retVal.Add(new FieldData(typeof(bool), t, "m_ReadAllowed", "Read Allowed"));
        // retVal.Add(new FieldData(typeof(bool), t, "m_AlphaIsTransparency", "Alpha Is Transparency"));
        // Image Count doesn't mean 'sprite count'
        // retVal.Add(new FieldData(typeof(int), t, "m_ImageCount", "Image Count"));
        // retVal.Add(new FieldData(typeof(int), t, "m_TextureDimension", "Texture Dimension"));
        // retVal.Add(new FieldData(typeof(int), t, "m_LightmapFormat", "Lightmap Format"));
        // retVal.Add(new FieldData(typeof(ColorSpace), t, "m_ColorSpace", "Color Space"));
        retVal.Add(new FieldData(typeof(FilterMode), t, "m_TextureSettings.m_FilterMode", "Filter Mode (Replacable)"));
        retVal.Add(new FieldData(typeof(AnisotropicFiltering), t, "m_TextureSettings.m_Aniso", "Anisotropic Filtering"));
        retVal.Add(new FieldData(typeof(int), t, "m_TextureSettings.m_MipBias", "Mip Bias"));
        retVal.Add(new FieldData(typeof(TextureWrapMode), t, "m_TextureSettings.m_WrapMode", "Wrap Mode (Replacable)"));
        return retVal;
      }

      if(typeof(UnityEngine.AudioClip) == t)
      {
        retVal.Add(new FieldData(typeof(AudioClipLoadType), t, "m_LoadType", "Load Type"));
        retVal.Add(new FieldData(typeof(int), t, "m_Channels", "Channels"));
        retVal.Add(new FieldData(typeof(int), t, "m_Frequency", "Frequency"));
        retVal.Add(new FieldData(typeof(int), t, "m_BitsPerSample", "Bits Per Sample"));
        retVal.Add(new FieldData(typeof(float), t, "m_Length", "Length"));
        retVal.Add(new FieldData(typeof(bool), t, "m_IsTrackerFormat", "Is Tracker Format"));
        retVal.Add(new FieldData(typeof(int), t, "m_SubsoundIndex", "Subsound Index"));
        retVal.Add(new FieldData(typeof(bool), t, "m_PreloadAudioData", "Preload Audio Data (Replacable)"));
        retVal.Add(new FieldData(typeof(bool), t, "m_LoadInBackground", "Load In Background (Replacable)"));
        retVal.Add(new FieldData(typeof(bool), t, "m_Legacy3D", "Legacy 3D"));
        // retVal.Add(new FieldData(typeof(StreamedResource), t, "m_Resource", "Resource"));
        retVal.Add(new FieldData(typeof(AudioCompressionFormat), t, "m_CompressionFormat", "Compression Format"));
        // retVal.Add(new FieldData(typeof(StreamedResource), t, "m_EditorResource", "Editor Resource"));
        retVal.Add(new FieldData(typeof(AudioCompressionFormat), t, "m_EditorCompressionFormat", "Editor Compression Format"));
        return retVal;
      }

      if(typeof(UnityEditor.Animations.AnimatorController) == t)
      {
        // State Machine (aka layer)
        retVal.Add(new FieldData(typeof(string), typeof(AnimatorStateMachine), "m_Name", "Layer/Name"));

        // Animator States
        retVal.Add(new FieldData(typeof(string), typeof(AnimatorState), "m_Name", "State/Name"));
        retVal.Add(new FieldData(typeof(float), typeof(AnimatorState), "m_Speed", "State/Speed"));
        retVal.Add(new FieldData(typeof(Motion), typeof(AnimatorState), "m_Motion", "State/AnimationClip"));
        retVal.Add(new FieldData(typeof(float), typeof(AnimatorState), "m_CycleOffset", "State/Cycle Offset"));
        retVal.Add(new FieldData(typeof(bool), typeof(AnimatorState), "m_WriteDefaultValues", "State/Write Defaults"));

        // AnimatorStateTransition
        retVal.Add(new FieldData(typeof(bool), typeof(AnimatorStateTransition), "m_Solo", "Transition/Solo"));
        retVal.Add(new FieldData(typeof(bool), typeof(AnimatorStateTransition), "m_Mute", "Transition/Mute"));
        retVal.Add(new FieldData(typeof(bool), typeof(AnimatorStateTransition), "m_HasExitTime", "Transition/Has Exit Time"));
        retVal.Add(new FieldData(typeof(float), typeof(AnimatorStateTransition), "m_TransitionDuration", "Transition/Transition Duration"));
        retVal.Add(new FieldData(typeof(float), typeof(AnimatorStateTransition), "m_TransitionOffset", "Transition/Transition Offset"));
        retVal.Add(new FieldData(typeof(float), typeof(AnimatorStateTransition), "m_ExitTime", "Transition/Exit Time"));
        retVal.Add(new FieldData(typeof(bool), typeof(AnimatorStateTransition), "m_HasFixedDuration", "Transition/Fixed Duration"));
        retVal.Add(new FieldData(typeof(TransitionInterruptionSource), typeof(AnimatorStateTransition), "m_InterruptionSource", "Transition/Interruption Source"));
        retVal.Add(new FieldData(typeof(bool), typeof(AnimatorStateTransition), "m_OrderedInterruption", "Transition/Ordered Interruption"));
        retVal.Add(new FieldData(typeof(bool), typeof(AnimatorStateTransition), "m_CanTransitionToSelf", "Transition/Can Transition To Self"));
        return retVal;
      }
      return retVal;
    }
  }
}

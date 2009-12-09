using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

using KiloWatt.Base.Animation;

namespace AnimationProcessor
{
  [ContentTypeWriter]
  public class AnimationSetWriter : ContentTypeWriter<AnimationSet>
  {
    protected override void Write(ContentWriter output, AnimationSet value)
    {
      output.WriteObject(value.AnimationDictionary);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.AnimationSetReader, KiloWatt.Base";
      return str;
    }
  }

  [ContentTypeWriter]
  public class AnimationWriter : ContentTypeWriter<Animation>
  {
    protected override void Write(ContentWriter output, Animation value)
    {
      output.Write(value.Name);
      output.Write(value.FrameRate);
      output.Write(value.NumFrames);
      output.WriteObject(value.Tracks);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.AnimationReader, KiloWatt.Base";
      return str;
    }
  }

  [ContentTypeWriter]
  public class AnimationTrackWriter : ContentTypeWriter<AnimationTrack>
  {
    protected override void Write(ContentWriter output, AnimationTrack value)
    {
      output.Write(value.BoneIndex);
      output.WriteObject(value.Keyframes);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.AnimationTrackReader, KiloWatt.Base";
      return str;
    }
  }

  [ContentTypeWriter]
  public class KeyframeWriter : ContentTypeWriter<Keyframe>
  {
    protected override void Write(ContentWriter output, Keyframe value)
    {
#if !MATRIXFRAMES
      output.Write(value.Pos);
      output.Write(value.Ori);
      output.Write(value.Scale);
#else
      output.Write(value.Transform);
#endif
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.KeyframeReader, KiloWatt.Base";
      return str;
    }
  }

  [ContentTypeWriter]
  public class AnimationTrackDictionaryWriter : ContentTypeWriter<AnimationTrackDictionary>
  {
    protected override void Write(ContentWriter output, AnimationTrackDictionary value)
    {
      output.Write(value.Count);
      foreach (KeyValuePair<int, AnimationTrack> kvp in value)
      {
        output.Write(kvp.Key);
        output.WriteObject<AnimationTrack>(kvp.Value);
      }
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.AnimationTrackDictionaryReader, KiloWatt.Base";
      return str;
    }
  }

  [ContentTypeWriter]
  public class AnimationDictionaryWriter : ContentTypeWriter<AnimationDictionary>
  {
    protected override void Write(ContentWriter output, AnimationDictionary value)
    {
      output.Write(value.Count);
      foreach (KeyValuePair<string, Animation> kvp in value)
      {
        output.Write(kvp.Key);
        output.WriteObject<Animation>(kvp.Value);
      }
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.AnimationDictionaryReader, KiloWatt.Base";
      return str;
    }
  }

  [ContentTypeWriter]
  public class SkinnedBoneWriter : ContentTypeWriter<SkinnedBone>
  {
    protected override void Write(ContentWriter output, SkinnedBone value)
    {
      output.Write(value.Name);
      output.Write(value.InverseBindTransform);
      output.Write(value.Index);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.SkinnedBoneReader, KiloWatt.Base";
      return str;
    }
  }

  [ContentTypeWriter]
  public class BoundsInfoWriter : ContentTypeWriter<BoundsInfo>
  {
    protected override void Write(ContentWriter output, BoundsInfo value)
    {
      output.Write(value.MaxScale);
      output.Write(value.MaxOffset);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string str = "KiloWatt.Base.Animation.BoundsInfoReader, KiloWatt.Base";
      return str;
    }
  }
}

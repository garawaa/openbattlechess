using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Content;

namespace KiloWatt.Base.Animation
{
  //  One track of animation data, which means a set of keyframes over time
  //  for a given bone.
  public class AnimationTrack
  {
    public AnimationTrack()
    {
    }

    public AnimationTrack(int boneIndex, Keyframe[] kfs)
    {
      Load(boneIndex, kfs);
    }
    
    Keyframe[] keyframes_;
    public Keyframe[] Keyframes { get { return keyframes_; } }
    public int NumFrames { get { return keyframes_.Length; } }
    int boneIndex_;
    public int BoneIndex { get { return boneIndex_; } }

    internal void Load(int boneIndex, Keyframe[] kfs)
    {
      keyframes_ = kfs;
      boneIndex_ = boneIndex;
    }
  }

  //  File I/O
  public class AnimationTrackReader : ContentTypeReader<AnimationTrack>
  {
    public AnimationTrackReader()
    {
    }

    protected override AnimationTrack Read(ContentReader input, AnimationTrack existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new AnimationTrack();
      int ix = input.ReadInt32();
      Keyframe[] kfs = input.ReadObject<Keyframe[]>();
      existingInstance.Load(ix, kfs);
      return existingInstance;
    }
  }
}

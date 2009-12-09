using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.ComponentModel;

using TInput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent;
using TOutput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent;

namespace PageSourceProcessor
{
  [ContentProcessor(DisplayName = "Splat Weight Processor"), DesignTimeVisible(false)]
  class SplatWeightProcessor : ContentProcessor<TInput, TOutput>
  {
    public override TOutput Process(TInput input, ContentProcessorContext context)
    {
      input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));
      PixelBitmapContent<Vector4> bm = (PixelBitmapContent<Vector4>)(((Texture2DContent)input).Mipmaps[0]);
      for (int y = 0, ym = bm.Height; y != ym; ++y)
      {
        for (int x = 0, xm = bm.Width; x != xm; ++x)
        {
          Vector4 v = bm.GetPixel(x, y);
          float w = 1 - v.W;
          if (v.Z > w) v.Z = w;
          w -= v.Z;
          if (v.Y > w) v.Y = w;
          w -= v.Y;
          if (v.X > w) v.X = w;
          System.Diagnostics.Debug.Assert(v.X + v.Y + v.Z + v.W <= 1.001f);
          //  it's OK for w to be > 0 here, as that means the base layer weight
          bm.SetPixel(x, y, v);
        }
      }
      input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
      input.GenerateMipmaps(true);
      return input;
    }
  }
}

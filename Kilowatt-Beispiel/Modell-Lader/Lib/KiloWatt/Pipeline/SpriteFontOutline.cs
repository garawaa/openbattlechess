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

namespace KiloWatt.Pipeline
{
  [ContentProcessor(DisplayName = "KiloWatt Sprite Font Processor")]
  public class SpriteFontOutline : FontDescriptionProcessor
  {
    FontDescription input_;
    ContentProcessorContext context_;
    public override SpriteFontContent Process(FontDescription input, ContentProcessorContext context)
    {
      input_ = input;
      context_ = context;
      SpriteFontContent ret = base.Process(input, context);
      System.Reflection.PropertyInfo pi = ret.GetType().GetProperty("Texture", System.Reflection.BindingFlags.NonPublic
          | System.Reflection.BindingFlags.Instance);
      if (pi != null)
      {
        if (!AddOutline(pi.GetValue(ret, null) as Texture2DContent))
        {
          throw new System.FormatException("The format of the bitmap is not recognized.");
        }
      }
      else
      {
        context.Logger.LogWarning("http://msdn.microsoft.com/", input.Identity, "Could not get property 'Texture'");
      }
      return ret;
    }

    Color borderColor_ = new Color(0, 0, 0, 0);
    [DisplayName("Border Color")]
    public Color BorderColor { get { return borderColor_; } set { borderColor_ = value; } }

    public bool AddOutline(Texture2DContent tex)
    {
      SurfaceFormat fmt;
      if (!tex.Mipmaps[0].TryGetFormat(out fmt))
      {
        return false;
      }
      if (fmt != SurfaceFormat.Color)
      {
        context_.Logger.LogImportantMessage("Converting from format {0} to Color.", fmt.ToString());
        tex.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
      }
      byte[] data = tex.Mipmaps[0].GetPixelData();
      int n = AddOutline(data, tex.Mipmaps[0].Width, tex.Mipmaps[0].Height);
      tex.Mipmaps[0].SetPixelData(data);
      context_.Logger.LogMessage("Converting bitmap {0}x{1} touches {2} pixels.", tex.Mipmaps[0].Width, tex.Mipmaps[0].Height, n);
      tex.GenerateMipmaps(true);
      return true;
    }
    
    static uint AlphaMask = (new Color(0, 0, 0, 255)).PackedValue;

    public unsafe int AddOutline(byte[] data, int width, int height)
    {
      if (width * height * 4 != data.Length)
        throw new ArgumentOutOfRangeException(String.Format("Color bitmap size {0}x{1} should be {2} bytes in length; I got passed {3}.",
            width, height, width*height*4, data.Length));
      int n = 0;
      fixed(byte *bptr = data)
      {
        uint rpl = borderColor_.PackedValue & 0xffffffU;
        //  avoid the top and bottom scanlines to make filtering easier
        for (int y = 1; y < height-1; ++y)
        {
          uint* ptr = (uint*)bptr + y * width;
          for (int x = 0; x < width; ++x)
          {
            uint left = ptr[x-1];
            uint top = ptr[x-width];
            uint right = ptr[x+1];
            uint bottom = ptr[x+width];
            uint center = ptr[x];
            if ((center & AlphaMask) > 0)   //  not a transparent pixel
            {
              if (((left & top & right & bottom) & AlphaMask) < AlphaMask)  //  neighboring a non-opaque pixel
              {
                ++n;
                ptr[x] = (center & AlphaMask) | rpl;
              }
            }
          }
        }
      }
      return n;
    }
  }
}

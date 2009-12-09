using System;
using System.Collections.Generic;

using KiloWatt.Base.Terrain;

using TInput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Texture2DContent;
using TOutput = KiloWatt.Base.Terrain.PageSource;

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
  [ContentProcessor(DisplayName = "Page Source Processor")]
  public class PageSourceProcessor : ContentProcessor<TInput, TOutput>
  {
    internal static string RuntimeAssembly(TargetPlatform p) { return "KiloWatt.Runtime"; }
    internal static string BaseAssembly(TargetPlatform p) { return "KiloWatt.Base"; }

    public PageSourceProcessor()
    {
      tsi_ = new TerrainShadeInfo();
      tsi_.terrainEffectName_ = "terrain/terrain.fx";
      tsi_.splatTextureName_ = "terrain/splat.tga";
      tsi_.baseSplatTextureName_ = "terrain/base.tga";
      tsi_.rSplatTextureName_ = "terrain/rsplat.tga";
      tsi_.gSplatTextureName_ = "terrain/gsplat.tga";
      tsi_.bSplatTextureName_ = "terrain/bsplat.tga";
      tsi_.aSplatTextureName_ = "terrain/asplat.tga";
      tsi_.nDetailTextureName_ = "terrain/ndetail.tga";
    }
    public virtual ExternalReference<TextureContent> BuildControlTexture(
        ContentProcessorContext context, string path)
    {
      return context.BuildAsset<TextureContent, TextureContent>(
        new ExternalReference<TextureContent>(path),
        "SplatWeightProcessor",
        null,
        "TextureImporter",
        path.Substring(0, path.IndexOf('.')));
    }
    public virtual ExternalReference<TextureContent> BuildTexture(
        ContentProcessorContext context, string path)
    {
      OpaqueDataDictionary texParms = new OpaqueDataDictionary();
      texParms.Add("ColorKeyEnabled", false);
      texParms.Add("GenerateMipmaps", true);
      texParms.Add("TextureFormat", TextureProcessorOutputFormat.DxtCompressed);
      return context.BuildAsset<TextureContent, TextureContent>(
        new ExternalReference<TextureContent>(path),
        "TextureProcessor",
        texParms,
        "TextureImporter",
        path.Substring(0, path.IndexOf('.')));
    }
    public override TOutput Process(TInput input, ContentProcessorContext context)
    {
      input.ConvertBitmapType(typeof(PixelBitmapContent<Single>));
      PixelBitmapContent<Single> bm = input.Mipmaps[0] as PixelBitmapContent<Single>;
      int w = bm.Width;
      int h = bm.Height;
      if (((w-2) & (w-1)) != 0 || ((h-2) & (h-1)) != 0 || 
          w < Page.DataSizeWithoutPad || h < Page.DataSizeWithoutPad)
      {
        throw new Microsoft.Xna.Framework.Content.Pipeline.InvalidContentException(
            String.Format("PageSource texture must be power of 2 plus 1 in each direction, at least {0} pixels; got {1}x{2}.",
              Page.DataSizeWithPad, w, h),
            input.Identity);
      }
      int wt = (w - 1) / Page.DataSizeWithoutPad;
      int ht = (h - 1) / Page.DataSizeWithoutPad;
      int minx = -wt/2;
      int minz = -ht/2;
      int maxx = minx+wt;
      int maxz = minz+ht;
      TOutput ps = new TOutput();
      float f = bm.GetPixel(0, 0);
      bool ok = false;
      unchecked
      {
        for (int z = minz; z < maxz; ++z)
        {
          for (int x = minx; x < maxx; ++x)
          {
            Page p = new Page();
            PageAddress pa = new PageAddress();
            pa.X = x;
            pa.Z = z;
            PageRect pr = PageAddress.RectFor(pa);
            p.SetAddress(pa, pr);
            ushort[] data = new ushort[Page.DataSizeWithPad * Page.DataSizeWithPad];
            int[] count = new int[65536];
            for (int v = 0; v < Page.DataSizeWithPad; ++v)
            {
              for (int u = 0; u < Page.DataSizeWithPad; ++u)
              {
                float q = bm.GetPixel(u + (x - minx) * Page.DataSizeWithoutPad,
                  v + (z - minz) * Page.DataSizeWithoutPad);
                if (q != f) ok = true;
                ushort b;
                if (q < 0)
                  b = 0;
                else if (q > 1)
                  b = 65535;
                else
                  b = (ushort)(q * 65535);
                count[b]++;
                data[v * Page.DataSizeWithPad + u] = b;
              }
            }
            int numBins = 0;
            for (int i = 0; i < 65536; ++i) {
              if (count[i] != 0) {
                ++numBins;
              }
            }
            p.SetData(data);
            ps.AddPage(p);
          }
        }
      }
      if (!ok)
      {
        throw new Microsoft.Xna.Framework.Content.Pipeline.InvalidContentException(
            String.Format("The input data has all the same value ({0}).", f),
            input.Identity);
      }
      TerrainShadeTag tst = new TerrainShadeTag();
      tst.effect_ = context.BuildAsset<EffectContent, CompiledEffect>(
        new ExternalReference<EffectContent>(tsi_.terrainEffectName_),
        "EffectProcessor");
      tst.splatControl_ = BuildControlTexture(context, tsi_.splatTextureName_);
      tst.base_ = BuildTexture(context, tsi_.baseSplatTextureName_);
      tst.r_ = BuildTexture(context, tsi_.rSplatTextureName_);
      tst.g_ = BuildTexture(context, tsi_.gSplatTextureName_);
      tst.b_ = BuildTexture(context, tsi_.bSplatTextureName_);
      tst.a_ = BuildTexture(context, tsi_.aSplatTextureName_);
      tst.nDetail_ = BuildTexture(context, tsi_.nDetailTextureName_);
      PixelBitmapContent<Color> normalPixels = new PixelBitmapContent<Color>(w - 1, h - 1);
      BuildNormalMap(bm, normalPixels);
      tst.normalMap_ = new Texture2DContent();
      tst.normalMap_.Mipmaps = new MipmapChain(normalPixels);
      tst.normalMap_.GenerateMipmaps(true);
      NormalizeMipmaps(tst.normalMap_.Mipmaps);
      tsi_.tag_ = tst;
      ps.ShadeInfo = tsi_;
      return ps;
    }

    protected virtual void BuildNormalMap(PixelBitmapContent<Single> bm, PixelBitmapContent<Color> n)
    {
      unchecked
      {
        int ww = n.Width;
        int hh = n.Height;
        for (int y = 0; y < hh; ++y)
        {
          for (int x = 0; x < ww; ++x)
          {
            float v00 = bm.GetPixel(x, y);
            float v01 = bm.GetPixel(x, y+1);
            float v10 = bm.GetPixel(x+1, y);
            float v11 = bm.GetPixel(x+1, y+1);
            //  80 meters up/down for the renderer
            //  two dx-es means divide by 2 (to average)
            //  two meters per triangle side means divide by 2 to normalize to meters
            //  That is crappy resolution. I should probably use floats instead of bytes 
            //  for the underlying storage pixmap...
            float ddx = (v10 + v11 - v00 - v01) * Page.DynamicRange * 0.5f * 0.5f;
            float ddy = (v01 + v11 - v00 - v10) * Page.DynamicRange * 0.5f * 0.5f;
            Vector3 v = new Vector3(-ddx, -ddy, 1.0f);
            v.Normalize();
            v = v * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);
            Color c = new Color(v);
            n.SetPixel(x, y, c);
          }
        }
      }
    }
    
    protected virtual void NormalizeMipmaps(MipmapChain mmc)
    {
      unchecked
      {
        foreach (PixelBitmapContent<Color> bmc in mmc)
        {
          for (int y = 0, yy = bmc.Height; y != yy; ++y)
          {
            for (int x = 0, xx = bmc.Width; x != xx; ++x)
            {
              Color c = bmc.GetPixel(x, y);
              Vector3 v = new Vector3((c.R * 2f) / 255f - 1f,
                  (c.G * 2f) / 255f - 1f,
                  (c.B * 2f) / 255f - 1f);
              v.Normalize();
              v = v * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);
              bmc.SetPixel(x, y, new Color(v));
            }
          }
        }
      }
    }

    TerrainShadeInfo tsi_;

    [DisplayName("Effect Name")]
    [DefaultValue("terrain/terrain.fx")]
    [Description("The Effect used for rendering the terrain")]
    public string TerrainEffectName
    {
      get { return tsi_.terrainEffectName_; }
      set { tsi_.terrainEffectName_ = value; }
    }

    [DisplayName("Splat Control Texture")]
    [DefaultValue("terrain/splat.tga")]
    [Description("The texture (RGBA) to use to control splatting")]
    public string SplatTextureName
    {
      get { return tsi_.splatTextureName_; }
      set { tsi_.splatTextureName_ = value; }
    }

    [DisplayName("1 Bottom Splat Texture")]
    [DefaultValue("terrain/base.tga")]
    [Description("The texture to use for the bottom layer")]
    public string BaseSplatTextureName
    {
      get { return tsi_.baseSplatTextureName_; }
      set { tsi_.baseSplatTextureName_ = value; }
    }

    [DisplayName("2 R Splat Texture")]
    [DefaultValue("terrain/rsplat.tga")]
    [Description("The texture to use with R weighted splat pixels")]
    public string RSplatTextureName
    {
      get { return tsi_.rSplatTextureName_; }
      set { tsi_.rSplatTextureName_ = value; }
    }

    [DisplayName("3 G Splat Texture")]
    [DefaultValue("terrain/gsplat.tga")]
    [Description("The texture to use with G weighted splat pixels")]
    public string GSplatTextureName
    {
      get { return tsi_.gSplatTextureName_; }
      set { tsi_.gSplatTextureName_ = value; }
    }

    [DisplayName("4 B Splat Texture")]
    [DefaultValue("terrain/bsplat.tga")]
    [Description("The texture to use with G weighted splat pixels")]
    public string BSplatTextureName
    {
      get { return tsi_.bSplatTextureName_; }
      set { tsi_.bSplatTextureName_ = value; }
    }

    [DisplayName("5 A Splat Texture")]
    [DefaultValue("terrain/asplat.tga")]
    [Description("The texture to use with A weighted splat pixels")]
    public string ASplatTextureName
    {
      get { return tsi_.aSplatTextureName_; }
      set { tsi_.aSplatTextureName_ = value; }
    }

    [DisplayName("6 Normal Detail Texture")]
    [DefaultValue("terrain/ndetail.tga")]
    [Description("Detail texture to perturb terrain normals")]
    public string NDetailTextureName
    {
      get { return tsi_.nDetailTextureName_; }
      set { tsi_.nDetailTextureName_ = value; }
    }
  }

  [ContentTypeWriter]
  public class PageSourceTypeWriter : ContentTypeWriter<TOutput>
  {
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Runtime.Terrain.PageSourceReader, " + PageSourceProcessor.RuntimeAssembly(targetPlatform);
    }
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Base.Terrain.PageSource, " + PageSourceProcessor.BaseAssembly(targetPlatform);
    }
    protected override void Write(ContentWriter output, TOutput value)
    {
      int cnt = value.NumPages;
      output.Write(cnt);
      foreach (Page p in value.Pages)
      {
        output.WriteRawObject<Page>(p);
      }
      output.WriteRawObject<TerrainShadeInfo>(value.ShadeInfo);
    }
  }

  [ContentTypeWriter]
  public class PageTypeWriter : ContentTypeWriter<Page>
  {
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Runtime.Terrain.PageReader, " + PageSourceProcessor.RuntimeAssembly(targetPlatform);
    }
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Runtime.Terrain.HeightmapPage, " + PageSourceProcessor.BaseAssembly(targetPlatform);
    }
    protected override void Write(ContentWriter output, Page value)
    {
      output.WriteRawObject<PageAddress>(value.Address);
      output.WriteRawObject<PageRect>(value.Rect);
      output.WriteRawObject<ushort[]>(value.PeekData);
    }
    protected override void Write(ContentWriter output, object value)
    {
      Write(output, (Page)value);
    }
  }

  [ContentTypeWriter]
  public class PageAddressWriter : ContentTypeWriter<PageAddress>
  {
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Runtime.Terrain.PageAddressReader, " + PageSourceProcessor.RuntimeAssembly(targetPlatform);
    }
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Base.Terrain.PageAddress, " + PageSourceProcessor.BaseAssembly(targetPlatform);
    }
    protected override void Write(ContentWriter output, PageAddress value)
    {
      output.Write(value.X);
      output.Write(value.Z);
    }
  }

  [ContentTypeWriter]
  public class PageRectWriter : ContentTypeWriter<PageRect>
  {
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Runtime.Terrain.PageRectReader, " + PageSourceProcessor.RuntimeAssembly(targetPlatform);
    }
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Base.Terrain.PageRect, " + PageSourceProcessor.BaseAssembly(targetPlatform);
    }
    protected override void Write(ContentWriter output, PageRect value)
    {
      output.Write(value.X);
      output.Write(value.Z);
      output.Write(value.W);
      output.Write(value.H);
    }
  }

  [ContentTypeWriter]
  public class TerrainShadeInfoWriter : ContentTypeWriter<TerrainShadeInfo>
  {
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Runtime.Terrain.TerrainShadeInfoReader, " + PageSourceProcessor.RuntimeAssembly(targetPlatform);
    }
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "KiloWatt.Base.Terrain.TerrainShadeInfo, " + PageSourceProcessor.BaseAssembly(targetPlatform);
    }
    protected override void Write(ContentWriter output, TerrainShadeInfo value)
    {
      TerrainShadeTag tst = (TerrainShadeTag)value.tag_;
      output.WriteExternalReference<CompiledEffect>(tst.effect_);
      output.WriteExternalReference<TextureContent>(tst.splatControl_);
      output.WriteExternalReference<TextureContent>(tst.base_);
      output.WriteExternalReference<TextureContent>(tst.r_);
      output.WriteExternalReference<TextureContent>(tst.g_);
      output.WriteExternalReference<TextureContent>(tst.b_);
      output.WriteExternalReference<TextureContent>(tst.a_);
      output.WriteExternalReference<TextureContent>(tst.nDetail_);
      output.WriteObject<Texture2DContent>(tst.normalMap_);
    }
  }

  public struct TerrainShadeTag {
    public ExternalReference<CompiledEffect> effect_;
    public ExternalReference<TextureContent> splatControl_;
    public ExternalReference<TextureContent> base_;
    public ExternalReference<TextureContent> r_;
    public ExternalReference<TextureContent> g_;
    public ExternalReference<TextureContent> b_;
    public ExternalReference<TextureContent> a_;
    public ExternalReference<TextureContent> nDetail_;
    public Texture2DContent normalMap_;
  }
}

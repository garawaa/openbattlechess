using System;
using System.Collections.Generic;
using System.Text;

namespace KiloWatt.Base.Terrain
{
  public class PageSource : PageOwner
  {
    public PageSource()
      : base()
    {
    }
    public PageAddress MinAddress { get { return minAddress_; } }
    PageAddress minAddress_ = new PageAddress();
    PageAddress maxAddress_ = new PageAddress();
    public PageAddress MaxAddress { get { return maxAddress_; } }
    public Page PageAt(float x, float z)
    {
      return PageAt(FloatToAddress(x, z));
    }
    public Page PageAt(PageAddress pa)
    {
      Page ret = null;
      pages_.TryGetValue(pa, out ret);
      return ret;
    }
    static PageAddress ftaRet_ = new PageAddress();
    public PageAddress FloatToAddress(float x, float z)
    {
      if (x == bounds_.X + bounds_.W) x -= 1e-3f;
      if (z == bounds_.Z + bounds_.H) z -= 1e-3f;
      ftaRet_.X = (int)Math.Floor(x / Page.TileSizeUnits);
      ftaRet_.Z = (int)Math.Floor(z / Page.TileSizeUnits);
      return ftaRet_;
    }
    object tag_;
    public object BuildTag { get { return tag_; } set { tag_ = value; } }
    TerrainShadeInfo tsi_ = new TerrainShadeInfo();
    public TerrainShadeInfo ShadeInfo { get { return tsi_; } set { tsi_ = value; } }

    public void AddPage(Page p)
    {
      if (pages_.Count == 0)
      {
        minAddress_ = p.Address;
        maxAddress_ = p.Address;
        bounds_ = p.Rect;
      }
      else
      {
        bounds_.Union(p.Rect);
        if (p.Address.X < minAddress_.X) minAddress_.X = p.Address.X;
        if (p.Address.Z < minAddress_.Z) minAddress_.Z = p.Address.Z;
        if (p.Address.X > maxAddress_.X) maxAddress_.X = p.Address.X;
        if (p.Address.Z > maxAddress_.Z) maxAddress_.Z = p.Address.Z;
      }
      p.SetOwner(this);
      pages_.Add(p.Address, p);
    }

    public void FillArray(float[] a)
    {
      int w = SamplesWidth;
      int h = SamplesHeight;
      if (a.GetLength(0) != w * h)
        throw new System.ArgumentException("Bad array size in FillArray()", "a");
      foreach (Page p in pages_.Values)
      {
        ushort[] f = p.PeekData;
        int bz = (p.Address.Z - minAddress_.Z) * Page.DataSizeWithoutPad;
        int bix = bz * w + (p.Address.X - minAddress_.X) * Page.DataSizeWithoutPad;
        for (int y = 0; y < Page.DataSizeWithPad; ++y)
        {
          for (int x = 0; x < Page.DataSizeWithPad; ++x)
          {
            a[bix + y * w + x] = f[y * Page.DataSizeWithPad + x] * Page.Scale + Page.Offset;
          }
        }
      }
    }

    Dictionary<PageAddress, Page> pages_ = new Dictionary<PageAddress, Page>();
    public int NumPages { get { return pages_.Count; } }
    public IEnumerable<Page> Pages { get { return pages_.Values; } }
    /// <summary>
    /// The Bounds are the X/Z coordinates in the global coordinate space
    /// of the total extent of page sources.
    /// </summary>
    PageRect bounds_;
    public PageRect Bounds { get { return bounds_; } }
    public int SamplesWidth { get { return (int)(bounds_.W / Page.SizePerSample) + 1; } }
    public int SamplesHeight { get { return (int)(bounds_.H / Page.SizePerSample) + 1; } }
  }
}

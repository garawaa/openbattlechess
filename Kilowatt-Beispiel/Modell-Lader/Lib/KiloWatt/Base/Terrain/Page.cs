using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using KiloWatt.Base.Physics;

namespace KiloWatt.Base.Terrain
{
  public struct PageAddress
  {
    public int X;
    public int Z;
    public static PageRect RectFor(PageAddress addr)
    {
      PageRect ret = new PageRect();
      ret.X = addr.X * Page.TileSizeUnits;
      ret.Z = addr.Z * Page.TileSizeUnits;
      ret.W = Page.TileSizeUnits;
      ret.H = Page.TileSizeUnits;
      return ret;
    }
  }
  public struct PageRect
  {
    public override string ToString()
    {
      return String.Format("PageRect({0},{1},{2},{3}) {4},{5}",
          X, Z, X + W, Z + H, W, H);
    }
    public float x, z;
    public float w, h;
    public float X
    {
      get { return x; }
      set { x = value; }
    }
    public float Z
    {
      get { return z; }
      set { z = value; }
    }
    public float W
    {
      get { return w; }
      set { w = value; System.Diagnostics.Debug.Assert(w >= 0, "Width must be positive."); }
    }
    public float H
    {
      get { return h; }
      set { h = value; System.Diagnostics.Debug.Assert(h >= 0, "Height must be positive."); }
    }
    public void Union(PageRect r)
    {
      float NX = Math.Min(X, r.X);
      float NZ = Math.Min(Z, r.Z);
      float XM = Math.Max(X + W, r.X + r.W);
      float ZM = Math.Max(Z + H, r.Z + r.H);
      X = NX;
      Z = NZ;
      W = XM - X;
      H = ZM - Z;
      System.Diagnostics.Debug.Assert(W >= 0);
      System.Diagnostics.Debug.Assert(H >= 0);
    }
    public float Distance2To(float X, float Z)
    {
      float dX = 0;
      float dZ = 0;
      if (X < this.X) dX = X - this.X;
      if (X > this.X + this.W) dX = X - this.X - this.W;
      if (Z < this.Z) dZ = Z - this.Z;
      if (Z > this.Z + this.H) dZ = Z - this.Z - this.H;
      return dX * dX + dZ * dZ;
    }
  }

  public interface PageOwner
  {
  }

  public class Page
  {
    public const float TileSizeUnits = 256.0f;
    //  300 meters high, 40 meters deep
    public const float DynamicRange = 340.0f;
    public const float Scale = DynamicRange / 65535f;
    public const float Offset = -40.0f;

    public struct CullHeights
    {
      public float top;
      public float bottom;
    }
    CullHeights[] cullQuadTree_;

    public Page()
    {
    }
    internal Page(PageOwner owner, PageAddress addr)
    {
      addr_ = addr;
      owner_ = owner;
      rect_ = PageAddress.RectFor(addr);
    }
    public void GetSamples(float[] data, float xpos, float zpos, float xw, float zw, int size)
    {
      System.Diagnostics.Debug.Assert(xw <= Page.TileSizeUnits);
      System.Diagnostics.Debug.Assert(zw <= Page.TileSizeUnits);
      System.Diagnostics.Debug.Assert(xw > 0);
      System.Diagnostics.Debug.Assert(zw > 0);
      float sampleStep = Page.DataSizeWithoutPad / (float)(size - 1);
      float xStep = sampleStep * xw / Page.TileSizeUnits;
      float zStep = sampleStep * zw / Page.TileSizeUnits;
      float xBasis = (xpos - rect_.X) / (Page.TileSizeUnits / Page.DataSizeWithoutPad);
      float zBasis = (zpos - rect_.Z) / (Page.TileSizeUnits / Page.DataSizeWithoutPad);
      System.Diagnostics.Debug.Assert(xBasis >= 0);
      System.Diagnostics.Debug.Assert(zBasis >= 0);
      System.Diagnostics.Debug.Assert(xBasis + xStep * (size - 1) <= Page.DataSizeWithoutPad);
      System.Diagnostics.Debug.Assert(zBasis + zStep * (size - 1) <= Page.DataSizeWithoutPad);
      float zz = zBasis;
      int ix = 0;
      unchecked
      {
        for (int z = 0; z < size; ++z)
        {
          float xx = xBasis;
          int zi = (int)Math.Floor(zz);
          float zd = zz - (float)zi;
          for (int x = 0; x < size; ++x)
          {
            int xi = (int)Math.Floor(xx);
            float xd = xx - (float)xi;
            //  TODO: to reduce popping, I could 
            //  calculate the maximum height of an area of 
            //  samples here.
            data[ix] = SampleScale(xi, xd, zi, zd);
            xx += xStep;
            ++ix;
          }
          zz += zStep;
        }
      }
    }
    public float HeightAt(float x, float z)
    {
      x -= rect_.X;
      z -= rect_.Z;
      float xd = x / Page.TileSizeUnits * DataSizeWithoutPad;
      float zd = z / Page.TileSizeUnits * DataSizeWithoutPad;
      int xi = (int)Math.Floor(xd);
      int zi = (int)Math.Floor(zd);
      xd -= xi;
      zd -= zi;
      //  make sure I can sample the rightmost fringe
      if (xi == DataSizeWithoutPad && xd < 1e-3)
      {
        xi = DataSizeWithoutPad - 1;
        xd = 1;
      }
      if (zi == DataSizeWithoutPad && zd < 1e-3)
      {
        zi = DataSizeWithoutPad - 1;
        zd = 1;
      }
      if (xi >= DataSizeWithPad || zi >= DataSizeWithPad)
        throw new System.Exception("Attempt to address outside of Page range.");
      return SampleScale(xi, xd, zi, zd);
    }

    internal float SampleScale(int xi, float xd, int zi, float zd)
    {
      unchecked
      {
        float tl = 0, tr = 0, bl = 0, br = 0;
        tl = data_[zi * DataSizeWithPad + xi] * Page.Scale;
        if (xd > 0)
        {
          tr = data_[zi * DataSizeWithPad + (xi + 1)] * Page.Scale;
        }
        if (zd > 0)
        {
          bl = data_[(zi + 1) * DataSizeWithPad + xi] * Page.Scale;
          if (xd > 0)
          {
            br = data_[(zi + 1) * DataSizeWithPad + (xi + 1)] * Page.Scale;
          }
        }
        return (tl * (1 - xd) + tr * xd) * (1 - zd) + (bl * (1 - xd) + br * xd) * zd + Page.Offset;
      }
    }

    public const int DataSizeWithoutPad = 128;
    public const int DataSizeWithPad = 129;
    public const float SizePerSample = TileSizeUnits / DataSizeWithoutPad;
    PageOwner owner_;
    PageAddress addr_;
    PageRect rect_;
    AABB box_;
    protected ushort[] data_;
    public ushort[] PeekData { get { return data_; } }
    public PageOwner Owner { get { return owner_; } }
    public PageAddress Address { get { return addr_; } }
    public PageRect Rect { get { return rect_; } }

    internal void SetOwner(PageOwner owner) { owner_ = owner; }

    public void SetAddress(PageAddress addr, PageRect rect)
    {
      addr_ = addr;
      rect_ = rect;
    }

    public void SetData(ushort[] data)
    {
      data_ = data;
      BuildCullQuadTree();
    }

    void BuildCullQuadTree()
    {
      cullQuadTree_ = new CullHeights[1 + 4 + 16 + 64 + 256 + 1024];
      for (int i = 0, l = cullQuadTree_.Length; i != l; ++i)
      {
        cullQuadTree_[i].bottom = Page.DynamicRange + Page.Offset;
        cullQuadTree_[0].top = Page.Offset;
      }
      int shift = 0;
      while ((1 << shift) != Page.DataSizeWithoutPad)
      {
        ++shift;
      }
      //  Whee! Here's hoping for big L2 caches!
      for (int y = 0; y < Page.DataSizeWithoutPad; ++y)
      {
        for (int x = 0; x < Page.DataSizeWithoutPad; ++x)
        {
          for (int l = 0; l < NumCullLevels; ++l)
          {
            Update(
                ref cullQuadTree_[(y >> (shift - l)) * StrideOfLevel[l] + (x >> (shift - l)) + OffsetOfLevel[l]],
                data_[y * Page.DataSizeWithPad + x]);
            Update(
                ref cullQuadTree_[(y >> (shift - l)) * StrideOfLevel[l] + (x >> (shift - l)) + OffsetOfLevel[l]],
                data_[(y + 1) * Page.DataSizeWithPad + x]);
            Update(
                ref cullQuadTree_[(y >> (shift - l)) * StrideOfLevel[l] + (x >> (shift - l)) + OffsetOfLevel[l]],
                data_[y * Page.DataSizeWithPad + (x + 1)]);
            Update(
                ref cullQuadTree_[(y >> (shift - l)) * StrideOfLevel[l] + (x >> (shift - l)) + OffsetOfLevel[l]],
                data_[(y + 1) * Page.DataSizeWithPad + (x + 1)]);
          }
        }
      }
      box_.Set(rect_.X, cullQuadTree_[0].bottom, rect_.Z,
          rect_.X+rect_.W, cullQuadTree_[0].top, rect_.Z+rect_.H);
    }

    static void Update(ref CullHeights ch, ushort v)
    {
      float f = v * Page.Scale * Page.Offset;
      if (f < ch.bottom) ch.bottom = f;
      if (f > ch.top) ch.top = f;
    }

    public static uint[] OffsetOfLevel = new uint[] { 0, 1, 1 + 4, 1 + 4 + 16, 1 + 4 + 16 + 64, 1 + 4 + 16 + 64 + 256 };
    public static uint[] StrideOfLevel = new uint[] { 1, 2, 4, 8, 16, 32 };
    public const uint NumCullLevels = 6;
    public const float SizeAtMaxLevel = Page.TileSizeUnits / (1 << (int)NumCullLevels);
    public CullHeights[] CullQuadTree { get { return cullQuadTree_; } }

  }
}

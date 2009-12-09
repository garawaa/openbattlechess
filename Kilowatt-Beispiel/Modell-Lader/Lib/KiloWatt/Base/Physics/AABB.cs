using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace KiloWatt.Base.Physics
{
  public struct AABB
  {
    public void Set(float ax, float ay, float az, float bx, float by, float bz)
    {
      Lo.X = ax; Lo.Y = ay; Lo.Z = az;
      Hi.X = bx; Hi.Y = by; Hi.Z = bz;
    }
    public void Set(Vector3 a, Vector3 b)
    {
      Lo.X = Math.Min(a.X, b.X);
      Hi.X = Math.Max(a.X, b.X);
      Lo.Y = Math.Min(a.Y, b.Y);
      Hi.Y = Math.Max(a.Y, b.Y);
      Lo.Z = Math.Min(a.Z, b.Z);
      Hi.Z = Math.Max(a.Z, b.Z);
    }
    public void Set(Vector3 center, float radius)
    {
      Lo.X = center.X - radius;
      Lo.Y = center.Y - radius;
      Lo.Z = center.Z - radius;
      Hi.X = center.X + radius;
      Hi.Y = center.Y + radius;
      Hi.Z = center.Z + radius;
    }
    public void Inflate(float ds)
    {
      System.Diagnostics.Debug.Assert(ds >= 0);
      Lo.X -= ds;
      Lo.Y -= ds;
      Lo.Z -= ds;
      Hi.X += ds;
      Hi.Y += ds;
      Hi.Z += ds;
    }
    public void Include(Vector3 pt)
    {
      if (Lo.X > pt.X) Lo.X = pt.X;
      if (Hi.X < pt.X) Hi.X = pt.X;
      if (Lo.Y > pt.Y) Lo.Y = pt.Y;
      if (Hi.Y < pt.Y) Hi.Y = pt.Y;
      if (Lo.Z > pt.Z) Lo.Z = pt.Z;
      if (Hi.Z < pt.Z) Hi.Z = pt.Z;
    }
    public void Set(Vector3 start, Vector3 dim, float len)
    {
      Set(start, start + dim * len);
    }
    public Vector3 Lo;
    public Vector3 Hi;
    public Vector3 Center { get { return (Lo + Hi) * 0.5f; } }
    public Vector3 HalfDim { get { return (Hi - Lo) * 0.5f; } }

    public bool Overlaps(AABB other)
    {
      if (other.Lo.X > Hi.X || other.Hi.X < Lo.X) return false;
      if (other.Lo.Y > Hi.Y || other.Hi.Y < Hi.Y) return false;
      if (other.Lo.Z > Hi.Z || other.Hi.Z < Hi.Z) return false;
      return true;
    }
    public void SetUnion(AABB other)
    {
      Lo.X = Math.Min(Lo.X, other.Lo.X);
      Lo.Y = Math.Min(Lo.Y, other.Lo.Y);
      Lo.Z = Math.Min(Lo.Z, other.Lo.Z);
      Hi.X = Math.Max(Hi.X, other.Hi.X);
      Hi.Y = Math.Max(Hi.Y, other.Hi.Y);
      Hi.Z = Math.Max(Hi.Z, other.Hi.Z);
    }
    public bool SetIntersection(AABB other)
    {
      Lo.X = Math.Max(Lo.X, other.Lo.X);
      Lo.Y = Math.Max(Lo.Y, other.Lo.Y);
      Lo.Z = Math.Max(Lo.Z, other.Lo.Z);
      Hi.X = Math.Min(Hi.X, other.Hi.X);
      Hi.Y = Math.Min(Hi.Y, other.Hi.Y);
      Hi.Z = Math.Min(Hi.Z, other.Hi.Z);
      if (Hi.X >= Lo.X && Hi.Y >= Lo.Y && Hi.Z >= Lo.Z)
        return true;
      Hi = Lo;
      return false;
    }
  }
}

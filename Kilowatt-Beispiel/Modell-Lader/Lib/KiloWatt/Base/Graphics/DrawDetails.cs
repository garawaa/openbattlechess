using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace KiloWatt.Base.Graphics
{
  public class DrawDetails
  {
    public GraphicsDevice dev;
    public Matrix world;
    public Matrix worldView;
    public Matrix view;
    public Matrix viewProj;
    public Matrix viewInv;
    public Matrix projection;
    public Vector3 lightDir;
    public Vector4 lightDiffuse;
    public Vector4 lightAmbient;
    public float fogDistance;
    public Vector4 fogColor;
    public float fogHeight;
    public float fogDepth;
    public int frame;

    public void CopyTo(DrawDetails o)
    {
      o.dev = dev;
      o.world = world;
      o.worldView = worldView;
      o.view = view;
      o.viewProj = viewProj;
      o.viewInv = viewInv;
      o.projection = projection;
      o.lightDir = lightDir;
      o.lightDiffuse = lightDiffuse;
      o.lightAmbient = lightAmbient;
      o.fogDistance = fogDistance;
      o.fogColor = fogColor;
      o.fogHeight = fogHeight;
      o.fogDepth = fogDepth;
      o.frame = frame;
    }
  }
}

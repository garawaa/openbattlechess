using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KiloWatt.Base.Terrain
{
  public struct TerrainShadeInfo
  {
    public string terrainEffectName_;
    public string splatTextureName_;
    public string baseSplatTextureName_;
    public string rSplatTextureName_;
    public string gSplatTextureName_;
    public string bSplatTextureName_;
    public string aSplatTextureName_;
    public string nDetailTextureName_;

    public Effect terrainEffect_;
    public Texture2D splatTexture_;
    public Texture2D baseSplatTexture_;
    public Texture2D rSplatTexture_;
    public Texture2D gSplatTexture_;
    public Texture2D bSplatTexture_;
    public Texture2D aSplatTexture_;
    public Texture2D normalTexture_;
    public Texture2D nDetailTexture_;

    public object tag_;
  }
}

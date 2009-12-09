using System;

using Microsoft.Xna.Framework.Graphics;

namespace KiloWatt.Base.Graphics
{
  public interface IScene
  {
    void AddRenderable(ISceneRenderable sr);
    void RemoveRenderable(ISceneRenderable sr);
    ISceneTexture GetSceneTexture();
  }

  public interface ISceneTexture : IDisposable
  {
    Texture2D Texture { get; }
  }
}

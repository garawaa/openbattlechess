using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace KiloWatt.Base.Graphics
{
  public interface ISceneRenderable
  {
    bool SceneDraw(DrawDetails dd);
    void SceneDrawTransparent(DrawDetails dd);
    Matrix Transform { get; }
    BoundingSphere Bounds { get; }
  }
}

using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace KiloWatt.Pipeline
{
  [ContentProcessor(DisplayName = "Scene File Processor")]
  public class SceneFileProcessor : ContentProcessor<string, string>
  {
    public override string Process(string input, ContentProcessorContext context)
    {
      XmlDocument xd = new XmlDocument();
      //  verify the integrity of the scene
      xd.LoadXml(input);
      return input;
    }
  }
}

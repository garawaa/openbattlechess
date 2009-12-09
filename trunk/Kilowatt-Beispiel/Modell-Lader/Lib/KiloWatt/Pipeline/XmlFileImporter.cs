using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TImport = System.String;

namespace KiloWatt.Pipeline
{
  /// <summary>
  /// Import an XML file as a string.
  /// </summary>
  [ContentImporter(".xml", ".scene", DisplayName = "XML as Text Importer", DefaultProcessor = "SceneFileProcessor")]
  public class XmlFileImporter : ContentImporter<TImport>
  {
    public override TImport Import(string filename, ContentImporterContext context)
    {
      XmlDocument xd = new XmlDocument();
      //  make sure that the document validates
      xd.Load(System.IO.Path.GetFullPath(filename));
      //  return the document as text
      return xd.OuterXml;
    }
  }
}

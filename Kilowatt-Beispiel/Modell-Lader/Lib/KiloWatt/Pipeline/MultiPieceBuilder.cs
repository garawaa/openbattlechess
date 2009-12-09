using System;
using System.Collections.Generic;
using System.Xml;
using System.ComponentModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace KiloWatt.Pipeline
{
  /// <summary>
  /// MultiPieceBuilder takes an XML file as input, where the XML file 
  /// describes how to build different meshes into a single output model.
  /// MultiPieceBuilder does not support animation or skinning.
  /// </summary>
  [ContentProcessor(DisplayName = "Multiple Piece Builder")]
  public class MultiPieceBuilder : ContentProcessor<System.String, List<KiloWatt.Pipeline.PieceContent>>
  {
    public override List<KiloWatt.Pipeline.PieceContent> Process(System.String input, ContentProcessorContext context)
    {
      XmlDocument xd = new XmlDocument();
      xd.LoadXml(input);
      List<PieceContent> pieceList = new List<PieceContent>();
      foreach (XmlNode xn in xd.SelectNodes("//Model"))
      {
        string relPath = xn.InnerText;
        int lio = relPath.LastIndexOf('/');
        string path = "";
        string pattern = "";
        if (lio == -1)
          pattern = relPath;
        else
        {
          path = relPath.Substring(0, lio);
          pattern = relPath.Substring(lio+1);
        }
        string[] fileNames = System.IO.Directory.GetFiles(path, pattern);
        foreach (string fileName in fileNames)
        {
          context.Logger.LogMessage("Importing model {0}");
          ModelContent mc = context.BuildAndLoadAsset<NodeContent, ModelContent>(
              new ExternalReference<NodeContent>(System.IO.Path.Combine(path, fileName)), "Dxt5ModelProcessor");
          //  1) bake material into color channel, if not existing
          //  2) bake to identity transform
          //  3) add as a node to the mesh builder
          PieceBuilder mb = PieceBuilder.NewPiece(System.IO.Path.GetFileNameWithoutExtension(xn.InnerText));
          AddMeshes(mc, mb);
          PieceContent pc = mb.FinishPiece();
          pieceList.Add(pc);
        }
      }
      return pieceList;
    }

    public static void AddMeshes(ModelContent mc, PieceBuilder mb)
    {
      foreach (ModelMeshContent mmc in mc.Meshes)
      {
        FlattenTransform(mmc.SourceMesh);
        mb.StartMesh(mmc);
        foreach (ModelMeshPartContent mmpc in mmc.MeshParts)
        {
          mb.AddMesh(mmpc);
        }
      }
    }

    public static void FlattenTransform(NodeContent nc)
    {
      if (!nc.Transform.Equals(Matrix.Identity))
        MeshHelper.TransformScene(nc, nc.Transform);
      nc.Transform = Matrix.Identity;
      if (nc.Parent != null)
        FlattenTransform(nc.Parent);
    }

    bool includeColor_ = true;
    [DefaultValue(true)]
    public bool IncludeColor { get { return includeColor_; } set { includeColor_ = value; } }

    int numTexCoords_ = 1;
    [DefaultValue(1)]
    public int NumTexCoords { get { return numTexCoords_; } set { numTexCoords_ = value; } }

    bool includeNormal_ = true;
    [DefaultValue(true)]
    public bool IncludeNormal { get { return includeNormal_; } set { includeNormal_ = value; } }

    bool includeTangents_ = true;
    [DefaultValue(true)]
    public bool IncludeTangents { get { return includeTangents_; } set { includeTangents_ = value; } }
  }

  public class PieceBuilder : IEqualityComparer<object[]>
  {
    public static PieceBuilder NewPiece(string name)
    {
      return new PieceBuilder(name);
    }
    protected PieceBuilder(string name)
    {
      name_ = name;
    }

    string name_;
    public string Name { get { return name_; } set { name_ = value; } }

    public int CreatePositionChannel()
    {
      return CreateChannel(VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
    }
    public int CreateNormalChannel()
    {
      return CreateChannel(VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
    }
    public int CreateTangentChannel()
    {
      return CreateChannel(VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0);
    }
    public int CreateBitangentChannel()
    {
      return CreateChannel(VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0);
    }
    public int CreateTexcoordChannel(int index)
    {
      return CreateChannel(VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, index);
    }
    public int CreateColorChannel()
    {
      return CreateChannel(VertexElementFormat.Color, VertexElementUsage.Color, 0);
    }

    List<VertexElement> dataChannels = new List<VertexElement>();
    List<object[]> vertexData = new List<object[]>();
    Dictionary<object[], int> vertexIndices = new Dictionary<object[], int>();
    List<ushort> triangleList = new List<ushort>();
    Color materialColor_ = Color.White;

    public int CreateChannel(VertexElementFormat format, VertexElementUsage usage, int index)
    {
      if (vertexData.Count > 0)
        throw new System.InvalidOperationException("Can't add a new vertex channel when indices are already added.");
      VertexElement ve = new VertexElement(0, 0, format, VertexElementMethod.Default, usage, (byte)index);
      dataChannels.Add(ve);
      return dataChannels.Count - 1;
    }

    public void SetMaterialColor(Color materialColor)
    {
      materialColor_ = materialColor;
    }

    object[] v0;
    object[] v1;
    object[] v2;
    byte[] vertexBufferData_;
    IndexCollection indexBufferData_;

    public void StartMesh(ModelMeshContent mmc)
    {
      v0 = new object[dataChannels.Count];
      v1 = new object[dataChannels.Count];
      v2 = new object[dataChannels.Count];

      for (int i = 0, n = dataChannels.Count; i != n; ++i)
      {
        switch (dataChannels[i].VertexElementFormat)
        {
          case VertexElementFormat.Color:
            v0[i] = new Color(255, 255, 255, 255);
            v1[i] = new Color(255, 255, 255, 255);
            v2[i] = new Color(255, 255, 255, 255);
            break;
          case VertexElementFormat.Vector2:
            v0[i] = new Vector2(0, 0);
            v1[i] = new Vector2(0, 0);
            v2[i] = new Vector2(0, 0);
            break;
          case VertexElementFormat.Vector3:
            v0[i] = new Vector3(0, 0, 0);
            v1[i] = new Vector3(0, 0, 0);
            v2[i] = new Vector3(0, 0, 0);
            break;
        }
      }
      
      vertexBufferData_ = mmc.VertexBuffer.VertexData;
      indexBufferData_ = mmc.IndexBuffer;
    }

    delegate object ReadConversion(byte[] data, int offset);

    public void AddMesh(ModelMeshPartContent mmpc)
    {
      //  build a look-up table for channel in to channel out
      VertexElement[] ves = mmpc.GetVertexDeclaration();
      int[] dstOffset = new int[ves.Length];
      ReadConversion[] converter = new ReadConversion[ves.Length];
      int stride = 0;
      for (int i = 0, n = ves.Length; i != n; ++i)
        dstOffset[i] = MatchComponent(ves[i], out converter[i], ref stride);
      object colorValue;
      if (!mmpc.Material.OpaqueData.TryGetValue("Diffuse", out colorValue)
          || colorValue == null || !(colorValue is Color))
      {
        materialColor_ = Color.White;
      }
      else
      {
        materialColor_ = (Color)colorValue;
      }
      //  walk the primitives and build triangle data
      for (int i = 0, n = mmpc.PrimitiveCount; i != n; ++i)
      {
        //  walk vertex declaration
        for (int q = 0, m = ves.Length; q != m; ++q)
        {
          if (dstOffset[q] >= 0)
          {
            int ix, bytePos;

            ix = indexBufferData_[0 + i * 3 + mmpc.StartIndex];
            bytePos = ix * stride + mmpc.BaseVertex + ves[q].Offset;
            v0[dstOffset[q]] = converter[q](vertexBufferData_, bytePos);

            ix = indexBufferData_[1 + i * 3 + mmpc.StartIndex];
            bytePos = ix * stride + mmpc.BaseVertex + ves[q].Offset;
            v1[dstOffset[q]] = converter[q](vertexBufferData_, bytePos);

            ix = indexBufferData_[2 + i * 3 + mmpc.StartIndex];
            bytePos = ix * stride + mmpc.BaseVertex + ves[q].Offset;
            v2[dstOffset[q]] = converter[q](vertexBufferData_, bytePos);
          }
        }
        //  add triangle
        AddTriangle();
      }
    }
    
    public PieceContent FinishPiece()
    {
      PieceContent ret = new PieceContent(name_);
      ret.vertexArray = BuildVertexArray(ref ret.stride);
      ret.declaration = dataChannels.ToArray();
      ret.indexArray = triangleList.ToArray();
      return ret;
    }

    delegate void WriteConversion(object src, byte[] dst, ref int offset);

    static unsafe void WriteVector2(object src, byte[] dst, ref int offset)
    {
      if (offset + 8 > dst.Length)
        throw new System.IndexOutOfRangeException("Bad data in WriteVector2");
      Vector2 v2 = (Vector2)src;
      fixed (byte* ptr = dst)
      {
        Copy((byte*)&v2, ptr + offset, 8);
        offset += 8;
      }
    }

    static unsafe void WriteVector3(object src, byte[] dst, ref int offset)
    {
      if (offset + 12 > dst.Length)
        throw new System.IndexOutOfRangeException("Bad data in WriteVector3");
      Vector3 v3 = (Vector3)src;
      fixed (byte* ptr = dst)
      {
        Copy((byte*)&v3, ptr + offset, 12);
        offset += 12;
      }
    }

    static unsafe void WriteColor(object src, byte[] dst, ref int offset)
    {
      if (offset + 4 > dst.Length)
        throw new System.IndexOutOfRangeException("Bad data in WriteColor");
      Color c = (Color)src;
      fixed (byte* ptr = dst)
      {
        Copy((byte*)&c, ptr + offset, 4);
        offset += 4;
      }
    }

    byte[] BuildVertexArray(ref int stride)
    {
      //  calc stride for output
      WriteConversion[] convert = new WriteConversion[dataChannels.Count];
      for (int i = 0, n = dataChannels.Count; i != n; ++i)
      {
        VertexElement ve = dataChannels[i];
        ve.Offset = (short)stride;
        dataChannels[i] = ve;
        stride += GetFormatSize(dataChannels[i].VertexElementFormat);
        switch (dataChannels[i].VertexElementFormat)
        {
          case VertexElementFormat.Vector2:
            convert[i] = new WriteConversion(WriteVector2);
            break;
          case VertexElementFormat.Vector3:
            convert[i] = new WriteConversion(WriteVector3);
            break;
          case VertexElementFormat.Color:
            convert[i] = new WriteConversion(WriteColor);
            break;
          default:
            throw new System.InvalidOperationException(String.Format(
                "Internal error; seeing {0} for output format is not supported.",
                dataChannels[i].VertexElementFormat));
        }
      }
      byte[] ret = new byte[vertexData.Count * stride];
      int offset = 0;
      for (int i = 0, n = vertexData.Count; i != n; ++i)
      {
        for (int j = 0, m = convert.Length; j != m; ++j)
        {
          convert[j](vertexData[i][j], ret, ref offset);
        }
      }
      return ret;
    }

    internal static unsafe void Copy(byte* src, byte* dst, int len)
    {
      while (len-- > 0)
        *(dst++) = *(src++);
    }

    static unsafe object ReadVector2FromVector2(byte[] data, int offset)
    {
      Vector2 ret = new Vector2();
      fixed (byte* bp = data)
      {
        Copy(bp + offset, (byte*)&ret, 8);
      }
      return ret;
    }

    static unsafe object ReadVector3FromVector3(byte[] data, int offset)
    {
      Vector3 ret = new Vector3();
      fixed (byte* bp = data)
      {
        Copy(bp + offset, (byte*)&ret, 12);
      }
      return ret;
    }

    static unsafe object ReadColorFromColor(byte[] data, int offset)
    {
      Color ret = new Color();
      fixed (byte* bp = data)
      {
        Copy(bp + offset, (byte*)&ret, 4);
      }
      return ret;
    }

    static unsafe object ReadColorFromVector4(byte[] data, int offset)
    {
      Vector4 v4 = new Vector4();
      Color ret;
      fixed (byte* bp = data)
      {
        Copy(bp + offset, (byte*)&v4, 16);
        ret = new Color(v4);
      }
      return ret;
    }

    static unsafe object ReadColorFromVector3(byte[] data, int offset)
    {
      Vector3 v3 = new Vector3();
      Color ret;
      fixed (byte* bp = data)
      {
        Copy(bp + offset, (byte*)&v3, 12);
        ret = new Color(v3);
      }
      return ret;
    }

    int GetFormatSize(VertexElementFormat vef)
    {
      switch (vef)
      {
        case VertexElementFormat.Color:
          return 4;
        case VertexElementFormat.Vector2:
          return 8;
        case VertexElementFormat.Vector3:
          return 12;
        case VertexElementFormat.Vector4:
          return 16;
        default:
          throw new System.FormatException(String.Format("The vertex element format {0} can't be used.", vef));
      }
    }

    int MatchComponent(VertexElement ve, out ReadConversion converter, ref int stride)
    {
      if (ve.Stream != 0)
        throw new System.FormatException("MultiPieceBuilder cannot use input meshes with multiple streams.");
      int end = ve.Offset + GetFormatSize(ve.VertexElementFormat);
      if (end > stride)
        stride = end;
      converter = null;
      for (int i = 0, n = dataChannels.Count; i != n; ++i)
      {
        if (dataChannels[i].VertexElementUsage == ve.VertexElementUsage &&
            dataChannels[i].UsageIndex == ve.UsageIndex)
        {
          switch (ve.VertexElementFormat)
          {
            case VertexElementFormat.Color:
              switch (dataChannels[i].VertexElementFormat)
              {
                case VertexElementFormat.Color:
                  converter = new ReadConversion(ReadColorFromColor);
                  return i;
                case VertexElementFormat.Vector3:
                  converter = new ReadConversion(ReadColorFromVector3);
                  return 1;
                case VertexElementFormat.Vector4:
                  converter = new ReadConversion(ReadColorFromVector4);
                  return i;
              }
              break;
            case VertexElementFormat.Vector2:
              if (dataChannels[i].VertexElementFormat == VertexElementFormat.Vector2)
              {
                converter = new ReadConversion(ReadVector2FromVector2);
                return i;
              }
              break;
            case VertexElementFormat.Vector3:
              if (dataChannels[i].VertexElementFormat == VertexElementFormat.Vector3)
              {
                converter = new ReadConversion(ReadVector3FromVector3);
                return i;
              }
              break;
          }
          throw new System.InvalidOperationException(
              String.Format("Can't map {0} to {1} for vertex element format.",
                  ve.VertexElementFormat, dataChannels[i].VertexElementFormat));
        }
      }
      return -1;
    }

    public void AddTriangle()
    {
      triangleList.Add(ProcessVertex(v0));
      triangleList.Add(ProcessVertex(v1));
      triangleList.Add(ProcessVertex(v2));
    }

    ushort ProcessVertex(object[] o)
    {
      if (o.Length != dataChannels.Count)
        throw new System.FormatException("Vertex must have same format as vertex declaration.");
      //  pre-condition color channel to multiply in material color
      for (int i = 0, n = o.Length; i != n; ++i)
      {
        if (dataChannels[i].VertexElementUsage == VertexElementUsage.Color
            && dataChannels[i].VertexElementFormat == VertexElementFormat.Color)
        {
          o[i] = Modulate((Color)o[i], materialColor_);
        }
      }
      //  re-use existing vertex if available
      int ix;
      if (!vertexIndices.TryGetValue(o, out ix))
      { //  else add new
        vertexData.Add(o);
        ix = vertexData.Count - 1;
      }
      if (ix > 65535 || ix < 1)
        throw new System.FormatException("The imported mesh is too big -- can't use more than 65,536 vertices!");
      vertexIndices.Add(o, ix);
      return (ushort)ix;
    }

    public static Color Modulate(Color a, Color b)
    {
      return new Color(a.ToVector4() * b.ToVector4());
    }

    #region IEqualityComparer<object[]> Members

    public bool Equals(object[] x, object[] y)
    {
      if (x.Length != y.Length) return false;
      for (int i = 0, n = x.Length; i != n; ++i)
        if (!x[i].Equals(y[i]))
          return false;
      return true;
    }

    public int GetHashCode(object[] obj)
    {
      int hc = 0;
      //  introduce some ordering dependency
      foreach (object o in obj)
        hc = (hc << 1) ^ o.GetHashCode() ^ (hc >> 31);
      return hc;
    }

    #endregion
  }

  public class PieceContent
  {
    public PieceContent(string n)
    {
      name = n;
    }
    public string name;
    public byte[] vertexArray;
    public ushort[] indexArray;
    public VertexElement[] declaration;
    public int stride;
  }

  [ContentTypeWriter]
  public class PieceContentWriter : ContentTypeWriter<PieceContent>
  {
    protected override void Write(ContentWriter output, PieceContent value)
    {
      output.Write(value.name);
      output.Write(value.stride);
      output.WriteRawObject<byte[]>(value.vertexArray);
      output.WriteRawObject<byte[]>(ByteArray(value.indexArray));
      output.WriteRawObject<VertexElement[]>(value.declaration);
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      string reader = "KiloWatt.Runtime.Assets.PieceReader, KiloWatt.Runtime";
      return reader;
    }

    public unsafe static byte[] ByteArray(ushort[] data)
    {
      byte[] ret = new byte[data.Length * 2];
      fixed(byte* dst = ret)
      {
        fixed(ushort* src = data)
        {
          PieceBuilder.Copy((byte*)src, dst, ret.Length);
        }
      }
      return ret;
    }
  }
}

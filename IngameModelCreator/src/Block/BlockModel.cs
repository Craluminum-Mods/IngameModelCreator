using IngameModelCreator.Systems;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace IngameModelCreator;

public class BlockModel : Block
{
    public CompositeShape CompositeShape { get; protected set; } = new();
    public Dictionary<string, CompositeTexture> CustomTextures { get; protected set; } = new();

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        CompositeShape = Attributes[attributeShape].AsObject<CompositeShape>();
        CustomTextures = Attributes[attributeTextures].AsObject<Dictionary<string, CompositeTexture>>();
    }

    public MeshData GetOrCreateMesh(ICoreClientAPI capi)
    {
        MeshData mesh = null;

        if (Client.Shape == null)
        {
            CompositeShape rcshape = CompositeShape;
            if (rcshape == null) return mesh;
            rcshape.Base.WithPathAppendixOnce(appendixJson).WithPathPrefixOnce(prefixShapes);
            Client.Shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();            
        }

        Shape shape = Client.Shape;
        if (shape == null)
        {
            return mesh;
        }

        ITexPositionSource texSource = HandleTextures(capi, shape);
        try
        {
            capi.Tesselator.TesselateShape("", shape, out mesh, texSource);
        }
        catch (Exception)
        {
            capi.Tesselator.TesselateBlock(this, out mesh);
            return mesh;
        }
        Client.Shape = shape;
        return mesh;
    }

    public ITexPositionSource HandleTextures(ICoreClientAPI capi, Shape shape, string filenameForLogging = "")
    {
        ShapeTextureSource texSource = new ShapeTextureSource(capi, shape, filenameForLogging);

        foreach ((string textureCode, CompositeTexture texture) in CustomTextures)
        {
            CompositeTexture ctex = texture.Clone();
            ctex.Bake(capi.Assets);
            texSource.textures[textureCode] = ctex;
        }
        return texSource;
    }
}

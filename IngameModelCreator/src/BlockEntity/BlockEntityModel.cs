using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace IngameModelCreator;

public class BlockEntityModel : BlockEntity
{
    public BlockModel ModelBlock => Block as BlockModel;

    public MeshData Mesh { get; protected set; }

    protected void Init()
    {
        if (Api is not ICoreClientAPI capi || ModelBlock == null)
        {
            return;
        }

        Mesh = ModelBlock.GetOrCreateMesh(capi);
    }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        Init();
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        Init();
    }

    public override void MarkDirty(bool redrawOnClient = false, IPlayer skipPlayer = null)
    {
        base.MarkDirty(redrawOnClient, skipPlayer);
        if (redrawOnClient)
        {
            Init();
        }
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        try
        {
            mesher.AddMeshData(Mesh);
        }
        catch(Exception) { }
        base.OnTesselation(mesher, tesselator);
        return true;
    }
}
global using static IngameModelCreator.Utility.Constants;
using Vintagestory.API.Common;

namespace IngameModelCreator.Systems;

public class Core : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("IngameModelCreator.BlockModelCreator", typeof(BlockModel));
        api.RegisterBlockEntityClass("IngameModelCreator.ModelCreator", typeof(BlockEntityModel));
    }
}

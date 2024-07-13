using Vintagestory.API.Client;

namespace IngameModelCreator.Utility;

public static class GuiElementExtensions
{
    public static ElementBounds BelowCopySet(ref ElementBounds bounds, double fixedDeltaX = 0.0, double fixedDeltaY = 0.0, double fixedDeltaWidth = 0.0, double fixedDeltaHeight = 0.0)
    {
        return bounds = bounds.BelowCopy(fixedDeltaX, fixedDeltaY, fixedDeltaWidth, fixedDeltaHeight);
    }

    public static ElementBounds RightCopySet(ref ElementBounds bounds, double fixedDeltaX = 0.0, double fixedDeltaY = 0.0, double fixedDeltaWidth = 0.0, double fixedDeltaHeight = 0.0)
    {
        return bounds = bounds.RightCopy(fixedDeltaX, fixedDeltaY, fixedDeltaWidth, fixedDeltaHeight);
    }
}

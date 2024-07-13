namespace IngameModelCreator.Utility;

public static class Constants
{
    public const string modid = "ingamemodelcreator";

    public const string langCodeRenderPassPrefix = $"{modid}:EnumChunkRenderPass.";
    public const string langCodeReflectiveModePrefix = $"{modid}:EnumReflectiveMode.";
    public const string langCodeWindModePrefix = $"{modid}:EnumWindBitMode.";
    public const string langCodeSidePrefix = $"{modid}:side-";
    public const string langCodeDefault = "Default";

    public const string attributeShape = "shape";
    public const string attributeTextures = "textures";
    public const string appendixJson = ".json";
    public const string prefixShapes = "shapes/";

    public const string hexRed = "#FF0000";
    public const string hexBlue = "#0000FF";
    public const string hexGreen = "#00CD00";

    public const string iconAddCustom = $"{modid}:add";
    public const string iconRemoveCustom = $"{modid}:remove";
    public const string iconDuplicateCustom = $"{modid}:duplicate";
    public const string iconPlus = "plus";

    public static string ShowDialogSetting => $"{modid}:showDialog";

    public const string guiCode = $"{modid}:modelcreatordialog";

    public const string tabCube = "cube";
    public const string tabFace = "face";

    public static class TabCube
    {
        public const string langCodeName = $"{modid}:Tab.Cube";

        public const string langCodeScale = $"{modid}:Cube.Scale";
        public const string langCodePosition = $"{modid}:Cube.Position";
        public const string langCodeOrigin = $"{modid}:Cube.Origin";
        public const string langCodeXYZRotation = $"{modid}:Cube.XYZRotation";
        public const string langCodeElementProperties = $"{modid}:Cube.ElementProperties";
        public const string langCodeShade = $"{modid}:Cube.ElementProperties.Shade";
        public const string langCodeClimateColorMap = $"{modid}:Cube.ClimateColorMap";
        public const string langCodeSeasonColorMap = $"{modid}:Cube.SeasonColorMap";
        public const string langCodeRenderPass = $"{modid}:Cube.RenderPass";
    }

    public static class TabFace
    {
        public const string langCodeName = $"{modid}:Tab.Face";

        public const string langCodeSide = $"{modid}:Face.Side";
        public const string langCodeFaceUV = $"{modid}:Face.FaceUV";
        public const string langCodeRotation = $"{modid}:Face.Rotation";
        public const string langCodeProperties = $"{modid}:Face.Properties";
        public const string langCodePropertiesEnabled = $"{modid}:Face.Properties.Enabled";
        public const string langCodePropertiesAutoResolution = $"{modid}:Face.Properties.AutoResolution";
        public const string langCodePropertiesSnapUV = $"{modid}:Face.Properties.SnapUV";
        public const string langCodeGlowLevel = $"{modid}:Face.GlowLevel";
        public const string langCodeReflectiveMode = $"{modid}:Face.ReflectiveMode";
        public const string langCodeWindMode1 = $"{modid}:Face.WindMode1";
        public const string langCodeWindMode2 = $"{modid}:Face.WindMode2";
        public const string langCodeWindMode3 = $"{modid}:Face.WindMode3";
        public const string langCodeWindMode4 = $"{modid}:Face.WindMode4";
    }
}

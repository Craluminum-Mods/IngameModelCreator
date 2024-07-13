using Cairo;
using IngameModelCreator.Systems;
using IngameModelCreator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using static IngameModelCreator.Utility.GuiElementExtensions;

namespace IngameModelCreator.GUI;

public class GuiDialogModelCreator : GuiDialog
{
    private bool recompose;
    private GuiComposer composer;
    private BlockEntityModel blockEntity;
    private int selectedElementIndex = 0;
    private int selectedFaceIndex = 0;
    private EnumTab currentTab = EnumTab.Cube;

    private List<GuiTab> tabs = new List<GuiTab>(new GuiTab[2]
{
        new GuiTab
        {
            Name = TabCube.langCodeName.Localize(),
            DataInt = 0
        },
        new GuiTab
        {
            Name = TabFace.langCodeName.Localize(),
            DataInt = 1
        }
    });

    public List<GuiTab> Tabs => tabs;

    public override string ToggleKeyCombinationCode => guiCode;

    public GuiDialogModelCreator(ICoreClientAPI capi) : base(capi)
    {
        capi.Event.RegisterGameTickListener(Every500ms, 500);
        if (Client.ShowDialog == true)
        {
            TryOpen();
        }

        ClientSettings.Inst.AddWatcher(ShowDialogSetting, delegate (bool on)
        {
            switch (on)
            {
                case true: TryOpen(); break;
                case false: TryClose(); break;
            }
        });
    }

    private void Every500ms(float dt)
    {
        if (blockEntity == null)
        {
            BlockSelection blockSel = capi?.World?.Player?.CurrentBlockSelection;
            if (blockSel != null && capi.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityModel bemodel)
            {
                blockEntity = bemodel;
            }
        }

        if (recompose || blockEntity == null)
        {
            recompose = false;
            ComposeDialog();
        }
        //ComposeDialog(); // used only for debug
    }

    private void ComposeDialog()
    {
        ClearComposers();

        if (blockEntity == null) { return; }

        Dictionary<short, string> renderPasses = new Dictionary<short, string>()
        {
            [-1] = langCodeDefault,
        };
        renderPasses.AddRange(Enum.GetValues<EnumChunkRenderPass>().ToDictionary(x => (short)x, y => $"{langCodeRenderPassPrefix}{Enum.GetName(y)}"));
        string[] renderPassNames = renderPasses.Values.ToArray();
        string[] renderPassValues = renderPasses.Keys.Select(x => x.ToString()).ToArray();

        string[] sideNames = BlockFacing.ALLFACES.Select(x => x.Code).ToArray();
        string[] sideValues = BlockFacing.ALLFACES.Select(x => x.Index.ToString()).ToArray();

        string[] faceRotationNames = new string[] { "0", "90", "180", "270" };

        string[] reflectiveModeNames = Enum.GetNames<EnumReflectiveMode>().Select(x => $"{langCodeReflectiveModePrefix}{x}".Localize()).ToArray();
        string[] reflectiveModeValues = Enum.GetValues<EnumReflectiveMode>().Select(x => x.ToString()).ToArray();

        Dictionary<int, string> windModes = new Dictionary<int, string>()
        {
            [-1] = langCodeDefault,
        };
        windModes.AddRange(Enum.GetValues<EnumWindBitMode>().ToDictionary(x => (int)x, y => $"{langCodeWindModePrefix}{Enum.GetName(y)}".Localize()));
        string[] windModeNames = windModes.Values.ToArray();
        string[] windModeValues = windModes.Keys.Select(x => x.ToString()).ToArray();

        double height = GuiElement.scaled(30);
        double textHeight = GuiElement.scaled(height) / GuiElement.scaled(1.5);
        double gap = GuiElement.scaled(10);
        double gapM = GuiElement.scaled(gap) / GuiElement.scaled(2);
        double inputWidth1 = GuiElement.scaled(60);
        double inputWidth3 = GuiElement.scaled(75);
        double inputWidth4 = GuiElement.scaled(54);
        double textWidth = GuiElement.scaled(240);
        double textInputWidth = GuiElement.scaled(180);
        double dropdownWidth = (inputWidth3 * GuiElement.scaled(3)) + (gap * GuiElement.scaled(2));
        double sliderWidth = GuiElement.scaled(5.75);

        CairoFont textFont = CairoFont.WhiteSmallText();
        CairoFont tabFont = CairoFont.WhiteSmallText().WithWeight(FontWeight.Bold);

        ElementBounds mainBounds = ElementStdBounds.AutosizedMainDialog
            .WithAlignment(EnumDialogArea.LeftTop)
            .WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, GuiStyle.DialogToScreenPadding);

        ElementBounds childBounds = new ElementBounds().WithSizing(ElementSizing.FitToChildren);
        ElementBounds backgroundBounds = childBounds.WithFixedPadding(GuiElement.scaled(15));

        ElementBounds oneBounds = ElementBounds.FixedSize(height * 3, height).WithFixedOffset(0, height);
        ElementBounds twoBounds = oneBounds.RightCopy(gap);
        ElementBounds oneBoundsReserve = null;
        ElementBounds twoBoundsReserve = null;

        try
        {
            composer = Composers[guiCode] = capi.Gui.CreateCompo(guiCode, mainBounds);
            composer.AddDialogBG(backgroundBounds, false);
            composer.AddDialogTitleBarWithBg(guiCode.Localize(), () => TryClose());
            composer.BeginChildElements(childBounds);

            composer.AddHorizontalTabs(tabs.ToArray(), oneBounds.WithFixedWidth(textWidth), OnTabClicked, tabFont, tabFont, "tabs");

            composer.AddIconButton(iconPlus, OnAddElement, oneBoundsReserve = BelowCopySet(ref oneBounds, fixedDeltaY: gap).WithFixedWidth(height));
            composer.AddIconButton(iconRemoveCustom, OnRemoveElement, RightCopySet(ref oneBounds, gap));
            composer.AddIconButton(iconDuplicateCustom, OnDuplicateElement, RightCopySet(ref oneBounds, gap));

            composer.AddTextInput(BelowCopySet(ref oneBoundsReserve, fixedDeltaY: gap).WithFixedWidth(height * 5), (val) => OnInput(val, EnumAction.Rename), key: "inputElemName");
            composer.AddInset(BelowCopySet(ref oneBoundsReserve, fixedDeltaY: gap).WithFixedSize(height * 5, height * 21));

            switch (currentTab)
            {
                case EnumTab.Cube:
                    composer.AddDynamicText("", textFont, BelowCopySet(ref twoBounds, height * 2, gap).WithFixedSize(textWidth, textHeight), "textScaleXYZ");
                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(inputWidth3, height), (val) => OnInput(val, EnumAction.Scale, EnumAxis.X), key: "inputScaleX");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnInput(val, EnumAction.Scale, EnumAxis.Y), key: "inputScaleY");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnInput(val, EnumAction.Scale, EnumAxis.Z), key: "inputScaleZ");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textPositionXYZ");
                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(inputWidth3, height), (val) => OnInput(val, EnumAction.Position, EnumAxis.X), key: "inputPositionX");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnInput(val, EnumAction.Position, EnumAxis.Y), key: "inputPositionY");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnInput(val, EnumAction.Position, EnumAxis.Z), key: "inputPositionZ");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textOriginXYZ");
                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(inputWidth3, height), (val) => OnInput(val, EnumAction.Origin, EnumAxis.X), key: "inputOriginX");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnInput(val, EnumAction.Origin, EnumAxis.Y), key: "inputOriginY");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnInput(val, EnumAction.Origin, EnumAxis.Z), key: "inputOriginZ");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textRotationXYZ");
                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(inputWidth1, height), (val) => OnInput(val, EnumAction.Rotation, EnumAxis.X), key: "inputRotationX");
                    composer.AddSlider((val) => OnRotationXYZ(val, EnumAxis.X), RightCopySet(ref twoBoundsReserve, fixedDeltaX: gap).WithFixedWidth(height * sliderWidth), "sliderRotationX");

                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap), (val) => OnInput(val, EnumAction.Rotation, EnumAxis.Y), key: "inputRotationY");
                    composer.AddSlider((val) => OnRotationXYZ(val, EnumAxis.Y), RightCopySet(ref twoBoundsReserve, fixedDeltaX: gap).WithFixedWidth(height * sliderWidth), "sliderRotationY");

                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap), (val) => OnInput(val, EnumAction.Rotation, EnumAxis.Z), key: "inputRotationZ");
                    composer.AddSlider((val) => OnRotationXYZ(val, EnumAxis.Z), RightCopySet(ref twoBoundsReserve, fixedDeltaX: gap).WithFixedWidth(height * sliderWidth), "sliderRotationZ");

                    composer.AddDynamicText("", textFont, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textElementProperties");
                    composer.AddSwitch(ToggleElementPropertiesShade, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedHeight(height), "switchElementPropertiesShade");
                    composer.AddDynamicText("", textFont, RightCopySet(ref twoBounds, fixedDeltaX: gap, fixedDeltaY: gapM).WithFixedSize(textInputWidth, textHeight), "textElementPropertiesShade");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textClimateColorMap");
                    composer.AddTextInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(textWidth, height), OnClimateInput, key: "inputClimateColorMap");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textSeasonColorMap");
                    composer.AddTextInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(textWidth, height), OnSeasonInput, key: "inputSeasonColorMap");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textRenderPass");
                    composer.AddDropDown(renderPassValues, renderPassNames, 0, OnSetRenderPass, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(textWidth, height), key: "dropdownRenderPass");
                    break;
                case EnumTab.Face:
                    composer.AddDynamicText("", textFont, BelowCopySet(ref twoBounds, height * 2, gap).WithFixedSize(textWidth, textHeight), "textFaceSide");
                    composer.AddDropDown(sideValues, sideNames, 0, OnSetFaceSide, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), "dropdownFaceSide");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceUV");
                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(inputWidth4, height), (val) => OnFaceInput(val, 0), key: "inputFaceUV0");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnFaceInput(val, 1), key: "inputFaceUV1");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnFaceInput(val, 2), key: "inputFaceUV2");
                    composer.AddNumberInput(RightCopySet(ref twoBounds, fixedDeltaX: gap), (val) => OnFaceInput(val, 3), key: "inputFaceUV3");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceRotation");
                    composer.AddDropDown(faceRotationNames, faceRotationNames, 0, OnSetFaceRotation, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), key: "dropdownFaceRotation");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceProperties");

                    composer.AddSwitch(ToggleFacePropertiesEnabled, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedHeight(height), "switchFacePropertiesEnabled");
                    composer.AddDynamicText("", textFont, RightCopySet(ref twoBounds, fixedDeltaX: gap, fixedDeltaY: gapM).WithFixedSize(textInputWidth, textHeight), "textFacePropertiesEnabled");

                    composer.AddSwitch(ToggleFacePropertiesAutoResolution, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedHeight(height), "switchFacePropertiesAutoResolution");
                    composer.AddDynamicText("", textFont, RightCopySet(ref twoBounds, fixedDeltaX: gap, fixedDeltaY: gapM).WithFixedSize(textInputWidth, textHeight), "textFacePropertiesAutoResolution");

                    composer.AddSwitch(ToggleFacePropertiesSnapUV, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedHeight(height), "switchFacePropertiesSnapUV");
                    composer.AddDynamicText("", textFont, RightCopySet(ref twoBounds, fixedDeltaX: gap, fixedDeltaY: gapM).WithFixedSize(textInputWidth, textHeight), "textFacePropertiesSnapUV");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceGlowLevel");
                    composer.AddNumberInput(twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), OnFaceGlowLevel, key: "inputFaceGlowLevel");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceReflectiveMode");
                    composer.AddDropDown(reflectiveModeValues, reflectiveModeNames, 0, OnSetFaceReflectiveMode, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), key: "dropdownFaceReflectiveMode");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceWindMode1");
                    composer.AddDropDown(windModeValues, windModeNames, 0, OnSetFaceWindMode, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), key: "dropdownFaceWindMode1");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceWindMode2");
                    composer.AddDropDown(windModeValues, windModeNames, 0, OnSetFaceWindMode, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), key: "dropdownFaceWindMode2");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceWindMode3");
                    composer.AddDropDown(windModeValues, windModeNames, 0, OnSetFaceWindMode, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), key: "dropdownFaceWindMode3");

                    composer.AddDynamicText("", textFont, twoBounds = BelowCopySet(ref twoBoundsReserve, fixedDeltaY: gap).WithFixedSize(textWidth, textHeight), "textFaceWindMode4");
                    composer.AddDropDown(windModeValues, windModeNames, 0, OnSetFaceWindMode, twoBoundsReserve = BelowCopySet(ref twoBounds, fixedDeltaY: gap).WithFixedSize(dropdownWidth, height), key: "dropdownFaceWindMode4");
                    break;
            }

            composer.EndChildElements();
            composer.Compose(focusFirstElement: false);
        }
        catch (Exception) { }

        composer.GetHorizontalTabs("tabs").activeElement = (int)currentTab;

        if (Client.Shape != null && Client.Shape.Elements.Length != 0 && Client.Shape.Elements.Length > selectedElementIndex)
        {
            ShapeElement selectedElem = Client.Shape.Elements[selectedElementIndex];
            composer?.GetTextInput("inputElemName")?.SetValue(text: selectedElem.Name);
        }

        switch (currentTab)
        {
            case EnumTab.Cube:
                composer?.GetDynamicText("textScaleXYZ")?.SetNewText(TabCube.langCodeScale.Localize());
                composer?.GetDynamicText("textPositionXYZ")?.SetNewText(TabCube.langCodePosition.Localize());
                composer?.GetDynamicText("textOriginXYZ")?.SetNewText(TabCube.langCodeOrigin.Localize());
                composer?.GetDynamicText("textRotationXYZ")?.SetNewText(TabCube.langCodeXYZRotation.Localize());
                composer?.GetDynamicText("textElementProperties")?.SetNewText(TabCube.langCodeElementProperties.Localize());
                composer?.GetDynamicText("textElementPropertiesShade")?.SetNewText(TabCube.langCodeShade.Localize());
                composer?.GetDynamicText("textClimateColorMap")?.SetNewText(TabCube.langCodeClimateColorMap.Localize());
                composer?.GetDynamicText("textSeasonColorMap")?.SetNewText(TabCube.langCodeSeasonColorMap.Localize());
                composer?.GetDynamicText("textRenderPass")?.SetNewText(TabCube.langCodeRenderPass.Localize());

                if (Client.Shape != null && Client.Shape.Elements.Length != 0 && Client.Shape.Elements.Length > selectedElementIndex)
                {
                    ShapeElement selectedElem = Client.Shape.Elements[selectedElementIndex];
                    Vec3d from = new Vec3d(selectedElem.From[0], selectedElem.From[1], selectedElem.From[2]);
                    Vec3d to = new Vec3d(selectedElem.To[0], selectedElem.To[1], selectedElem.To[2]);
                    double[] rotation = selectedElem.RotationOrigin;

                    composer?.GetNumberInput("inputScaleX")?.SetValue(text: selectedElem.ScaleX.ToString());
                    composer?.GetNumberInput("inputScaleY")?.SetValue(text: selectedElem.ScaleY.ToString());
                    composer?.GetNumberInput("inputScaleZ")?.SetValue(text: selectedElem.ScaleZ.ToString());

                    composer?.GetNumberInput("inputPositionX")?.SetValue(text: to.X.ToString());
                    composer?.GetNumberInput("inputPositionY")?.SetValue(text: to.Y.ToString());
                    composer?.GetNumberInput("inputPositionZ")?.SetValue(text: to.Z.ToString());

                    composer?.GetNumberInput("inputOriginX")?.SetValue(text: rotation == null ? 0.ToString() : rotation[0].ToString());
                    composer?.GetNumberInput("inputOriginY")?.SetValue(text: rotation == null ? 0.ToString() : rotation[1].ToString());
                    composer?.GetNumberInput("inputOriginZ")?.SetValue(text: rotation == null ? 0.ToString() : rotation[2].ToString());

                    composer?.GetNumberInput("inputRotationX")?.SetValue(text: selectedElem.RotationX.ToString());
                    composer?.GetNumberInput("inputRotationY")?.SetValue(text: selectedElem.RotationY.ToString());
                    composer?.GetNumberInput("inputRotationZ")?.SetValue(text: selectedElem.RotationZ.ToString());

                    composer?.GetSlider("sliderRotationX")?.SetValues((int)selectedElem.RotationX, -180, 180, 1);
                    composer?.GetSlider("sliderRotationY")?.SetValues((int)selectedElem.RotationY, -180, 180, 1);
                    composer?.GetSlider("sliderRotationZ")?.SetValues((int)selectedElem.RotationZ, -180, 180, 1);

                    composer?.GetSwitch("switchElementPropertiesShade")?.SetValue(selectedElem.Shade);

                    composer?.GetTextInput("inputClimateColorMap")?.SetValue(selectedElem.ClimateColorMap);

                    composer?.GetDropDown("dropdownRenderPass")?.SetSelectedIndex(selectedElem.RenderPass + 1);
                }
                break;
            case EnumTab.Face:
                composer?.GetDynamicText("textFaceSide")?.SetNewText(TabFace.langCodeSide.Localize());
                composer?.GetDynamicText("textFaceUV")?.SetNewText(TabFace.langCodeFaceUV.Localize());
                composer?.GetDynamicText("textFaceRotation")?.SetNewText(TabFace.langCodeRotation.Localize());
                composer?.GetDynamicText("textFaceProperties")?.SetNewText(TabFace.langCodeProperties.Localize());
                composer?.GetDynamicText("textFacePropertiesEnabled")?.SetNewText(TabFace.langCodePropertiesEnabled.Localize());
                composer?.GetDynamicText("textFacePropertiesAutoResolution")?.SetNewText(TabFace.langCodePropertiesAutoResolution.Localize());
                composer?.GetDynamicText("textFacePropertiesSnapUV")?.SetNewText(TabFace.langCodePropertiesSnapUV.Localize());
                composer?.GetDynamicText("textFaceGlowLevel")?.SetNewText(TabFace.langCodeGlowLevel.Localize());
                composer?.GetDynamicText("textFaceReflectiveMode")?.SetNewText(TabFace.langCodeReflectiveMode.Localize());
                composer?.GetDynamicText("textFaceWindMode1")?.SetNewText(TabFace.langCodeWindMode1.Localize());
                composer?.GetDynamicText("textFaceWindMode2")?.SetNewText(TabFace.langCodeWindMode2.Localize());
                composer?.GetDynamicText("textFaceWindMode3")?.SetNewText(TabFace.langCodeWindMode3.Localize());
                composer?.GetDynamicText("textFaceWindMode4")?.SetNewText(TabFace.langCodeWindMode4.Localize());

                if (Client.Shape != null && Client.Shape.Elements.Length != 0 && Client.Shape.Elements.Length > selectedElementIndex)
                {
                    ShapeElement selectedElem = Client.Shape.Elements[selectedElementIndex];
                    ShapeElementFace selectedFace = selectedElem.FacesResolved[selectedFaceIndex];
                    float[] uv = selectedFace.Uv;

                    composer?.GetDropDown("dropdownFaceSide")?.SetSelectedIndex(selectedFaceIndex);

                    composer?.GetNumberInput("inputFaceUV0")?.SetValue(text: uv[0].ToString());
                    composer?.GetNumberInput("inputFaceUV1")?.SetValue(text: uv[1].ToString());
                    composer?.GetNumberInput("inputFaceUV2")?.SetValue(text: uv[2].ToString());
                    composer?.GetNumberInput("inputFaceUV3")?.SetValue(text: uv[3].ToString());

                    switch (selectedFace.Rotation)
                    {
                        case 0: composer?.GetDropDown("dropdownFaceRotation")?.SetSelectedValue("0"); break;
                        case 90: composer?.GetDropDown("dropdownFaceRotation")?.SetSelectedValue("90"); break;
                        case 180: composer?.GetDropDown("dropdownFaceRotation")?.SetSelectedValue("180"); break;
                        case 270: composer?.GetDropDown("dropdownFaceRotation")?.SetSelectedValue("270"); break;
                    }

                    composer?.GetSwitch("switchFacePropertiesEnabled")?.SetValue(selectedFace.Enabled);
                    //composer?.GetSwitch("switchFacePropertiesAutoResolution")?.SetValue(autoresolutio);
                    //composer?.GetSwitch("switchFacePropertiesSnapUV")?.SetValue(snapuv);
                    composer?.GetNumberInput("inputFaceGlowLevel")?.SetValue(text: selectedFace.Glow.ToString());

                    //composer?.GetDropDown("dropdownFaceWindMode1")?.SetSelectedIndex(selectedFace.WindMode1);
                    //composer?.GetDropDown("dropdownFaceWindMode2")?.SetSelectedIndex(selectedFace.WindMode2);
                    //composer?.GetDropDown("dropdownFaceWindMode3")?.SetSelectedIndex(selectedFace.WindMode3);
                    //composer?.GetDropDown("dropdownFaceWindMode4")?.SetSelectedIndex(selectedFace.WindMode4);
                }
                break;
        }
    }

    private void OnTabClicked(int tabindex)
    {
        currentTab = (EnumTab)tabindex;
        ComposeDialog();
    }

    private void ToggleFacePropertiesEnabled(bool val)
    {
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;
        ShapeElement selectedElem = Client.Shape.Elements[selectedElementIndex];
        if (selectedElem.FacesResolved.Length == 0) return;
        ShapeElementFace selectedFace = selectedElem.FacesResolved[selectedFaceIndex];
        selectedFace.Enabled = val;
        blockEntity.MarkDirty(true);
    }

    private void ToggleFacePropertiesAutoResolution(bool val)
    {
        // not implemented
    }

    private void ToggleFacePropertiesSnapUV(bool val)
    {
        // not implemented
    }

    private void OnFaceGlowLevel(string val)
    {
        int newVal;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;
        if (!int.TryParse(val, out newVal)) return;
        if (newVal is < 0 or > 255) return;
        ShapeElement selectedElem = Client.Shape.Elements[selectedElementIndex];
        if (selectedElem.FacesResolved.Length == 0) return;
        ShapeElementFace selectedFace = selectedElem.FacesResolved[selectedFaceIndex];
        selectedFace.Glow = newVal;
        blockEntity.MarkDirty(true);
    }

    private void OnSetFaceReflectiveMode(string val, bool selected)
    {
        if (!EnumReflectiveMode.TryParse(val, out EnumReflectiveMode newVal)) return;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        Client.Shape.Elements[selectedElementIndex].FacesResolved[selectedFaceIndex].ReflectiveMode = newVal;
    }

    private void OnSetFaceWindMode(string val, bool selected)
    {
        // not implemented
        //Client.Shape.Elements[selectedElementIndex].FacesResolved[selectedFaceIndex].WindMode = newVal;
        //Client.Shape.Elements[selectedElementIndex].FacesResolved[selectedFaceIndex].WindData = newVal;
    }

    private void OnFaceInput(string val, int uvIndex)
    {
        float newVal;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;
        if (!float.TryParse(val, out newVal)) return;
        ShapeElement selectedElem = Client.Shape.Elements[selectedElementIndex];
        if (selectedElem.FacesResolved.Length == 0) return;
        ShapeElementFace selectedFace = selectedElem.FacesResolved[selectedFaceIndex];
        selectedFace.Uv[uvIndex] = newVal;
        blockEntity.MarkDirty(true);
    }

    private void OnSetFaceSide(string val, bool selected)
    {
        if (!int.TryParse(val, out int newVal)) return;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        selectedFaceIndex = newVal;
    }

    private void OnSetFaceRotation(string val, bool selected)
    {
        if (!int.TryParse(val, out int newVal)) return;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        Client.Shape.Elements[selectedElementIndex].FacesResolved[selectedFaceIndex].Rotation = newVal;
    }

    private void OnSetRenderPass(string val, bool selected)
    {
        if (!short.TryParse(val, out short newVal)) return;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        Client.Shape.Elements[selectedElementIndex].RenderPass = newVal;
    }

    private void OnClimateInput(string val)
    {
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        Client.Shape.Elements[selectedElementIndex].ClimateColorMap = val;
    }

    private void OnSeasonInput(string val)
    {
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        Client.Shape.Elements[selectedElementIndex].SeasonColorMap = val;
    }

    private void ToggleElementPropertiesShade(bool val)
    {
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        Client.Shape.Elements[selectedElementIndex].Shade = val;
    }

    private bool OnRotationXYZ(int val, EnumAxis axis)
    {
        double newVal = val;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return false;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return false;
        if (newVal is < -180 or > 180) return false;

        switch (axis)
        {
            case EnumAxis.X: Client.Shape.Elements[selectedElementIndex].RotationX = newVal; break;
            case EnumAxis.Y: Client.Shape.Elements[selectedElementIndex].RotationY = newVal; break;
            case EnumAxis.Z: Client.Shape.Elements[selectedElementIndex].RotationZ = newVal; break;
        }
        return true;
    }

    private void OnInput(string val, EnumAction action, EnumAxis axis = EnumAxis.X)
    {
        double newVal = 0;
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;
        if (action is not EnumAction.Rename && !double.TryParse(val, out newVal)) return;

        if (action == EnumAction.Rename)
        {
            Client.Shape.Elements[selectedElementIndex].Name = val;
            return;
        }

        switch (action)
        {
            case EnumAction.Rename:
                Client.Shape.Elements[selectedElementIndex].Name = val;
                break;
            case EnumAction.Scale:
                if (newVal <= 0) return;
                switch (axis)
                {
                    case EnumAxis.X: Client.Shape.Elements[selectedElementIndex].ScaleX = newVal; break;
                    case EnumAxis.Y: Client.Shape.Elements[selectedElementIndex].ScaleY = newVal; break;
                    case EnumAxis.Z: Client.Shape.Elements[selectedElementIndex].ScaleZ = newVal; break;
                }
                break;
            case EnumAction.Position:
                switch (axis)
                {
                    case EnumAxis.X:
                        Client.Shape.Elements[selectedElementIndex].To[0] = newVal;
                        Client.Shape.Elements[selectedElementIndex].From[0] = newVal - 1;
                        break;
                    case EnumAxis.Y:
                        Client.Shape.Elements[selectedElementIndex].To[1] = newVal;
                        Client.Shape.Elements[selectedElementIndex].From[1] = newVal - 1;
                        break;
                    case EnumAxis.Z:
                        Client.Shape.Elements[selectedElementIndex].To[2] = newVal;
                        Client.Shape.Elements[selectedElementIndex].From[2] = newVal - 1;
                        break;
                }
                break;
            case EnumAction.Origin:
                switch (axis)
                {
                    case EnumAxis.X:
                        Client.Shape.Elements[selectedElementIndex].RotationOrigin ??= new double[3];
                        Client.Shape.Elements[selectedElementIndex].RotationOrigin[0] = newVal;
                        break;
                    case EnumAxis.Y:
                        Client.Shape.Elements[selectedElementIndex].RotationOrigin ??= new double[3];
                        Client.Shape.Elements[selectedElementIndex].RotationOrigin[1] = newVal;
                        break;
                    case EnumAxis.Z:
                        Client.Shape.Elements[selectedElementIndex].RotationOrigin ??= new double[3];
                        Client.Shape.Elements[selectedElementIndex].RotationOrigin[2] = newVal;
                        break;
                }
                break;
            case EnumAction.Rotation:
                if (newVal is < -180 or > 180) return;
                switch (axis)
                {
                    case EnumAxis.X: Client.Shape.Elements[selectedElementIndex].RotationX = newVal; break;
                    case EnumAxis.Y: Client.Shape.Elements[selectedElementIndex].RotationY = newVal; break;
                    case EnumAxis.Z: Client.Shape.Elements[selectedElementIndex].RotationZ = newVal; break;
                }
                break;

        }
        blockEntity.MarkDirty(true);
    }

    private void OnAddElement(bool val)
    {
        ShapeElement newElement = new ShapeElement()
        {
            Name = $"Cube{selectedElementIndex + 1}",
            From = new Vec3d(0, 0, 0).ToDoubleArray(),
            To = new Vec3d(1, 1, 1).ToDoubleArray(),
            FacesResolved = new ShapeElementFace[BlockFacing.NumberOfFaces]
            {
                new ShapeElementFace() { Texture =  "#0", Uv = new float[4] { 0,0,1,1 } },
                new ShapeElementFace() { Texture =  "#0", Uv = new float[4] { 0,0,1,1 } },
                new ShapeElementFace() { Texture =  "#0", Uv = new float[4] { 0,0,1,1 } },
                new ShapeElementFace() { Texture =  "#0", Uv = new float[4] { 0,0,1,1 } },
                new ShapeElementFace() { Texture =  "#0", Uv = new float[4] { 0,0,1,1 } },
                new ShapeElementFace() { Texture =  "#0", Uv = new float[4] { 0,0,1,1 } },
            }
        };
        Client.Shape.Elements = Client.Shape.Elements.Append(newElement);
        blockEntity.MarkDirty(true);
    }

    private void OnRemoveElement(bool val)
    {
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;
        Client.Shape.Elements = Client.Shape.Elements.Remove(Client.Shape.Elements[selectedElementIndex]);
        blockEntity.MarkDirty(true);
    }

    private void OnDuplicateElement(bool val)
    {
        if (Client.Shape == null || Client.Shape.Elements.Length == 0) return;
        if (Client.Shape.Elements.Length <= selectedElementIndex) return;

        ShapeElement duplicateElement = Client.Shape.Elements[selectedElementIndex].Clone();
        duplicateElement.Name = $"Cube{selectedElementIndex + 1}";
        Client.Shape.Elements = Client.Shape.Elements.Append(duplicateElement);
        blockEntity.MarkDirty(true);
    }

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();
        Client.ShowDialog = true;
        ComposeDialog();
    }

    public override void OnGuiClosed()
    {
        base.OnGuiClosed();
        Client.ShowDialog = false;
    }
}

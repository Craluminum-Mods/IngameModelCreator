using Cairo;
using IngameModelCreator.GUI;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace IngameModelCreator.Systems;

public class Client : ModSystem
{
    public static Shape Shape { get; set; }

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Gui.RegisterDialog(new GuiDialogModelCreator(api));
        api.Gui.Icons.CustomIcons.Add(iconAddCustom, DrawAdd);
        api.Gui.Icons.CustomIcons.Add(iconRemoveCustom, DrawRemove);
        api.Gui.Icons.CustomIcons.Add(iconDuplicateCustom, DrawDuplicate);

        api.Input.RegisterHotKey(guiCode, guiCode, GlKeys.V, HotkeyType.CreativeTool, shiftPressed: true, ctrlPressed: true);
    }

    private void DrawAdd(Context ctx, int x, int y, float w, float h, double[] rgba)
    {
        x = x - 5;
        y = y + 5;
        double halfWidth = w / 2;
        double halfHeight = h / 2;

        double[] _rgba = ColorUtil.Hex2Doubles(hexRed);
        ctx.SetSourceRGBA(_rgba[0], _rgba[1], _rgba[2], _rgba[3]);
        ctx.Rectangle(x, y, w, h);
        ctx.Fill();

        _rgba = ColorUtil.Hex2Doubles(hexBlue);
        ctx.SetSourceRGBA(_rgba[0], _rgba[1], _rgba[2], _rgba[3]);
        ctx.MoveTo(x, y);
        ctx.LineTo(x + halfWidth, y - halfHeight);
        ctx.LineTo(x + w + halfWidth, y - halfHeight);
        ctx.LineTo(x + w, y);
        ctx.ClosePath();
        ctx.Fill();

        _rgba = ColorUtil.Hex2Doubles(hexGreen);
        ctx.SetSourceRGBA(_rgba[0], _rgba[1], _rgba[2], _rgba[3]);
        ctx.MoveTo(x + w, y);
        ctx.LineTo(x + w + halfWidth, y - halfHeight);
        ctx.LineTo(x + w + halfWidth, y + h - halfHeight);
        ctx.LineTo(x + w, y + h);
        ctx.ClosePath();
        ctx.Fill();
    }

    private void DrawRemove(Context ctx, int x, int y, float w, float h, double[] rgba)
    {
        double lidHeight = h / 5;
        double bodyHeight = h - lidHeight;

        ctx.SetSourceRGBA(rgba[0], rgba[1], rgba[2], rgba[3]);
        ctx.Rectangle(x, y, w, lidHeight);
        ctx.Fill();

        ctx.Rectangle(x + w / 6, y + lidHeight, w * 2 / 3, bodyHeight);
        ctx.Fill();

        ctx.SetSourceRGBA(0.3, 0.3, 0.3, rgba[3]);
        ctx.Rectangle(x + w / 3, y - lidHeight / 2, w / 3, lidHeight / 2);
        ctx.Fill();
    }

    private void DrawDuplicate(Context ctx, int x, int y, float width, float height, double[] rgba)
    {
        Matrix matrix = ctx.Matrix;
        ctx.Save();
        float w = 126f;
        float h = 129f;
        float scale = Math.Min(width / w, height / h);
        matrix.Translate((float)x + Math.Max(0f, (width - w * scale) / 2f), (float)y + Math.Max(0f, (height - h * scale) / 2f));
        matrix.Scale(scale, scale);
        ctx.Matrix = matrix;
        ctx.Operator = Operator.Over;
        ctx.LineWidth = 5.0;
        ctx.MiterLimit = 10.0;
        ctx.LineCap = LineCap.Butt;
        ctx.LineJoin = LineJoin.Miter;
        Pattern pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
        ctx.SetSource(pattern);
        ctx.NewPath();
        ctx.MoveTo(71.328125, 66.042969);
        ctx.Tolerance = 0.1;
        ctx.Antialias = Antialias.Default;
        ctx.StrokePreserve();
        pattern?.Dispose();
        ctx.Operator = Operator.Over;
        ctx.LineWidth = 5.0;
        ctx.MiterLimit = 10.0;
        ctx.LineCap = LineCap.Butt;
        ctx.LineJoin = LineJoin.Miter;
        pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
        ctx.SetSource(pattern);
        ctx.NewPath();
        ctx.MoveTo(71.328125, 46.078125);
        ctx.LineTo(71.328125, 30.828125);
        ctx.CurveTo(71.328125, 29.691406, 70.667969, 28.097656, 69.863281, 27.292969);
        ctx.LineTo(60.363281, 17.792969);
        ctx.CurveTo(59.558594, 16.988281, 57.96875, 16.328125, 56.828125, 16.328125);
        ctx.LineTo(29.898438, 16.328125);
        ctx.CurveTo(28.761719, 16.328125, 27.828125, 17.261719, 27.828125, 18.398438);
        ctx.LineTo(27.828125, 76.398438);
        ctx.CurveTo(27.828125, 77.539063, 28.761719, 78.472656, 29.898438, 78.472656);
        ctx.LineTo(50.828125, 78.472656);
        ctx.Tolerance = 0.1;
        ctx.Antialias = Antialias.Default;
        ctx.StrokePreserve();
        pattern?.Dispose();
        ctx.Operator = Operator.Over;
        pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
        ctx.SetSource(pattern);
        ctx.NewPath();
        ctx.MoveTo(58.035156, 27.6875);
        ctx.CurveTo(58.035156, 28.828125, 58.96875, 29.757813, 60.109375, 29.757813);
        ctx.LineTo(68.394531, 29.757813);
        ctx.CurveTo(69.535156, 29.757813, 69.804688, 29.097656, 69.0, 28.292969);
        ctx.LineTo(59.5, 18.796875);
        ctx.CurveTo(58.695313, 17.988281, 58.035156, 18.261719, 58.035156, 19.402344);
        ctx.ClosePath();
        ctx.MoveTo(58.035156, 27.6875);
        ctx.Tolerance = 0.1;
        ctx.Antialias = Antialias.Default;
        ctx.FillRule = FillRule.Winding;
        ctx.FillPreserve();
        pattern?.Dispose();
        ctx.Operator = Operator.Over;
        ctx.LineWidth = 5.0;
        ctx.MiterLimit = 10.0;
        ctx.LineCap = LineCap.Butt;
        ctx.LineJoin = LineJoin.Miter;
        pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
        ctx.SetSource(pattern);
        ctx.NewPath();
        ctx.MoveTo(94.328125, 95.792969);
        ctx.LineTo(94.328125, 60.578125);
        ctx.CurveTo(94.328125, 59.441406, 93.667969, 57.847656, 92.863281, 57.042969);
        ctx.LineTo(83.363281, 47.542969);
        ctx.CurveTo(82.558594, 46.738281, 80.96875, 46.078125, 79.828125, 46.078125);
        ctx.LineTo(52.898438, 46.078125);
        ctx.CurveTo(51.761719, 46.078125, 50.828125, 47.011719, 50.828125, 48.148438);
        ctx.LineTo(50.828125, 106.148438);
        ctx.CurveTo(50.828125, 107.289063, 51.761719, 108.222656, 52.898438, 108.222656);
        ctx.LineTo(92.257813, 108.222656);
        ctx.CurveTo(93.398438, 108.222656, 94.328125, 107.289063, 94.328125, 106.148438);
        ctx.LineTo(94.328125, 95.792969);
        ctx.Tolerance = 0.1;
        ctx.Antialias = Antialias.Default;
        ctx.StrokePreserve();
        pattern?.Dispose();
        ctx.Operator = Operator.Over;
        pattern = new SolidPattern(rgba[0], rgba[1], rgba[2], rgba[3]);
        ctx.SetSource(pattern);
        ctx.NewPath();
        ctx.MoveTo(81.035156, 57.4375);
        ctx.CurveTo(81.035156, 58.578125, 81.96875, 59.507813, 83.109375, 59.507813);
        ctx.LineTo(91.394531, 59.507813);
        ctx.CurveTo(92.535156, 59.507813, 92.804688, 58.847656, 92.0, 58.042969);
        ctx.LineTo(82.5, 48.546875);
        ctx.CurveTo(81.695313, 47.738281, 81.035156, 48.011719, 81.035156, 49.152344);
        ctx.ClosePath();
        ctx.MoveTo(81.035156, 57.4375);
        ctx.Tolerance = 0.1;
        ctx.Antialias = Antialias.Default;
        ctx.FillRule = FillRule.Winding;
        ctx.FillPreserve();
        pattern?.Dispose();
        ctx.Restore();
    }
}

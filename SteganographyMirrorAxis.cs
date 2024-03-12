// Name: Mirror Axis
// Submenu: Steganography
// Author: Atsuhiro
// Title: Mirror Axis
// Version: 1.1
// Desc: Mirrors the pixels on a specific axis
// Keywords:
// URL:
// Help:

#region UICode
ListBoxControl MirroredAxis = 0; // Mirrored Axis|X|Y
CheckboxControl InvertMirror = true; // Invert
DoubleSliderControl MirrorOffset = 0; // [0,1] Offset
#endregion

protected override void OnRender(IBitmapEffectOutput output)
{
    using IEffectInputBitmap<ColorBgra32> sourceBitmap = Environment.GetSourceBitmapBgra32();
    using IBitmapLock<ColorBgra32> sourceLock = sourceBitmap.Lock(new RectInt32(0, 0, sourceBitmap.Size));
    RegionPtr<ColorBgra32> sourceRegion = sourceLock.AsRegionPtr();

    RectInt32 outputBounds = output.Bounds;
    using IBitmapLock<ColorBgra32> outputLock = output.LockBgra32();
    RegionPtr<ColorBgra32> outputSubRegion = outputLock.AsRegionPtr();
    var outputRegion = outputSubRegion.OffsetView(-outputBounds.Location);

    var selection = Environment.Selection.RenderBounds;
    int selectionCenterX = (selection.Right - selection.Left) / 2 + selection.Left;
    int selectionCenterY = (selection.Bottom - selection.Top) / 2 + selection.Top;

    // offset affects the selectionCenterX and selectionCenterY
    int horizontalOffset = Math.Max(0, (int)Math.Floor( MirrorOffset * (selection.Width / 2) ) ) * (InvertMirror ? -1 : 1) + 1;
    int verticalOffset = Math.Max(0, (int)Math.Floor( MirrorOffset * (selection.Height  / 2) ) ) * (InvertMirror ? -1 : 1) + 1;

    selectionCenterX = selectionCenterX + horizontalOffset;
    selectionCenterY = selectionCenterY + verticalOffset;

    bool MirrorHorizontal = MirroredAxis==0;

    int mirroredAxisCenter = MirrorHorizontal ? selectionCenterX : selectionCenterY;

    int startIndex = MirrorHorizontal ? selection.Left : selection.Top;
    int stopIndex = MirrorHorizontal ? selection.Right : selection.Bottom;

    int mirrorAxisWidth = MirrorHorizontal ? selection.Width : selection.Height;
    int mirrorWidth = InvertMirror ? mirroredAxisCenter : mirrorAxisWidth - mirroredAxisCenter;

    #if DEBUG
    Debug.WriteLine(mirroredAxisCenter);
    #endif

    // mirroredAxisCenter = 2
    // mirrorWidth = 2
    // isEven = false
    // midpointOffset = 0

    bool isMirroredAxisEven = mirrorAxisWidth % 2 == 0;
    int midpointOffset = isMirroredAxisEven ? -1 : 0;

    for (int y = outputBounds.Top; y < outputBounds.Bottom; ++y)
    {
        if (IsCancelRequested) return;

        for (int x = outputBounds.Left; x < outputBounds.Right; ++x)
        {
            ColorBgra32 sourcePixel = sourceRegion[x, y];

            if (x <= selection.Right && x >= selection.Left && y <= selection.Bottom && y >= selection.Top) {
                
                int mirroredAxisIndex = MirrorHorizontal ? x : y;

                bool isMirroredPixel = InvertMirror ? (mirroredAxisIndex <= mirroredAxisCenter) : (mirroredAxisIndex >= mirroredAxisCenter);

                if (isMirroredPixel) {
                    //int inMin = InvertMirror ? startIndex : mirroredAxisCenter;
                    //int inMax = InvertMirror ? mirroredAxisCenter : stopIndex + midpointOffset;
                    int inMin = InvertMirror ? startIndex : mirroredAxisCenter;
                    int inMax = InvertMirror ? mirroredAxisCenter : stopIndex + midpointOffset;

                    //int outMin = InvertMirror ? stopIndex + midpointOffset - 1 : mirroredAxisCenter + midpointOffset;
                    //int outMax = InvertMirror ? mirroredAxisCenter + midpointOffset : startIndex;
                    int outMin = InvertMirror ? mirroredAxisCenter + mirrorWidth + midpointOffset : mirroredAxisCenter + midpointOffset;                    
                    int outMax = InvertMirror ? mirroredAxisCenter + 1 : outMin - mirrorWidth;

                    int pixelBeingMirroredAxisIndex = mirrorAxisWidth > 1 ? (int)MapNumber(mirroredAxisIndex, inMin, inMax, outMin, outMax) : mirroredAxisIndex;

                    #if DEBUG
                    //Debug.WriteLine(mirrorWidth);
                    #endif

                    sourcePixel = sourceRegion[
                        MirrorHorizontal ? pixelBeingMirroredAxisIndex : x,
                        MirrorHorizontal ? y : pixelBeingMirroredAxisIndex
                    ];
                }
            }

            outputRegion[x, y] = sourcePixel;
        }
    }
}

private double MapNumber(double value, double in_min, double in_max, double out_min, double out_max)
{
    if (in_min == in_max) {
        return (value == in_min) ? out_min : out_max;
    }
    return out_min + (out_max - out_min) * ((value - in_min) / (in_max - in_min));
}
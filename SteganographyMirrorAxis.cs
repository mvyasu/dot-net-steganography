// Name: Mirror Axis
// Submenu: Steganography
// Author: Atsuhiro
// Title: Mirror Axis
// Version: 1.2
// Desc: Mirrors the pixels on a specific axis
// Keywords:
// URL:
// Help:

#region UICode
ListBoxControl MirroredAxis = 0; // Mirrored Axis|X|Y
CheckboxControl InvertMirror = false; // Invert
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
    
    int mirrorDirection = (InvertMirror ? -1 : 1);

    bool mirroredHorizontal = MirroredAxis==0;
    int mirroredAxisLength = mirroredHorizontal ? selection.Width : selection.Height;
    
    // this can be half a number or a full number
    // if it's a full number, then the axis has an even number of pixels
    // if it's not a full number, then it must an odd number of pixels
    double mirroredAxisCenter = (mirroredAxisLength / 2) + (MirrorOffset * (mirroredAxisLength / 2) * mirrorDirection);

    // sectionA is used for the left or top side
    // if it's even, we'll just minus it by 1 to make the end not match the start of sectionB
    Dictionary<string, int> sectionA = new Dictionary<string, int>();
    sectionA["Start"] = 0;
    sectionA["End"] = (int)Math.Floor(mirroredAxisCenter) + ((mirroredAxisCenter % 1) == 0 ? -1 : 0);
    sectionA["Length"] = sectionA["End"] - sectionA["Start"];

    // sectionB is used for right or bottom side
    Dictionary<string, int> sectionB = new Dictionary<string, int>();
    sectionB["Start"] = (int)Math.Ceiling(mirroredAxisCenter);
    sectionB["End"] = Math.Max(mirroredAxisLength - 1, sectionB["Start"]);
    sectionB["Length"] = sectionB["End"] - sectionB["Start"];

    //mirroredSection is the section that is being mirrored
    //mirroringSection is the section that gets written to
    Dictionary<string, int> mirroredSection = InvertMirror ? sectionB : sectionA;
    Dictionary<string, int> mirroringSection = InvertMirror ? sectionA : sectionB;

    int inMin = InvertMirror ? mirroringSection["End"] : mirroringSection["Start"];
    int inMax = InvertMirror ? mirroringSection["Start"] : mirroringSection["End"];

    int outMin = InvertMirror ? mirroredSection["Start"] : mirroredSection["End"];
    int outMax = InvertMirror ? mirroredSection["Start"] + mirroringSection["Length"] : mirroredSection["End"] - mirroringSection["Length"];

    #if DEBUG
    Debug.WriteLine("sectionA");
    Debug.WriteLine(sectionA["Start"]);
    Debug.WriteLine(sectionA["End"]);
    Debug.WriteLine("---------------");
    #endif

    #if DEBUG
    Debug.WriteLine("sectionB");
    Debug.WriteLine(sectionB["Start"]);
    Debug.WriteLine(sectionB["End"]);
    Debug.WriteLine("---------------");
    #endif

    for (int y = outputBounds.Top; y < outputBounds.Bottom; ++y)
    {
        if (IsCancelRequested) return;

        for (int x = outputBounds.Left; x < outputBounds.Right; ++x)
        {
            ColorBgra32 sourcePixel = sourceRegion[x, y];

            if (mirroredAxisLength > 1 && x <= selection.Right && x >= selection.Left && y <= selection.Bottom && y >= selection.Top) {
                
                int mirroringPixelIndex = mirroredHorizontal ? x : y;

                bool willContainMirroringPixel = mirroringPixelIndex >= mirroringSection["Start"] && mirroringPixelIndex <= mirroringSection["End"];

                if (willContainMirroringPixel) {
                    int mirroredPixelIndex = mirroringSection["Length"] > 0 ? (int)MapNumber(mirroringPixelIndex, inMin, inMax, outMin, outMax) : outMax;

                    sourcePixel = sourceRegion[
                        mirroredHorizontal ? mirroredPixelIndex : x,
                        mirroredHorizontal ? y : mirroredPixelIndex
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
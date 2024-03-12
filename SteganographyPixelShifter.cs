// Name: Pixel Shifter
// Submenu: Steganography
// Author: Atsuhiro
// Title: Pixel Shifter
// Version: 1.0
// Desc: Shifts the even and odd rows/columns in opposite directions
// Keywords:
// URL:
// Help:

// For help writing a Bitmap plugin: https://boltbait.com/pdn/CodeLab/help/tutorial/bitmap/

#region UICode
LabelComment XTitle = "X Shift"; // X Shift
IntSliderControl XSizeAmount = 1; // [1,2000] Size
IntSliderControl XShiftAmount = 0; // [-10000,10000] Shift
LabelComment YTitle = "Y Shift"; // Y Shift
IntSliderControl YSizeAmount = 1; // [1,2000] Size
IntSliderControl YShiftAmount = 0; // [-10000,10000] Shift
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

    for (int y = outputBounds.Top; y < outputBounds.Bottom; ++y)
    {
        if (IsCancelRequested) return;

        for (int x = outputBounds.Left; x < outputBounds.Right; ++x)
        {
            int xDirection = (y / XSizeAmount) % 2 == 0 ? -1 : 1;
            int yDirection = (x / YSizeAmount) % 2 == 0 ? -1 : 1;
            
            outputRegion[x, y] = sourceRegion[
                Mod(x + XShiftAmount * xDirection, sourceRegion.Width), 
                Mod(y + YShiftAmount * yDirection, sourceRegion.Height)
            ];
        }
    }
}

public static int Mod(int x, int m)
{
    return (x % m + m) % m;
}
// Name: Channel Looper
// Submenu: Steganography
// Author: Atsuhiro
// Title: Channel Looper
// Version: v0.1
// Desc: Loops the color channels to reveal patterns
// Keywords:
// URL:
// Help:

#region UICode
IntSliderControl RGBMax = 255; // [0,255] {!UseChannelSliders} RGB Max
CheckboxControl UseChannelSliders = false; // RGBA Sliders
IntSliderControl RedMax = 255; // [0,255] {UseChannelSliders} Red Max
IntSliderControl GreenMax = 255; // [0,255] {UseChannelSliders} Green Max
IntSliderControl BlueMax = 255; // [0,255] {UseChannelSliders} Blue Max
IntSliderControl AlphaMax = 255; // [0,255] {UseChannelSliders} Alpha Max
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

    // Loop through the output canvas tile
    for (int y = outputBounds.Top; y < outputBounds.Bottom; ++y)
    {
        if (IsCancelRequested) return;

        for (int x = outputBounds.Left; x < outputBounds.Right; ++x)
        {
            // Get your source pixel
            ColorBgra32 sourcePixel = sourceRegion[x,y];

            if (UseChannelSliders) {
                sourcePixel.R = (byte)MapPixel(sourcePixel.R, RedMax); // Red
                sourcePixel.G = (byte)MapPixel(sourcePixel.G, GreenMax); // Green
                sourcePixel.B = (byte)MapPixel(sourcePixel.B, BlueMax); // Blue
                sourcePixel.A = (byte)MapPixel(sourcePixel.A, AlphaMax); // Alpha
            } else {
                sourcePixel.R = (byte)MapPixel(sourcePixel.R, RGBMax); // Red
                sourcePixel.G = (byte)MapPixel(sourcePixel.G, RGBMax); // Green
                sourcePixel.B = (byte)MapPixel(sourcePixel.B, RGBMax); // Blue
            }

            // Save your pixel to the output canvas
            outputRegion[x,y] = sourcePixel;
        }
    }
}

private double MapNumber(double value, double in_min, double in_max, double out_min, double out_max)
{
    return out_min + (out_max - out_min) * ((value - in_min) / (in_max - in_min));
}

private int MapPixel(int num, int max)
{
    if (max == 0) {
        // Handle division by zero gracefully
        return 0;
    } else if (num % max == 0 && num != 0) {
        return max;
    } else {
        return (int)Math.Round(MapNumber(num % max, 0, max, 0, 255));
    }
}
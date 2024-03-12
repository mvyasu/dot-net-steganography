// Name: LSB Mixer
// Submenu: Steganography
// Author: Atsuhiro
// Title: LSB Mixer
// Version: 1.0
// Desc: Mixes the least significant bits of selected color channels
// Keywords:
// URL:
// Help:

#region UICode
CheckboxControl UseRedLSB = false; // Red
IntSliderControl RedLSBValue = 0; // [0,254] {UseRedLSB} Red LSB
CheckboxControl UseGreenLSB = false; // Green
IntSliderControl GreenLSBValue = 0; // [0,254] {UseGreenLSB} Green LSB
CheckboxControl UseBlueLSB = false; // Blue
IntSliderControl BlueLSBValue = 0; // [0,254] {UseBlueLSB} Blue LSB
CheckboxControl UseAlphaLSB = false; // Alpha
IntSliderControl AlphaLSBValue = 0; // [0,254] {UseAlphaLSB} Alpha LSB
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
            
            List<(bool useLSB, byte channelValue, int lsbValue)> channels = new List<(bool, byte, int)> {
                (UseRedLSB, sourcePixel.R, RedLSBValue),
                (UseGreenLSB, sourcePixel.G, GreenLSBValue),
                (UseBlueLSB, sourcePixel.B, BlueLSBValue),
                (UseAlphaLSB, sourcePixel.A, AlphaLSBValue)
            };

            foreach (var channelInfo in channels) {
                // Access individual elements of the tuple
                bool useLSB = channelInfo.useLSB;
                byte channelValue = channelInfo.channelValue;
                int lsbValue = channelInfo.lsbValue;

                if (!useLSB) {
                    continue;
                }

                byte lsb = (byte)(channelValue & (lsbValue + 1));
                
                int grayscaleValue = (lsb == 0) ? 255 : 0;

                sourcePixel.R = (byte)grayscaleValue;
                sourcePixel.G = (byte)grayscaleValue;
                sourcePixel.B = (byte)grayscaleValue;
                sourcePixel.A = 255;

                if (lsb==1) {
                    break;
                }
            }

            // Save your pixel to the output canvas
            outputRegion[x,y] = sourcePixel;
        }
    }
}
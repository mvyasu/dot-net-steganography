// Name: Color Randomizer
// Submenu: Steganography
// Author: Atsuhiro
// Title: Color Randomizer
// Version: 1.0
// Desc: Randomizes each pixel by using its RGB values as a seed
// Keywords:
// URL:
// Help:

// For help writing a Bitmap plugin: https://boltbait.com/pdn/CodeLab/help/tutorial/bitmap/

#region UICode
IntSliderControl ColorMapSeed = 0; // [0,64] Seed
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
    //uint seed = RandomNumber.InitializeSeed(RandomNumberRenderSeed, outputBounds.Location);

    int neighborhoodSize = 3;
    int halfSize = neighborhoodSize / 2;

    // Loop through the output canvas tile
    for (int y = outputBounds.Top; y < outputBounds.Bottom; ++y)
    {
        if (IsCancelRequested) return;

        for (int x = outputBounds.Left; x < outputBounds.Right; ++x)
        {
            // Get your source pixel
            ColorBgra32 sourcePixel = sourceRegion[x,y];

             // Keep the most significant 4 bits
            //sourcePixel.R = (byte)(sourcePixel.R >> 25);
            //sourcePixel.G = (byte)(sourcePixel.G >> 25);
            //sourcePixel.B = (byte)(sourcePixel.B >> 25);

            // TODO: Change source pixel according to some algorithm
            //int seed = (sourcePixel.R << 16) | (sourcePixel.G << 8) | sourcePixel.B;
            //List<ColorBgra> neighborhoodPixels = new List<ColorBgra>();
            //for (int yOffset = -halfSize; yOffset <= halfSize; yOffset++)
            //{
                //for (int xOffset = -halfSize; xOffset <= halfSize; xOffset++)
                //{
                    //int neighborX = x + xOffset;
                    //int neighborY = y + yOffset;

                    // Ensure neighbor is within bounds
                    //if (neighborX >= 0 && neighborX < sourceRegion.Width && neighborY >= 0 && neighborY < sourceRegion.Height)
                    //{
                        //neighborhoodPixels.Add(sourceRegion[neighborX, neighborY]);
                    //}
                //}
            //}

            int seed = sourcePixel.R + sourcePixel.B + sourcePixel.G + ColorMapSeed; //+ GenerateSeed(neighborhoodPixels);
            Random rand = new Random(seed);

            byte randomR = (byte)rand.Next(256);
            byte randomG = (byte)rand.Next(256);
            byte randomB = (byte)rand.Next(256);

            // Generate random color
            sourcePixel.R = randomR;
            sourcePixel.G = randomG;
            sourcePixel.B = randomB;

            // Save your pixel to the output canvas
            outputRegion[x,y] = sourcePixel;
        }
    }
}

//int GenerateSeed(List<ColorBgra> pixels)
//{
    // Combine neighboring pixel values to generate seed
    //int seed = 0;
    //foreach (var pixel in pixels)
    //{
        //seed ^= pixel.R + pixel.G + pixel.B;
    //}
    //return seed;
//}
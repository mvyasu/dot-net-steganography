// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:

// For help writing a Bitmap plugin: https://boltbait.com/pdn/CodeLab/help/tutorial/bitmap/

#region UICode
IntSliderControl VarianceThreshold = 64; // [0,1000] Threshold
CheckboxControl ThresholdInverted = false; // Inverted
ColorWheelControl NoiseColor = ColorBgra.FromBgr(60, 20, 220); // [Crimson] Noise Color
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    // Define the size of the neighborhood (e.g., 3x3)
    int neighborhoodSize = 3;
    int halfSize = neighborhoodSize / 2;

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        for (int x = rect.Left; x < rect.Right; x++)
        {
            // Collect neighboring pixel values
            List<byte> neighborhoodIntensities = new List<byte>();
            for (int yOffset = -halfSize; yOffset <= halfSize; yOffset++)
            {
                for (int xOffset = -halfSize; xOffset <= halfSize; xOffset++)
                {
                    int neighborX = x + xOffset;
                    int neighborY = y + yOffset;

                    // Ensure neighbor is within bounds
                    if (neighborX >= 0 && neighborX < src.Width && neighborY >= 0 && neighborY < src.Height)
                    {
                        byte intensity = src[neighborX, neighborY].GetIntensityByte();
                        neighborhoodIntensities.Add(intensity);
                    }
                }
            }

            // Calculate the local variance
            double localVariance = CalculateLocalVariance(neighborhoodIntensities);

            float varianceThreshold = MapNumber(VarianceThreshold, 0, 1000, 0, 5);

            // Determine if the pixel is noisy based on the local variance
            bool isNoisy = ThresholdInverted ? localVariance < varianceThreshold : localVariance > varianceThreshold; // Adjust threshold as needed

            // Set the pixel color based on noise detection
            dst[x, y] = isNoisy ? NoiseColor : src[x, y];
        }
    }
}

double CalculateLocalVariance(List<byte> intensities)
{
    // Convert byte intensities to integers
    List<int> integerIntensities = intensities.Select(i => (int)i).ToList();

    // Calculate the mean intensity
    double mean = integerIntensities.Average();

    // Calculate the sum of squared differences from the mean
    double sumSquaredDiff = integerIntensities.Sum(x => Math.Pow(x - mean, 2));

    // Calculate the variance
    double variance = sumSquaredDiff / integerIntensities.Count;

    return variance;
}

private float MapNumber(float value, float in_min, float in_max, float out_min, float out_max)
{
    return out_min + (out_max - out_min) * ((value - in_min) / (in_max - in_min));
}
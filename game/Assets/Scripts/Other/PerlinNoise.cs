using UnityEngine;

public static class PerlinNoise
{


    public static Texture2D GenerateTexture(float scale, float offsetX, float offsetY, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);

        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                Color color = CalculateColor(x,y, scale, offsetX, offsetY, width, height);
                texture.SetPixel(x,y,color);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Color CalculateColor(int x, int y, float scale, float offsetX, float offsetY, int width, int height)
    {
        float xCoord = (float)x/width * scale + offsetX;
        float yCoord = (float)y/height * scale + offsetY;

        float sample = Mathf.PerlinNoise(xCoord,yCoord);
        return new Color(sample, sample, sample);
    }


    /*
    Texture2D GenerateTextureFalloff()
    {
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = CalculateFalloff(x, y);
                texture.SetPixel(x, y, new Color(value, value, value));
            }
        }

        texture.Apply();
        return texture;
    }

    float CalculateFalloff(int x, int y)
    {
        // Normalizacja do -1..1
        float nx = (float)x / width * 2f - 1f;
        float ny = (float)y / height * 2f - 1f;

        float dist = 0f;

        switch (shape)
        {
            case FalloffShape.Circle:
                dist = Mathf.Sqrt(nx * nx + ny * ny); // koło
                break;
            case FalloffShape.Diamond:
                dist = Mathf.Abs(nx) + Mathf.Abs(ny); // romb
                break;
            case FalloffShape.Square:
                dist = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny)); // kwadrat
                break;
        }

        // falloff
        float falloff = Mathf.Pow(dist, gradientPower);
        float value = Mathf.Clamp01(baseHeight - falloff * falloffStrength);

        return value;
    }
    */
}

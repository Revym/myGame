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
}

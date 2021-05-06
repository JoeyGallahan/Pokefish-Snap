using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, Vector2 offset)
    {
        System.Random rand = new System.Random(seed);
        float[,] noiseMap = new float[width, height];

        //For normalization
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                noiseMap[x, y] = Mathf.PerlinNoise(x / width * scale + offset.x, y / width * scale + offset.y);

                if (noiseMap[x, y] > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseMap[x, y];
                }
                else if (noiseMap[x, y] < minNoiseHeight)
                {
                    minNoiseHeight = noiseMap[x, y];
                }
            }
        }

        //Normalize the noisemap
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    /*
     * MapWidth / Height the size of our noise map
     * Seed allows us to reuse the same noise map and better (pseudo)randomize new ones
     * Scale allows us to zoom in/out of the noise map
     * Octave is a single level/layer of noise. Multiple octaves allow more points of randomization
     * Lacunarity is the frequency of octaves. It will increase for each octave. Kind of like making a more wavy sin wave. 
     * Persistance is the amplitude of octaves. It will decrease for each octave. Kind of like smoothing the sin wave down towards the center (on the y axis).
     * Offset allows us to scroll through the noise
     */
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persisance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //Allows for seeds
        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + offset.x;
            float offsetY = rand.Next(-100000, 100000) + offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        //How zoomed into the noise map you want to be basically
        if (scale <= 0.0f)
        {
            scale = 0.0001f;
        }

        //For normalization
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //Allows us to zoom in/out in the center of the map
        float halfWidth = mapWidth / 2.0f;
        float halfHeight = mapHeight / 2.0f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1f; //y Axis scaling
                float frequency = 0.5f; //x Axis scaling. Higher the frequency, the more the height values will change
                float noiseHeight = 0; //current height value

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[o].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[o].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY); // *2 - 1 allows negative values so our noise height can actually decrease
                    noiseHeight += perlinValue * amplitude; //make the noise build off of the previous octave

                    amplitude *= persisance; //decrease each time
                    frequency *= lacunarity; //increase each time
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        //Normalize the noisemap
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
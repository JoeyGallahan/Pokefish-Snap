using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    /*
     * MapWidth / Height the size of our noise map
     * Seed allows us to reuse the same noise map and better (pseudo)randomize new ones
     * Scale allows us to zoom in/out of the noise map
     * Octave is a single level/layer of noise. Multiple octaves allow more points of randomization
     * Lacunarity is the frequency of octaves. It will increase for each octave. Kind of like making a more wavy sin wave. 
     * Persistance is the amplitude of octaves. It will decrease for each octave. Kind of like smoothing the sin wave down towards the center (on the y axis).
     * Offset allows us to scroll through the noise
     * AnimationCurve lets us make smoother transitions between heights, more frequent valleys, steeper slopes, etc etc. Its just a normalized curve.
     * IsWater because the water blocks should change over time, but the ground blocks should not
     */
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persisance, float lacunarity, Vector2 offset, AnimationCurve heightCurve, bool isWater = false)
    {
        if (isWater)
        {
            seed++; //We just want the water seed to be different than the ground seed            
        }

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
        float halfWidth = (mapWidth) / 2.0f;
        float halfHeight = (mapHeight) / 2.0f;

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

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                    noiseHeight += perlinValue * amplitude; //just make the noise build off of the previous octave

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
                noiseMap[x, y] = heightCurve.Evaluate(Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]));
            }
        }
        return noiseMap;
    }

    public static float[,] GenerateWaterMap(int mapWidth, int mapHeight, int seed, float scale, Vector2 offset, float deltaTime)
    {
        float[,] waterMap = new float[mapWidth, mapHeight];
        System.Random rand = new System.Random(seed);

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
                float sampleX = (x - halfWidth) / scale + offset.x;
                float sampleY = (y - halfHeight) / scale + offset.y;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                waterMap[x, y] = Mathf.Cos(perlinValue + ChunkManager.WaterSpeed.magnitude * deltaTime);

                if (waterMap[x, y] > maxNoiseHeight)
                {
                    maxNoiseHeight = waterMap[x, y];
                }
                else if (waterMap[x, y] < minNoiseHeight)
                {
                    minNoiseHeight = waterMap[x, y];
                }
            }
        }
        //Normalize the noisemap
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                waterMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, waterMap[x, y]);
            }
        }

        return waterMap;
    }
}
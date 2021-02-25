// Copyright (C) 2020 - UMons
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageExtruder
{
    private float SaturationThreshold = 0.1f;
    private float ValueThreshold = 0.8f;

    private Color32[] pixels;
    private int[,] map;

    public void Extrude(Texture2D textureRef, Color colorFor1, float sizeBetweenPixels, MeshFilter ceilMeshFilter, MeshFilter wallsMeshFilter, float WallsHeight, bool createGround = false, MeshFilter groundMeshFilter = null)
    {
        MeshGenerator meshGen = new MeshGenerator();

        if (WallsHeight > 0.0001f)
        {
            if (CreateMapFromTexture(textureRef, colorFor1))
            {
                meshGen.GenerateMeshes(map, sizeBetweenPixels, ceilMeshFilter, wallsMeshFilter, WallsHeight, createGround, groundMeshFilter);
            }
        }
    }

    public Texture2D BlackOrWhite(Texture2D myTexture)
    {
        pixels = myTexture.GetPixels32();
        float h, s, v;
        for (int i = 0; i < pixels.Length; ++i)
        {
            Color.RGBToHSV(pixels[i], out h, out s, out v);
            if (s < SaturationThreshold && v < ValueThreshold)
            {
                    pixels[i] = Color.black;
            }
        }

        Texture2D texture = new Texture2D(myTexture.width, myTexture.height, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    private float ColorDistance(Color color, Color colorRef)
    {
        return Mathf.Abs(color.r - colorRef.r) + Mathf.Abs(color.b - colorRef.b) + Mathf.Abs(color.g - colorRef.g);
    }

    public Texture2D CorrectColor(Texture2D myTexture)
    {
        pixels = myTexture.GetPixels32();
        Color result;
        float distWhite;
        float distRed;
        float distBlue;
        float distGreen;
        float distBlack;
        float dist;

        for (int i = 0; i < pixels.Length; ++i)
        {
            distWhite = ColorDistance(pixels[i], Color.white);
            distRed = ColorDistance(pixels[i], Color.red);
            distBlue = ColorDistance(pixels[i], Color.blue);
            distGreen = ColorDistance(pixels[i], Color.green);
            distBlack = ColorDistance(pixels[i], Color.black);

            dist = distWhite;
            result = Color.white;

            if (dist > distRed)
            {
                dist = distRed;
                result = Color.red;
            }

            if (dist > distBlue)
            {
                dist = distBlue;
                result = Color.blue;
            }

            if (dist > distGreen)
            {
                dist = distGreen;
                result = Color.green;
            }

            if (dist > distBlack)
            {
                dist = distBlack;
                result = Color.black;
            }

            pixels[i] = result;
        }

        Texture2D texture = new Texture2D(myTexture.width, myTexture.height, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    private bool CreateMapFromTexture(Texture2D myTexture, Color colorFor1)
    {
        bool isColorPresent = false;
        int width = myTexture.width;
        int height = myTexture.height;

        map = new int[width + 2, height + 2];
        int result;

        for (int i = 0; i < pixels.Length; ++i)
        {
            if (pixels[i] == colorFor1)
            {
                result = 1;
                isColorPresent = true;
            }
            else
            {
                result = 0;
            }

            map[(i % width) + 1, (i / width) + 1] = result;
        }
        
        return isColorPresent;
    }
}

//using UnityEngine;
//using System.Collections;
//using System;

//public class Extrude : MonoBehaviour
//{

//    public int width;
//    public int height;

//    public string seed;
//    public bool useRandomSeed;

//    [Range(0, 100)]
//    public int randomFillPercent;

//    int[,] map;

//    void Start()
//    {
//        GenerateMap();
//    }

//    void Update()
//    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            GenerateMap();
//        }
//    }

//    void GenerateMap()
//    {
//        map = new int[width, height];
//        RandomFillMap();

//        for (int i = 0; i < 5; i++)
//        {
//            SmoothMap();
//        }

//        MeshGenerator meshGen = GetComponent<MeshGenerator>();
//        meshGen.GenerateMesh(map, 1);
//    }


//    void RandomFillMap()
//    {
//        if (useRandomSeed)
//        {
//            seed = Time.time.ToString();
//        }

//        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

//        for (int x = 0; x < width; x++)
//        {
//            for (int y = 0; y < height; y++)
//            {
//                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
//                {
//                    map[x, y] = 1;
//                }
//                else
//                {
//                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
//                }
//            }
//        }
//    }

//    void SmoothMap()
//    {
//        for (int x = 0; x < width; x++)
//        {
//            for (int y = 0; y < height; y++)
//            {
//                int neighbourWallTiles = GetSurroundingWallCount(x, y);

//                if (neighbourWallTiles > 4)
//                    map[x, y] = 1;
//                else if (neighbourWallTiles < 4)
//                    map[x, y] = 0;

//            }
//        }
//    }

//    int GetSurroundingWallCount(int gridX, int gridY)
//    {
//        int wallCount = 0;
//        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
//        {
//            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
//            {
//                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
//                {
//                    if (neighbourX != gridX || neighbourY != gridY)
//                    {
//                        wallCount += map[neighbourX, neighbourY];
//                    }
//                }
//                else
//                {
//                    wallCount++;
//                }
//            }
//        }

//        return wallCount;
//    }


//    void OnDrawGizmos()
//    {
//        /*
//        if (map != null) {
//            for (int x = 0; x < width; x ++) {
//                for (int y = 0; y < height; y ++) {
//                    Gizmos.color = (map[x,y] == 1)?Color.black:Color.white;
//                    Vector3 pos = new Vector3(-width/2 + x + .5f,0, -height/2 + y+.5f);
//                    Gizmos.DrawCube(pos,Vector3.one);
//                }
//            }
//        }
//        */
//    }
//}
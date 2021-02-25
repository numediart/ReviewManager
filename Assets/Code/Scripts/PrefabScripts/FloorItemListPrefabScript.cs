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

﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorItemListPrefabScript : MonoBehaviour
{
    public Text FloorNameTxt;
    public Image BackGroundImg;
    public GameObject mesh;
    public Texture2D texture2D;
    public Texture2D Heatmap;
    public string floorImageBase64String;

    private FloorsManagerScript ScriptRef;
    private bool isSelected = false;
    private int id;

    private float[,] heatmapFloat;
    private float sigma;
    private byte heatmapAlpha = 80;


    public int GetIndex()
    {
        return ProjectManagerScript.Instance.SimulationSetup.GetFloorIndex(id);
    }



    public FloorStruct GetFloorStruct()
    {
        return ProjectManagerScript.Instance.SimulationSetup.FloorList[GetIndex()];
    }



    public void SetFloorStruct(FloorStruct newFloorStruct)
    {
        ProjectManagerScript.Instance.SimulationSetup.FloorList[GetIndex()] = newFloorStruct;
    }



    public void SetFloorTexture(Texture2D floorTexture)
    {
        texture2D = floorTexture;
        Heatmap = new Texture2D(texture2D.width, texture2D.height);
        floorImageBase64String = Convert.ToBase64String(texture2D.EncodeToPNG());
        sigma = Math.Min(texture2D.width, texture2D.height) * 0.05f;
        CreateHeatmap();
    }



    public void Init(GameObject floorMesh, Texture2D floorTexture, FloorsManagerScript script, int floorId)
    {
        id = floorId;
        mesh = floorMesh;
        ScriptRef = script;
        FloorNameTxt.text = GetFloorStruct().Name + " (" + GetFloorStruct().Id + ")";
        SetFloorTexture(floorTexture);
    }



    public void UpdateButtonName()
    {
        FloorNameTxt.text = GetFloorStruct().Name + " (" + GetFloorStruct().Id + ")";
    }



    public void FloorItemListBtnOnClick()
    {
        if (isSelected)
        {
            BackGroundImg.color = Color.white;
            ScriptRef.RemoveSelectedFloorId(GetFloorStruct().Id); ;
            isSelected = false;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                bool added = ScriptRef.AddSelectedFloorId(GetFloorStruct().Id);
                if (!added)
                    ScriptRef.SetSelectedFloorItem(GetIndex());
                BackGroundImg.color = new Color(172.0f / 255.0f, 148.0f / 255.0f, 1.0f);
                isSelected = true;
            }
            else
            {
                ScriptRef.SetSelectedFloorItem(GetIndex());
                BackGroundImg.color = new Color(172.0f / 255.0f, 148.0f / 255.0f, 1.0f);
                isSelected = true;
            }
        }
    }

    public void Select()
    {
        BackGroundImg.color = new Color(172.0f / 255.0f, 148.0f / 255.0f, 1.0f);
        isSelected = true;
    }

    public void Deselect()
    {
        BackGroundImg.color = Color.white;
        isSelected = false;
    }



    public void CreateHeatmap()
    {
        heatmapFloat = new float[texture2D.width, texture2D.height];
        NoteFilter filter = new NoteFilter();
        filter.AddFloor(GetFloorStruct().Id);
        foreach (NoteStruct n in ProjectManagerScript.Instance.GetNotesFiltered(filter))
        {
            int noteX = (int)(n.Data.Location.XCoordinate * texture2D.width);
            // invert Y as Data.Location is relative to top left corner and texture origin is on bottom left corner
            int noteY = (int)((1.0f - n.Data.Location.YCoordinate) * texture2D.height);
            Vector2 noteVector = new Vector2(noteX, noteY);

            for (int i = 0; i < texture2D.width; i++)
            {
                for (int j = 0; j < texture2D.height; j++)
                {
                    heatmapFloat[i, j] += GaussianValue(Vector2.Distance(new Vector2(i, j), noteVector), sigma);
                }
            }
        }
        // convert float array to texture
        UpdateHeatmapImage();
    }



    public void UpdateHeatmap(NoteStruct newNote)
    {
        int noteX = (int)(newNote.Data.Location.XCoordinate * texture2D.width);
        // invert Y as Data.Location is relative to top left corner and texture origin is on bottom left corner
        int noteY = (int)((1.0f - newNote.Data.Location.YCoordinate) * texture2D.height);
        Vector2 noteVector = new Vector2(noteX, noteY);

        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                heatmapFloat[i, j] += GaussianValue(Vector2.Distance(new Vector2(i, j), noteVector), sigma);
            }
        }
        UpdateHeatmapImage();
    }



    public void UpdateHeatmapImage()
    {
        bool showHeatmap = FloorsManagerScript.Instance.ShowHeatmap;
        float heatmapMax = heatmapFloat.Cast<float>().Max();
        Color32[] heatmapPixels = Heatmap.GetPixels32();
        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                float hue = 0.667f * (1.0f - heatmapFloat[i, j] / heatmapMax);
                heatmapPixels[j * texture2D.width + i] = Color.HSVToRGB(hue, 0.95f, 0.95f);
                heatmapPixels[j * texture2D.width + i].a = heatmapAlpha;
            }
        }
        Heatmap.SetPixels32(heatmapPixels);
        Heatmap.Apply();
        FloorPrefabScript script = mesh.GetComponent<FloorPrefabScript>();
        if(showHeatmap)
            script.GroundMeshFilter.GetComponent<Renderer>().materials[1].mainTexture = Heatmap;
        else
        {
            Texture2D tex = new Texture2D(texture2D.width, texture2D.height, TextureFormat.ARGB32, false);
            Color[] fillPixels = new Color[tex.width * tex.height];
            for (int i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = Color.clear;
            }
            tex.SetPixels(fillPixels);
            tex.Apply();
            script.GroundMeshFilter.GetComponent<Renderer>().materials[1].mainTexture = tex;
        }
    }



    private float GaussianValue(float x, float sigma)
    {
        return (float)( Math.Exp( -(x * x) / (2 * (sigma * sigma)) ) 
                        / (sigma * Math.Sqrt(2 * Math.PI)) );
    }
}

// Copyright (C) 2020 - UMons
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;



[Serializable]
public struct FloorStruct
{
    public string Name;
    public int Id;
    public float BlackHeight;
    public float RedHeight;
    public float BlueHeight;
    public float GreenHeight;
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float Rotation;
    public float scaleX;
    public float scaleZ;

    public override string ToString()
    {
        return "FloorStruct : {" +
            "Name : " + Name + ", " +
            "Id : " + Id + ", " +
            "BlackHeight : " + BlackHeight + ", " +
            "RedHeight : " + RedHeight + ", " +
            "BlueHeight : " + BlueHeight + ", " +
            "GreenHeight : " + GreenHeight + ", " +
            "PositionX : " + PositionX + ", " +
            "PositionY : " + PositionY + ", " +
            "PositionZ : " + PositionZ + ", " +
            "Rotation : " + Rotation + ", " +
            "scaleX : " + scaleX + ", " +
            "scaleZ : " + scaleZ +
            "}";
    }
}

[Serializable]
public struct NoteStruct
{
    public ReceivedData Data;
    public GameObject ObjRef;
    public Texture2D Image;

    public override string ToString()
    {
        return "NoteStruct : {" +
            "ID : " + Data.Image + ", " +
            //"Message : " + Data.Title + " : " + Data.Content + ", " +
            "Author : " + Data.Author + ", " +
            "Date : " + Data.Date + ", " +
            "Location on floor : (" + Data.Location.XCoordinate + ", " + Data.Location.YCoordinate + "), " +
            "Floor ID : " + Data.Location.Id + ", " +
            "Tags : " + Data.Tags +
            "}";
    }
}

public class NoteFilter
{
    public List<int> FloorsIds;
    public List<string> Authors;
    public bool WithMessageOnly;
    public EmotionData EmotionCloseTo;
    public float EmotionDistance;
    public string Tags;

    public NoteFilter()
    {
        FloorsIds = new List<int>();           // updated by floor manager
        Authors = new List<string>();       // all authors will be selected if left empty
        WithMessageOnly = false;            // keep only notes with a message in Content field
        EmotionCloseTo = new EmotionData(); // coordinates of center of disk on emotion graph
        EmotionDistance = 2;                // radius of disk on emotion graph
        Tags = "";                          // not used yet
    }

    public override string ToString()
    {
        return "NoteFilter : {" +
            "FloorsIds : [" + String.Join(", ", FloorsIds) + "], " +
            "Authors : [" + String.Join(", ", Authors) + "], " +
            "WithMessageOnly : " + WithMessageOnly + ", " +
            "EmotionCloseTo : (" + EmotionCloseTo.Intensity + ", " + EmotionCloseTo.Valence + "), " +
           "Tags : " + Tags +
           "}";
    }

    public void AddFloor(int floorId)
    {
        if(!FloorsIds.Contains(floorId))
            FloorsIds.Add(floorId);
    }

    public void AddManyFloors(List<int> floorsIds)
    {
        foreach(int id in floorsIds)
        {
            AddFloor(id);
        }
    }

    public NoteFilter Copy()
    {
        NoteFilter copy = new NoteFilter();
        copy.FloorsIds.AddRange(this.FloorsIds);
        copy.Authors.AddRange(this.Authors);
        copy.WithMessageOnly = this.WithMessageOnly;
        copy.EmotionCloseTo = new EmotionData(this.EmotionCloseTo.Valence, this.EmotionCloseTo.Intensity);
        copy.EmotionDistance = this.EmotionDistance;                
        copy.Tags = this.Tags;
        return copy;
    }
}

[Serializable]
public class ReceivedData
{
    public string Title;
    public string Content;
    public EmotionData Emotion;
    public LocationData Location;
    public string Image;
    public float ImageOrientation;
    public string Tags;
    public string Author;
    public string Date;
    public string Session;

    public static ReceivedData CreateFromJSON(string jsonString)
    {
        ReceivedData d = JsonUtility.FromJson<ReceivedData>(jsonString);
        d.Title = d.Title ?? "";
        d.Content = d.Content ?? "";
        d.Tags = d.Tags ?? "";
        d.Author = d.Author ?? "";
        d.Date = d.Date ?? "";
        d.Session = d.Session ?? "";
        return d;
    }
}

[Serializable]
public class EmotionData
{
    public float Valence;
    public float Intensity;

    public EmotionData(float v, float i)
    {
        Valence = v;
        Intensity = i;
    }

    public EmotionData()
    {
        Valence = 0;
        Intensity = 0;
    }
}

[Serializable]
public class LocationData
{
    //x and y are in horizontal plan (texture) [0;1]
    //Id is floor ID
    public float XCoordinate;
    public float YCoordinate;
    public int Id;

    public bool CloseTo(LocationData other)
    {
        return Math.Abs(XCoordinate - other.XCoordinate) < 0.001 &&
                Math.Abs(YCoordinate - other.YCoordinate) < 0.001 && 
                Id == other.Id;
    }
}

public class RandomString
{
    public static string CreateRandomString(int lenght, bool useUpperCase = false, bool useNumbers = false)
    {
        string availableChars = "abcdefghijklmnopqrstuvwxyz";
        if (useUpperCase) availableChars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (useNumbers) availableChars += "0123456789";

        StringBuilder builder = new StringBuilder();
        System.Random random = new System.Random();
        char c;
        for (int i = 0; i < lenght; i++)
        {
            c = availableChars[random.Next(0, availableChars.Length)];
            builder.Append(c);
        }
        return builder.ToString();
    }
}
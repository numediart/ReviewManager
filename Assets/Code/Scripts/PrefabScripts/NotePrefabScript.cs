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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePrefabScript : MonoBehaviour
{
    public MeshRenderer[] Planes;


    public void Init(Texture2D texture, float imageOrientation)
    {
        foreach (MeshRenderer p in Planes)
        {
            p.materials[0].SetTexture("_MainTex", texture);
            Vector3 rot = p.transform.localEulerAngles;
            rot.y = -imageOrientation;
            p.transform.localRotation = Quaternion.Euler(rot);
        }
        SetHighlight(false);
    }



    public void SetHighlight(bool highlight)
    {
        float alpha = highlight ? 0.4f : 0.0f;
        Color c = Planes[0].materials[1].color;
        c.a = alpha;
        foreach (MeshRenderer p in Planes)
        {
            p.materials[1].color = c;
        }
    }
}

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

public class CameraDataScript : MonoBehaviour
{
    public Vector3 LookAt = new Vector3(0.0f, 0.0f, 0.0f);
    public GameObject Target;

    public void UpdateTargetPosition()
    {
        Target.transform.position = LookAt;

        if (gameObject.GetComponent<Camera>().orthographic)
        {

        }
        else
        {
            float factor = Vector3.Distance(LookAt, transform.position) * 0.02f;
            Target.transform.localScale = new Vector3(factor, factor, factor);
        }
    }
}

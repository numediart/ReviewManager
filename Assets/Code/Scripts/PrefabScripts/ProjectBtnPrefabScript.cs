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
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ProjectBtnPrefabScript : MonoBehaviour
{
    private string projectName;
    private string simulationXmlDirectory;
    private ProjectCanvasScript projectCanvasScript;

    public void Init(string name, string simulationXmlFullPath, ProjectCanvasScript script)
    {
        projectName = name;
        simulationXmlDirectory = simulationXmlFullPath;
        projectCanvasScript = script;

        gameObject.GetComponentInChildren<Text>().text = name;
    }

    public void ProjectBtnOnClick()
    {
        projectCanvasScript.OpenBtnOnSuccess(simulationXmlDirectory);
    }
}

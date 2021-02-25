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

public class ConfirmExitAppPanelScript : MonoBehaviour
{
    public WebsocketServerScript WebsocketServerScriptRef;

    public void YesButtonOnClick()
    {
        if (WebsocketServerScriptRef.IsServerStarted())
        {
            WebsocketServerScriptRef.StopServer();
        }
        Application.Quit();
        gameObject.SetActive(false);
    }

    public void NoButtonOnClick()
    {
        gameObject.SetActive(false);
    }
}

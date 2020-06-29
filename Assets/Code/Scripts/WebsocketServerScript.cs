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
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Net.Sockets;
using System.Text;

// needs websocket-sharp.dll compiled with code from https://github.com/sta/websocket-sharp



public class WSNotesReceiver : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log("NOTE received");
        if(e.IsText)
        {
            while(ProjectManagerScript.Instance.IsBusyCreatingNote())
            {
                ;
            }
            ProjectManagerScript.Instance.HandleNoteMessage(e.Data);
            while (ProjectManagerScript.Instance.IsBusyCreatingNote())
            {
                ;
            }
            string answer = ProjectManagerScript.Instance.GetCreateNoteAnswer();
            Send(answer);
        }
    }
}



public class WSFloorRequest : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log("FLOORS : " + e.Data);
        int nbFloors = FloorsManagerScript.Instance.FloorItemListPrefabScriptsList.Count;
        Debug.Log("NB floors to send : " + nbFloors);
        StringBuilder jsonStringBuilder = new StringBuilder();
        jsonStringBuilder.Append("[");
        for (int i = 0; i < nbFloors; i++)
        {
            FloorItemListPrefabScript f = FloorsManagerScript.Instance.FloorItemListPrefabScriptsList[i];
            jsonStringBuilder.Append("{" +
                "\"Name\":\"" + f.GetFloorStruct().Name + "\"," +
                "\"Id\":\"" + f.GetFloorStruct().Id + "\"," +
                "\"Image\":\"" + f.floorImageBase64String + "\"" +
                "}");
            if(i < nbFloors - 1)
            {
                jsonStringBuilder.Append(",");
            }
        }
        jsonStringBuilder.Append("]");
        Send(jsonStringBuilder.ToString());
    }
}



public class WSPing : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log("PING : " + e.Data);
        if(e.IsPing)
        {
            Send("Pong");
        }
    }
}



public class WebsocketServerScript : MonoBehaviour
{
    public int WebsocketPort = 7777;
    public bool IsStarted = false;
    public string SessionKey = "";

    private WebSocketServer wss;



    public void StartServer(string sessKey)
    {
        SessionKey = sessKey;
        wss = new WebSocketServer(WebsocketPort);
        wss.AddWebSocketService<WSNotesReceiver>("/" + SessionKey + "/note");
        wss.AddWebSocketService<WSFloorRequest>("/" + SessionKey + "/floors");
        wss.AddWebSocketService<WSPing>("/" + SessionKey);

        wss.Start();
        string urisString = "Websocket server listening on port " + WebsocketPort + " for message comming on uri(s) ";
        foreach(string s in wss.WebSocketServices.Paths)
        {
            urisString += "ws://" + getIPAddress() + ":" + WebsocketPort + s + ", ";
        }
        Debug.Log(urisString);
        IsStarted = true;
    }



    public bool IsServerStarted()
    {
        //return wss.IsListening;
        return IsStarted;
    }



    public void StopServer()
    {
        wss.Stop();
        Debug.Log("Websocket server stopped");
        IsStarted = false;
    }



    public string getIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
            }

        }
        return localIP;
    }
}

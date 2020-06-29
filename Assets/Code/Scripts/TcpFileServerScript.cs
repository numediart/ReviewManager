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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TcpFileServerScript : MonoBehaviour
{
    public int RcvPort = 7777;
    public bool isServerStarted = false;
/*
    private int BUFFER_SIZE = 50000;
    private System.Threading.Thread SocketThread;
    private volatile bool keepReading = false;
    private volatile bool isThreadToStop = false;

    private volatile Tuple<EndPoint, string>[] allReceivedTCPPackets = new Tuple<EndPoint, string>[255];
    private volatile Tuple<EndPoint, string>[] TCPPacketsToSend = new Tuple<EndPoint, string>[255];
    private volatile int incomingPullIndex = 0;
    private volatile int incomingPushIndex = 0;
    private volatile int outgoingPullIndex = 0;
    private volatile int outgoingPushIndex = 0;

    private ProjectManagerScript ProjectManagerScriptRef = null;

    private void Awake()
    {
    }

    void Update()
    {
        if(ProjectManagerScriptRef == null)
        {
            ProjectManagerScriptRef = GameObject.Find("ProjectManager").GetComponent<ProjectManagerScript>();
        }

        if (incomingPushIndex != incomingPullIndex)
        {
            TCPPacketsToSend[outgoingPushIndex] = ProjectManagerScriptRef.HandleTCPMessage(allReceivedTCPPackets[incomingPullIndex]);
            //Debug.Log("Data Sent to ProjectManager");

            if (incomingPullIndex == 254)
            {
                incomingPullIndex = 0;
            }
            else
            {
                ++incomingPullIndex;
            }

            if (outgoingPushIndex == 254)
            {
                outgoingPushIndex = 0;
            }
            else
            {
                ++outgoingPushIndex;
            }
        }
    }

    public void StartServer()
    {
        isServerStarted = true;
        isThreadToStop = false;
        SocketThread = new System.Threading.Thread(NetworkCode)
        {
            IsBackground = true
        };
        SocketThread.Start();
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


    private volatile Socket listener;
    private volatile Socket handler;

    void NetworkCode()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[BUFFER_SIZE];

        // host running the application.
        IPAddress[] ipArray = Dns.GetHostAddresses(getIPAddress());
        IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], RcvPort);

        // Create a TCP/IP socket.
        listener = new Socket(ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            Blocking = true
        };

        if (isThreadToStop)
        {
            listener.Dispose();
            listener.Close();
            return;
        }

        //Debug.Log("First Try");

        try
        {
            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            listener.Bind(localEndPoint);
            //Debug.Log("Listen to " + localEndPoint.ToString());
            listener.Listen(20); // max 20 pending connections in the queue

            if (isThreadToStop)
            {
                listener.Dispose();
                listener.Close();
                return;
            }

            // Start listening for connections.
            while (true)
            {
                if (isThreadToStop)
                {
                    listener.Dispose();
                    listener.Close();
                    return;
                }

                keepReading = true;

                // Program is suspended while waiting for an incoming connection.
                Debug.Log("Waiting for Connection");     //It works
                try
                {
                    handler = listener.Accept();

                    Debug.Log("Client Connected : " + handler.RemoteEndPoint);     //It doesn't work
                    string data = null;

                    // An incoming connection needs to be processed.
                    while (keepReading)
                    {
                        if (isThreadToStop)
                        {
                            handler.Dispose();
                            handler.Close();
                            listener.Dispose();
                            listener.Close();
                            return;
                        }
                        bytes = new byte[BUFFER_SIZE];
                        int bytesRec = handler.Receive(bytes);
                        //Debug.Log("Received from Client " + bytesRec);

                        if (bytesRec <= 0)
                        {
                            // all data has arrived
                            keepReading = false;
                            Tuple<EndPoint, string> tcpMessage = new Tuple<EndPoint, string>(handler.RemoteEndPoint, data);
                            handler.Disconnect(true);
                            Debug.Log("Disconnected!");
                            allReceivedTCPPackets[incomingPushIndex] = tcpMessage;

                            if (incomingPushIndex == 254)
                            {
                                incomingPushIndex = 0;
                            }
                            else
                            {
                                ++incomingPushIndex;
                            }

                            if (isThreadToStop)
                            {
                                listener.Dispose();
                                listener.Close();
                                return;
                            }
                            break;
                        }
                        else
                        {
                            // write incomming bytes to data field
                            data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        }

                        System.Threading.Thread.Sleep(1);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    if (isThreadToStop)
                    {
                        listener.Dispose();
                        listener.Close();
                        return;
                    }
                }

                System.Threading.Thread.Sleep(1);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void StopServer()
    {
        keepReading = false;

        isThreadToStop = true;

        if (handler != null && handler.Connected)
        {
            handler.Disconnect(false);
            Debug.Log("Disconnected!");
        }

        isServerStarted = false;
    }

    void OnDisable()
    {
        StopServer();
    }
    */
}

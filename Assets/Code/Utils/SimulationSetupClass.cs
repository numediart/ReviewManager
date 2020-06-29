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

﻿
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

public class SimulationSetupClass
{
    public string ProjectName;
    public string SessionKey;
    public int MaxUsedFloorId;
    public List<FloorStruct> FloorList = new List<FloorStruct>();

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(SimulationSetupClass));
        var encoding = Encoding.GetEncoding("UTF-8");

        using (var stream = new StreamWriter(path, false, encoding))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static SimulationSetupClass Load(string path)
    {
        var serializer = new XmlSerializer(typeof(SimulationSetupClass));
        var encoding = Encoding.GetEncoding("UTF-8");

        using (var stream = new StreamReader(path, encoding))
        {
            SimulationSetupClass ssc = serializer.Deserialize(stream) as SimulationSetupClass;
            ssc.SessionKey = ssc.SessionKey ?? RandomString.CreateRandomString(4).ToUpper();
            if (ssc.MaxUsedFloorId <= 0)
            {
                int maxUsedId = -1;
                foreach (FloorStruct info in ssc.FloorList)
                {
                    if (info.Id > maxUsedId)
                    {
                        maxUsedId = info.Id;
                    }
                }
                ssc.MaxUsedFloorId = maxUsedId;
            }
            return ssc;
        }
    }

    public bool IsFloorNameFree(string name)
    {
        foreach (FloorStruct info in FloorList)
        {
            if (info.Name == name)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsFloorIdFree(int id)
    {
        foreach (FloorStruct info in FloorList)
        {
            if (info.Id == id)
            {
                return false;
            }
        }

        return true;
    }

    public string GetFloorName(int id)
    {
        foreach (FloorStruct info in FloorList)
        {
            if (info.Id == id)
            {
                return info.Name;
            }
        }

        return "";
    }

    public int GetFloorIndex(string name)
    {
        int i = 0;
        foreach (FloorStruct info in FloorList)
        {
            if (info.Name == name)
            {
                return i;
            }
            i++;
        }

        return -1;
    }

    public int GetFloorIndex(int id)
    {
        int i = 0;
        foreach (FloorStruct info in FloorList)
        {
            if (info.Id == id)
            {
                return i;
            }
            i++;
        }

        return -1;
    }


    public int GetNextFreeFloorId()
    {
        return ++MaxUsedFloorId;
    }
}

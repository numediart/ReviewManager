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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

public class ListProjectClass
{
    [XmlElement("PreferedDirectory")]
    public string PreferedDirectory;
    [XmlArray("Projects"), XmlArrayItem("Project")]
    public List<string> Projects = new List<string>();

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(ListProjectClass));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static ListProjectClass Load(string path)
    {
        var serializer = new XmlSerializer(typeof(ListProjectClass));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as ListProjectClass;
        }
    }

    public int Exist(string path)
    {
        for(int index = 0;index <Projects.Count; ++ index)
        {
            if(Projects[index] == path)
            {
                return index;
            }
        }

        return -1;
    }
}

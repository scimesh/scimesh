using Kitware.VTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Scimesh.Third.Activiz.To
{
    public static class Activiz
    {
        /// <summary>
        /// FIXME Workaround of Activiz not reading xmlMultiBlockData MetaData
        /// </summary>
        public static readonly Func<string, vtkInformation[]> xmlMultiBlockMetaData = (relPath) =>
        {
            string absPath = Path.Combine(Application.dataPath, relPath);
            UnityEngine.Debug.Log("Reading " + absPath);
            List<vtkInformation> multiBlockMetaData = new List<vtkInformation>();
            using (XmlReader reader = XmlReader.Create(absPath))
            {
                while (reader.Read())
                {
                    // Only detect start elements.
                    if (reader.IsStartElement())
                    {    
                        // Get element name and switch on it.
                        switch (reader.Name)
                        {
                            case "VTKFile":
                                //UnityEngine.Debug.Log("Start <VTKFile> element.");
                                break;
                            case "vtkMultiBlockDataSet":
                                //UnityEngine.Debug.Log("Start <vtkMultiBlockDataSet> element.");
                                break;
                            case "DataSet":
                                //UnityEngine.Debug.Log("Start <DataSet> element.");
                                vtkInformation blockMetaData = vtkInformation.New();
                                // Search for the attribute name on this current node.
                                string attribute = reader["index"];
                                if (attribute != null)
                                {
                                    //UnityEngine.Debug.Log("  Has attribute name: " + attribute);
                                }
                                attribute = reader["name"];
                                if (attribute != null)
                                {
                                    blockMetaData.Set(vtkMultiBlockDataSet.NAME(), attribute);
                                    //UnityEngine.Debug.Log("  Has attribute name: " + attribute);
                                }
                                attribute = reader["file"];
                                if (attribute != null)
                                {
                                    //UnityEngine.Debug.Log("  Has attribute name: " + attribute);
                                }
                                multiBlockMetaData.Add(blockMetaData);
                                break;
                        }
                    }
                }
            }
            return multiBlockMetaData.ToArray();
        };
    }
}

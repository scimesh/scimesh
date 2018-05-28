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
        public static readonly Func<string, vtkInformation[]> readXmlMultiBlockMetaData = (relPath) =>
        {
            UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
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
                                break;
                            case "vtkMultiBlockDataSet":
                                break;
                            case "DataSet":
                                vtkInformation blockMetaData = vtkInformation.New();
                                // Search for the attribute name on this current node.
                                string index = reader["index"];  // FIXME Workaround No information key for index...
                                if (index != null)
                                {
                                    blockMetaData.Set(vtkCompositeDataSet.DATA_PIECE_NUMBER(), int.Parse(index));
                                }
                                string name = reader["name"];
                                if (name != null)
                                {
                                    blockMetaData.Set(vtkCompositeDataSet.NAME(), name);
                                }
                                string file = reader["file"];  // FIXME Workaround No information key for file path...
                                if (file != null)
                                {
                                    blockMetaData.Set(vtkCompositeDataSet.FIELD_NAME(), file);
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

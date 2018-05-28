using Kitware.VTK;
using System;
using System.Collections.Generic;

namespace Scimesh.Third.Activiz.To
{
    public static class Unity
    {
        /// <summary>
        /// Information keys names (vtkCompositeDataSet.DATA_PIECE_NUMBER() depend on 
        /// Scimesh.Third.Activiz.To.Activiz.readXmlMultiBlockMetaData function
        /// </summary>
        public static readonly Func<vtkInformation[], Dictionary<string, string>[]> vtkMultiBlockMetaDataToDicts = (infos) =>
        {
            List<Dictionary<string, string>> dicts = new List<Dictionary<string, string>>();
            foreach (vtkInformation info in infos)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                int index = info.Get(vtkCompositeDataSet.DATA_PIECE_NUMBER());
                dict.Add("index", index.ToString()); 
                string name = info.Get(vtkCompositeDataSet.NAME());
                dict.Add("name", name);
                string path = info.Get(vtkCompositeDataSet.FIELD_NAME());
                dict.Add("path", path);
                dicts.Add(dict);
            }
            return dicts.ToArray();
        };
    }
}

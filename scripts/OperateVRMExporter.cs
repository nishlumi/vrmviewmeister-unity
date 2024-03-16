using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniGLTF;
using UniVRM10;
using VRMShaders;


namespace UserHandleSpace
{


    public class OperateVRMExporter 
    {        
        public byte[] ExportSimple(GameObject model)
        {
            UniGLTF.GltfExportSettings sett = new UniGLTF.GltfExportSettings();
            sett.InverseAxis = Axes.Z;
            ITextureSerializer iser = new RuntimeTextureSerializer();

            return Vrm10Exporter.Export(model);
            
        }
    }
}
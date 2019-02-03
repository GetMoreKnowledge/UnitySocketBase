using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetsBudle
{

   

    [MenuItem("ITools/BuildAssetsBudle")]
    public static void BuildBudle()
    {
        string path = Application.dataPath+"/AssetsBudle";
        BuildPipeline.BuildAssetBundles(path,0,EditorUserBuildSettings.activeBuildTarget);


    }

   
}

using System;
using _Scripts.Scene_Stage.Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class StageDataDeleteHandler : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            StageData data = AssetDatabase.LoadAssetAtPath<StageData>(assetPath);
            
            if (data == null) return AssetDeleteResult.DidNotDelete;
            
            string listDataPath = "Assets/_SO/StageList.asset";
            StageListData listData = AssetDatabase.LoadAssetAtPath<StageListData>(listDataPath);

            if (listData == null) return AssetDeleteResult.DidNotDelete;
            
            listData.RemoveData(data);
            
            EditorUtility.SetDirty(listData);
            AssetDatabase.SaveAssets();
            
            Debug.LogWarning($"<color=yellow>[StageSystem]</color> \"{data.name}\" 파일 삭제, StageListData에서 제거됐습니다.");

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
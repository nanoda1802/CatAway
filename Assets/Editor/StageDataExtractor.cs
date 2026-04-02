using System;
using System.Collections.Generic;
using System.IO;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Data.Level;
using _Scripts.Scene_Stage.Enums;
using _Scripts.Scene_Stage.Table.Contactable;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

namespace Editor
{
    public class StageDataExtractor : EditorWindow
    {
        private int _stageId = 0;
        private StageMode _mode = StageMode.Coop;
        private string _description;
        private float _duration = 180f;
        private Transform _point0;
        private Transform _point1;
        private Transform _point2;
        private Transform _point3;
        private GameObject _tablesRoot;

        [MenuItem("Jobs/Stage Data Extractor")]
        public static void ShowExtractorWindow()
        {
            GetWindow<StageDataExtractor>("Stage Data Extractor");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("[ 스테이지 정보 입력 ]", EditorStyles.boldLabel);
            
            _stageId = EditorGUILayout.IntField("Id", _stageId);
            _mode = (StageMode) EditorGUILayout.EnumPopup("Mode", _mode);
            _description = EditorGUILayout.TextField("Description", _description);
            _duration = EditorGUILayout.FloatField("Duration", _duration);
            
            EditorGUILayout.Space();
            GUILayout.Label("[ 플레이어 스폰 위치 ]", EditorStyles.boldLabel);
            _point0 = (Transform)EditorGUILayout.ObjectField("Player0", _point0, typeof(Transform),true);
            _point1 = (Transform)EditorGUILayout.ObjectField("Player1", _point1, typeof(Transform),true);
            _point2 = (Transform)EditorGUILayout.ObjectField("Player2", _point2, typeof(Transform),true);
            _point3 = (Transform)EditorGUILayout.ObjectField("Player3", _point3, typeof(Transform),true);

            EditorGUILayout.Space();
            GUILayout.Label("[ 추출할 테이블 루트 오브젝트 ]", EditorStyles.boldLabel);
            _tablesRoot = (GameObject)EditorGUILayout.ObjectField("Tables Root", _tablesRoot, typeof(GameObject),true);
            
            EditorGUILayout.Space();
            if (GUILayout.Button("생성", GUILayout.Height(30)))
            {
                CreateStageData();
            }
        }

        private void CreateStageData()
        {
            if (!CheckTableRoot(out var tableInfos)) return;
            if (!CheckSpawnPoints(out var spawnPoints)) return;
            if (!CheckFilePath(out string fullPath)) return;
            if (!CheckStageList(out StageListData listData)) return;
            
            StageData stageSo = ScriptableObject
                .CreateInstance<StageData>()
                .Init(_stageId, _mode, _description, _duration, spawnPoints, tableInfos);
            
            listData.AddData(stageSo);
            
            AssetDatabase.CreateAsset(stageSo, fullPath);
            
            EditorUtility.SetDirty(listData);
            EditorUtility.SetDirty(stageSo);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.LogWarning($"<color=yellow>[Create StageData]</color> \"Stage_{_mode}_{_stageId}.asset\" 파일 생성, StageListData에 추가됐습니다.");
        }
        
        private bool CheckTableRoot(out List<TableSpawnInfo> tableInfos)
        {
            tableInfos = null;
            
            if (_tablesRoot == null)
            {
                EditorUtility.DisplayDialog("경고", "Hierarchy에서 Table들의 root인 게임오브젝트를 선택하세요.", "확인");
                return false;
            }

            if (_tablesRoot.transform.childCount <= 0)
            {
                EditorUtility.DisplayDialog("경고", "선택된 게임오브젝트은 자식 오브젝트를 갖지 않았습니다.", "확인");
                return false;
            }

            tableInfos = CollectTableInfos();
            
            return true;
        }
        
        private bool CheckSpawnPoints(out Vector3[] spawnPoints)
        {
            spawnPoints = null;
            
            if (_point0 == null || _point1 == null || _point2 == null || _point3 == null)
            {
                EditorUtility.DisplayDialog("경고", "설정되지 않은 SpawnPoint가 있습니다.", "확인");
                return false;
            }
            
            Vector3[] points = new Vector3[4];

            points[0] = _point0.position;
            points[1] = _point1.position;
            points[2] = _point2.position;
            points[3] = _point3.position;

            spawnPoints = points;
            
            return true;
        }

        private bool CheckFilePath(out string fullPath)
        {
            string directoryPath = "Assets/_SO/Stage";
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            string fileName = $"Stage_{_mode}_{_stageId}.asset";
            
            fullPath = Path.Combine(directoryPath, fileName);
            
            if (AssetDatabase.LoadAssetAtPath<StageData>(fullPath) != null)
            {
                EditorUtility.DisplayDialog("경고", "동일한 이름의 StageData 파일이 존재합니다.", "확인");
                return false;
            }

            return true;
        }

        private bool CheckStageList(out StageListData listData)
        {
            string listDataPath = "Assets/_SO/StageList.asset";
            listData = AssetDatabase.LoadAssetAtPath<StageListData>(listDataPath);

            if (listData == null)
            {
                EditorUtility.DisplayDialog("경고", "생성된 데이터를 등록할 StageList 파일을 찾지 못했습니다.", "확인");
                return false;
            }

            if (listData.IsDuplicate(_stageId, _mode))
            {
                EditorUtility.DisplayDialog("경고", "이미 StageList에 등록된 Id와 Mode입니다.", "확인");
                return false;
            }

            return true;
        }
        
        private List<TableSpawnInfo> CollectTableInfos()
        {
            List<TableSpawnInfo> tableInfos = new List<TableSpawnInfo>();

            foreach (Transform child in _tablesRoot.transform)
            {
                if (!child.CompareTag("Table")) continue;
                if (!child.gameObject.activeSelf) continue;
                
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(child.gameObject);
                
                if (prefabAsset == null) continue;
                if (!prefabAsset.TryGetComponent<NetworkObject>(out var netObj)) continue;
                
                // 이건 프리펩이 아니고 월드 오브젝트로 해야해
                bool isPantry = child.TryGetComponent<PantryTable_BackUp>(out var pantry);
                
                var info = new TableSpawnInfo
                {
                    GlobalObjectHashId = netObj.PrefabIdHash,
                    Position = child.position,
                    Rotation = child.rotation,
                    PantryType = isPantry ? pantry.PresetType : 0
                };
                
                tableInfos.Add(info);
            }
            
            return tableInfos;
        }

        
    }
}
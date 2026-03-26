using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Scene_Stage.Enums;
using UnityEngine;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;
using RO = _Scripts._Helper.InspectorReadOnlyAttribute;

namespace _Scripts.Scene_Stage.Data
{
    [CreateAssetMenu(menuName = "SO/Stage/StageList", fileName = "StageList")]
    public class StageListData : ScriptableObject, IInitializable, IDisposable
    {
        [RO, SF] private List<StageData> stages;
        
        private readonly Dictionary<StageMode, StageData[]> _dataDict = new();
        
    #if UNITY_EDITOR
        public bool IsDuplicate(int id, StageMode mode)
        {
            foreach (var data in stages)
            {
                if (data.Id == id && data.Mode == mode) return true;
            }
            
            return false;
        }

        public void AddData(StageData data)
        {
            stages.Add(data);
        }

        public void RemoveData(StageData data)
        {
            stages.Remove(data);
        }
    #endif
        
        public void Initialize()
        {
            _dataDict.Clear();

            if (stages == null || stages.Count == 0)
                throw new Exception("[StageListData.Initialize] list is null or empty!!");
            
            var coopStageCount = stages.Count(data => data.Mode == StageMode.Coop);
            var compStageCount = stages.Count - coopStageCount;
            
            _dataDict.Add(StageMode.Coop, new StageData[coopStageCount]);
            _dataDict.Add(StageMode.Comp, new StageData[compStageCount]);
            
            foreach (var data in stages)
                _dataDict[data.Mode][data.Id] = data;
        }

        public void Dispose()
        {
            _dataDict.Clear();
        }
        
        public StageData GetStageData(StageMode mode, int idx)
        {
            return _dataDict[mode][idx];
        }

        public int GetLeftIndex(StageMode mode, int centerIdx)
        {
            int len = _dataDict[mode].Length;
            return (centerIdx - 1 + len) % len;
        }

        public int GetRightIndex(StageMode mode, int centerIdx)
        {
            int len = _dataDict[mode].Length;
            return (centerIdx + 1) % len;
        }

        public (Sprite, Sprite, Sprite) GetThumbnails(StageMode mode, int centerIdx)
        {
            int len = _dataDict[mode].Length;
            if (centerIdx < 0 || centerIdx >= len) centerIdx = 0;
            
            int leftIdx = GetLeftIndex(mode, centerIdx);
            int rightIdx = GetRightIndex(mode, centerIdx);
            
            return (_dataDict[mode][leftIdx].Thumbnail, _dataDict[mode][centerIdx].Thumbnail, _dataDict[mode][rightIdx].Thumbnail);
        }
    }
}
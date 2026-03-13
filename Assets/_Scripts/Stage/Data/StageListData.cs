using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Data
{
    [CreateAssetMenu(menuName = "SO/Stage/StageList", fileName = "StageList")]
    public class StageListData : ScriptableObject, IInitializable, IDisposable
    {
        [SF] private StageData[] list;
        
        private readonly Dictionary<StageMode, StageData[]> _dataDict = new();
        
        
        public void Initialize()
        {
            _dataDict.Clear();

            if (list == null || list.Length == 0)
                throw new Exception("[StageListData.Initialize] list is null or empty!!");
            
            var coopStageCount = list.Count(data => data.Mode == StageMode.Coop);
            var compStageCount = list.Length - coopStageCount;
            
            _dataDict.Add(StageMode.Coop, new StageData[coopStageCount]);
            _dataDict.Add(StageMode.Comp, new StageData[compStageCount]);
            
            foreach (var data in list)
                _dataDict[data.Mode][data.Id] = data;
        }

        public void Dispose()
        {
            _dataDict.Clear();
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
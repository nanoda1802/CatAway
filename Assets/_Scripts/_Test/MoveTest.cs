using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts._Test
{
    public class MoveTest
    {
        private Vector3 _lastPos;

        private bool _recording;

        public MoveTest(Vector3 pos)
        {
            _lastPos = pos;
        }

        public void DetectMove(Transform tr)
        {
            bool changed = Vector3.Distance(_lastPos, tr.position) > 0.001f;

            if (changed)
            {
                _lastPos = tr.position;
                Debug.Log($"<b>[Transform Updating]</b> 현위치 : ({tr.position.x},{tr.position.z}) / 시간 : {DateTime.Now.ToString(("HH:mm:ss.ffff"))}");
            }
        }
        
        public void CancelRecordInput()
        {
            _recording = false;
        }

        public async UniTaskVoid RecordInput(Transform tr)
        {
            _recording = true;
            
            Debug.Log($"<color=blue>[Input Started]</color> 시작좌표 : ({tr.position.x},{tr.position.z}) / 시간 : {DateTime.Now.ToString(("HH:mm:ss.ffff"))}");
            
            while (_recording)
            {
                await UniTask.Yield();
            }
            
            Debug.Log($"<color=red>[Input Canceled]</color> 종료좌표 : ({tr.position.x},{tr.position.z}) / 시간 : {DateTime.Now.ToString(("HH:mm:ss.ffff"))}");        }
    }
}
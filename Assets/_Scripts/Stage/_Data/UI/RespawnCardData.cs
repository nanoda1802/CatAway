using PrimeTween;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage._Data.UI
{
    [CreateAssetMenu(fileName = "RespawnCardData", menuName = "SO/Stage/UI/RespawnCard")]
    public class RespawnCardData : ScriptableObject
    {
        [SF] private ShakeSettings scaleSettings;
        [SF] private ShakeSettings rotSettings;
        [SF] private string timerTextFormat = "{0}";
        [SF] private float respawnWaitTime = 3;
        [SF] private Vector3 offset = new Vector3(0, 1, 0);
        
        public ShakeSettings ScaleSettings => scaleSettings;
        public ShakeSettings RotSettings => rotSettings;
        public string TimerTextFormat => timerTextFormat;
        public float RespawnWaitTime => respawnWaitTime;
        public Vector3 Offset => offset;
    }
}
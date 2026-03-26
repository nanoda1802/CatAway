using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;

namespace _Scripts.Scene_Room.UI
{
    public abstract class SectionBase : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        protected readonly DisposableBagBuilder DisposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected virtual void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            DisposableBagBuilder?.Build().Dispose();
        }
        
        public abstract UniTask Show(CancellationToken token);
        public abstract UniTask Hide(CancellationToken token);
        
        protected CancellationToken RefreshToken()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }
}
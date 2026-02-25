using System;
using System.Collections.Generic;
using _Scripts.Stage.Table;
using _Scripts.Stage.UI.Widget;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Stage
{
    public class StageHub : IInitializable, IDisposable
    {
        private readonly Dictionary<Type, IPlacable> _placableDic = new();
        private readonly Dictionary<Type, IProvider> _providerDic = new();
        
        private readonly DisposableBagBuilder _disposableBag = DisposableBag.CreateBuilder();
        
        [Inject]
        private void Construct(
            ISubscriber<IPlacable> placableSub,
            ISubscriber<IProvider> providerSub,
            IBufferedPublisher<PublishRequestMessage> requestPub)
        {
            placableSub
                .Subscribe(table => _placableDic.TryAdd(table.GetType(), table))
                .AddTo(_disposableBag);
            
            providerSub
                .Subscribe(provider => _providerDic.TryAdd(provider.GetType(), provider))
                .AddTo(_disposableBag);
            
            requestPub.Publish(new PublishRequestMessage(typeof(IPlacable), typeof(IProvider)));
        }
        
        public IPlacable FetchPlacable<T>() where T : IPlacable
        {
            return _placableDic.GetValueOrDefault(typeof(T), null);
        }
        
        public T FetchProvider<T>() where T : MonoBehaviour, IProvider
        {
            return _providerDic.GetValueOrDefault(typeof(T), null) as T;
        }
        
        public void Initialize() { } // 그저 가장 먼저 Subscriber들을 열어두기 위한...

        public void Dispose()
        {
            _disposableBag.Build().Dispose();
            
            _placableDic.Clear();
            _providerDic.Clear();
        }
    }
}
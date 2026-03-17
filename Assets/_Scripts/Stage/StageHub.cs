using System;
using System.Collections.Generic;
using _Scripts.Stage.Data;
using _Scripts.Stage.Table;
using _Scripts.Stage.UI.Board.Order;
using _Scripts.Stage.UI.Board.Score;
using _Scripts.Stage.UI.Widget;
using MessagePipe;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Stage
{
    public class StageHub : IInitializable, IDisposable
    {
        private readonly IBufferedPublisher<PublishRequestMessage> _requestPub;
        
        private readonly Dictionary<Type, IPlacable> _placableDic = new();
        private readonly Dictionary<Type, IProvider> _providerDic = new();
        
        private readonly Dictionary<Team, ScorePresenter> _scorePresenterDic = new();
        private readonly Dictionary<Team, OrderPresenter> _orderPresenterDic = new();
        
        private readonly DisposableBagBuilder _disposableBag = DisposableBag.CreateBuilder();
        
        public StageHub(
            ISubscriber<IPlacable> placableSub,
            ISubscriber<IProvider> providerSub,
            ISubscriber<ScorePresenter> scorePresenterSub,
            ISubscriber<OrderPresenter> orderPresenterSub,
            IBufferedPublisher<PublishRequestMessage> requestPub)
        {
            placableSub
                .Subscribe(table => _placableDic.TryAdd(table.GetType(), table))
                .AddTo(_disposableBag);
            
            providerSub
                .Subscribe(provider => _providerDic.TryAdd(provider.GetType(), provider))
                .AddTo(_disposableBag);
            
            scorePresenterSub
                .Subscribe(presenter => _scorePresenterDic.TryAdd(presenter.Team, presenter))
                .AddTo(_disposableBag);
            
            orderPresenterSub
                .Subscribe(presenter => _orderPresenterDic.TryAdd(presenter.Team, presenter))
                .AddTo(_disposableBag);
            
            _requestPub = requestPub;
        }
        
        public IPlacable FetchPlacable<T>() where T : IPlacable
        {
            return _placableDic.GetValueOrDefault(typeof(T), null);
        }
        
        public T FetchProvider<T>() where T : MonoBehaviour, IProvider
        {
            return _providerDic.GetValueOrDefault(typeof(T), null) as T;
        }

        public ScorePresenter FetchScorePresenter(Team team)
        {
            return _scorePresenterDic.GetValueOrDefault(team, null);
        }

        public OrderPresenter FetchOrderPresenter(Team team)
        {
            return _orderPresenterDic.GetValueOrDefault(team, null);
        }

        public void Initialize()
        {
            var req = new PublishRequestMessage(
                typeof(IPlacable),
                typeof(IProvider),
                typeof(ScorePresenter),
                typeof(OrderPresenter));
            
            _requestPub.Publish(req);
        }

        public void Dispose()
        {
            _disposableBag.Build().Dispose();
            
            _placableDic.Clear();
            _providerDic.Clear();
            
            _scorePresenterDic.Clear();
            _orderPresenterDic.Clear();
        }
    }
}
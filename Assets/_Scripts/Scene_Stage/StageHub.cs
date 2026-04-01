using System;
using System.Collections.Generic;
using _Scripts.Scene_Stage.Enums;
using _Scripts.Scene_Stage.Table;
using _Scripts.Scene_Stage.Table.Placable;
using _Scripts.Scene_Stage.UI.Board.Order;
using _Scripts.Scene_Stage.UI.Board.Score;
using _Scripts.Scene_Stage.UI.Pop;
using _Scripts.Stage;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace _Scripts.Scene_Stage
{
    public class StageHub : IInitializable, IDisposable
    {
        private readonly IBufferedPublisher<HubCallMessage> _requestPub;
        
        private readonly Dictionary<Type, IPlacable> _placableDic = new();
        private readonly Dictionary<Type, IProvider> _providerDic = new();
        private readonly Dictionary<Team, PlateReturnTable> _plateReturnTableDic = new();
        
        private readonly Dictionary<Team, ScorePresenter> _scorePresenterDic = new();
        private readonly Dictionary<Team, OrderPresenter> _orderPresenterDic = new();
        private CuePresenter _cuePresenter;
        
        public StageHub(
            ISubscriber<IPlacable> placableSub,
            ISubscriber<IProvider> providerSub,
            ISubscriber<ScorePresenter> scorePresenterSub,
            ISubscriber<OrderPresenter> orderPresenterSub,
            ISubscriber<CuePresenter> cuePresenterSub,
            IBufferedPublisher<HubCallMessage> requestPub,
            DisposableBagBuilder disposableBagBuilder)
        {
            placableSub
                .Subscribe(table =>
                {
                    if (table is PlateReturnTable returnTable)
                    {
                        _plateReturnTableDic.TryAdd(returnTable.Team, returnTable);
                        return;
                    }

                    _placableDic.TryAdd(table.GetType(), table);
                })
                .AddTo(disposableBagBuilder);
            
            providerSub
                .Subscribe(provider => _providerDic.TryAdd(provider.GetType(), provider))
                .AddTo(disposableBagBuilder);
            
            scorePresenterSub
                .Subscribe(presenter => _scorePresenterDic.TryAdd(presenter.Team, presenter))
                .AddTo(disposableBagBuilder);
            
            orderPresenterSub
                .Subscribe(presenter => _orderPresenterDic.TryAdd(presenter.Team, presenter))
                .AddTo(disposableBagBuilder);
            
            cuePresenterSub
                .Subscribe(presenter => _cuePresenter = presenter)
                .AddTo(disposableBagBuilder);
            
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

        public PlateReturnTable FetchPlateReturnTable(Team team)
        {
            return _plateReturnTableDic.GetValueOrDefault(team, null);
        }

        public ScorePresenter FetchScorePresenter(Team team)
        {
            return _scorePresenterDic.GetValueOrDefault(team, null);
        }

        public OrderPresenter FetchOrderPresenter(Team team)
        {
            return _orderPresenterDic.GetValueOrDefault(team, null);
        }

        public CuePresenter FetchCuePresenter()
        {
            return _cuePresenter;
        }

        public void Initialize()
        {
            var req = new HubCallMessage(
                typeof(IPlacable),
                typeof(IProvider),
                typeof(ScorePresenter),
                typeof(OrderPresenter),
                typeof(CuePresenter));
            
            _requestPub.Publish(req);
        }

        public void Dispose()
        {
            _placableDic.Clear();
            _providerDic.Clear();
            
            _scorePresenterDic.Clear();
            _orderPresenterDic.Clear();
        }
    }
}
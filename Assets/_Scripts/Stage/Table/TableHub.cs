using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Stage.Table
{
    public class TableHub : IInitializable
    {
        private readonly Dictionary<Type, IPlacable> _tableDic = new();
        
        [Inject]
        private void Construct(
            IBufferedSubscriber<PlateRackTable> plateRackSub,
            IBufferedSubscriber<PlateReturnTable> plateReturnSub)
        {
            plateRackSub.Subscribe(table =>
            {
                _tableDic.Add(typeof(PlateRackTable), table);
                Debug.Log($"[{table is not null}] TableHub에 PlateRackTable 주입");
            });

            plateReturnSub.Subscribe(table =>
            {
                _tableDic.Add(typeof(PlateReturnTable), table);
                Debug.Log($"[{table is not null}] TableHub에 PlateReturnTable 주입");
            });
        }
        
        public void Initialize()
        {
            // 그저 가장 먼저 Subscriber들을 열어두기 위한...
        }

        public IPlacable Fetch<T>() where T : IPlacable
        {
            return _tableDic.GetValueOrDefault(typeof(T), null);
        }
    }
}
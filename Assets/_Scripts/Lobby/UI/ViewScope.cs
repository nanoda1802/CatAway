using System;
using _Scripts.Lobby.UI.Room;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI
{
    public class ViewScope : LifetimeScope
    {
        [Header("[ Data ]")]
        [SF] private RoomViewUiData roomViewUiData;
        [SF] private MemberCard memberCardPrefab;
        
        [Header("[ Elements ]")]
        [SF] private TitleView titleView;
        [SF] private RoomView roomView;
        [SF] private QuickMenuBar quickMenuBar;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            builder.RegisterEntryPoint<ViewChanger>(Lifetime.Singleton).AsSelf();

            builder.Register<MemberCardProvider>(Lifetime.Scoped);
            
            builder.RegisterInstance(roomViewUiData);
            builder.RegisterInstance(memberCardPrefab);
            
            builder.RegisterComponent<IView>(titleView);
            builder.RegisterComponent<IView>(roomView);
            builder.RegisterComponent(quickMenuBar);

            builder.RegisterComponent(roomView.GetComponent<RectTransform>());
        }
    }
}
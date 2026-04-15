using _Scripts.Result._Data;
using _Scripts.Result._Messages;
using _Scripts.Result.UI;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI;
using _Scripts.Shared.UI.QuickMenu.ButtonActions;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Result
{
    public class ResultScope : LifetimeScope
    {
        [Header("[ Components ]")]
        [SF] private RectTransform viewRectTr;
        [Header("[ Data ]")]   
        [SF] private ResultMember memberPrefab;
        [SF] private ResultViewData viewData;
        [SF] private ResultMemberCard resultMemberCardPrefab;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ResultInitiator>();
            builder.Register<ResultMemberCardProvider>(Lifetime.Singleton);
            
            builder.UseComponents(RegisterComponents);

            RegisterQuickMenuActions(builder);
            RegisterMessages(builder);

            builder.RegisterInstance(memberPrefab);
            builder.RegisterInstance(resultMemberCardPrefab);
            
            builder.RegisterInstance(_disposableBagBuilder);
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }

        private void RegisterQuickMenuActions(IContainerBuilder builder)
        {
            builder.Register<IButtonAction<QuickMenuButtonType>, CustomizeAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SkipAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, LeaveAction>(Lifetime.Scoped);
        }

        private void RegisterComponents(ComponentsBuilder builder)
        {
            builder.AddInstance(viewRectTr);
            // builder.AddInstance(memberPrefab);
            builder.AddInstance(viewData);
            // builder.AddInstance(resultMemberCardPrefab);
        }

        private void RegisterMessages(IContainerBuilder builder)
        {
            var msgOptions = Parent.Container.Resolve<MessagePipeOptions>();
            builder.RegisterMessageBroker<StartResultMessage>(msgOptions);
            
            builder.RegisterMessageBroker<float>(msgOptions); // Timer
            builder.RegisterMessageBroker<ResultBoardMessage>(msgOptions);
            builder.RegisterMessageBroker<SkipRequest>(msgOptions);
            builder.RegisterMessageBroker<SkipRespond>(msgOptions);
            
            builder.RegisterMessageBroker<ShowResultMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<HideMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<MoveMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<UpdateMemberNameMessage>(msgOptions);
        }
    }
}
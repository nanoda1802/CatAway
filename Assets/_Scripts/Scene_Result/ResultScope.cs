using _Scripts._Messages.Room;
using _Scripts._Messages.Shared;
using _Scripts._Shared.Enums;
using _Scripts._Shared.UI;
using _Scripts._Shared.UI.QuickMenu.ButtonActions;
using _Scripts.Messages.Room;
using _Scripts.Messages.StageResult;
using _Scripts.Scene_Result.Data;
using _Scripts.Scene_Result.UI;
using _Scripts.Scene_Room;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Result
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
            builder.Register<IButtonAction<QuickMenuButtonType>, CustomizeAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SkipAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, LeaveAction>(Lifetime.Scoped);

            builder.Register<ResultMemberCardProvider>(Lifetime.Singleton);
            
            builder.RegisterInstance(_disposableBagBuilder);

            builder.UseEntryPoints(pointsBuilder =>
            {
                pointsBuilder.Add<ResultInitiator>();
            });
            
            builder.UseComponents(componentsBuilder =>
            {
                componentsBuilder.AddInstance(viewRectTr);
                componentsBuilder.AddInstance(memberPrefab);
                componentsBuilder.AddInstance(viewData);
                componentsBuilder.AddInstance(resultMemberCardPrefab);
            });
            
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
            
            
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }
    }
}
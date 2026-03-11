using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby.UI.Pop
{
    public class PopUpScope : LifetimeScope
    {
        [SF] private CanvasGroup canvasGroup;
        [SF] private PopPanel popPanel;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(canvasGroup);
            builder.RegisterComponent(popPanel);
        }
    }
}
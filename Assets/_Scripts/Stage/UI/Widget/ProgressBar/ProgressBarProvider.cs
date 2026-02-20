using System;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace _Scripts.Stage.UI.Widget.ProgressBar
{
    public class ProgressBarProvider : WidgetProvider<ProgressBarWidget>
    {
        private IBufferedPublisher<ProgressBarProvider> _pub;
        
        [Inject]
        private void Construct(IBufferedPublisher<ProgressBarProvider> pub)
        {
            pub.Publish(this);
        }
    }
}
using System;
using _Scripts.Shared.UI.Pop;

namespace _Scripts.Shared._Messages
{
    public readonly struct PopUpMessage
    {
        private readonly Type _requestType;
        
        public PopUpMessage(Type requestType)
        {
            _requestType = requestType;
        }

        public bool IsRequested(PopBase pop)
        {
            if (_requestType is null || pop is null) return false;
            return _requestType == pop.GetType();
        }
    }
}
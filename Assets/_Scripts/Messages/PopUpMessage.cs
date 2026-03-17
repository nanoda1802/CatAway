using System;
using _Scripts.Lobby.UI.Pop;

namespace _Scripts.Messages
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
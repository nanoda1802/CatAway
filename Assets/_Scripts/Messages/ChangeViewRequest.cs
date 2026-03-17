using System;
using _Scripts.Lobby.UI;

namespace _Scripts.Messages
{
    public readonly struct ChangeViewRequest
    {
        public Type ViewType { get; }

        public ChangeViewRequest(Type viewType)
        {
            if (viewType == null || viewType.IsAssignableFrom(typeof(IView)))
                throw new Exception("[ChangeViewRequest] RequestType must implement IView.");
            
            ViewType = viewType;
        }
    }
}
using System;

namespace _Scripts.Stage
{
    public readonly struct HubCallMessage
    {
        private readonly Type[] _requestTypes;
        private bool IsValidMessage => _requestTypes is { Length: > 0 };
        
        public HubCallMessage(params Type[] requestTypes)
        {
            _requestTypes = requestTypes;
        }

        public bool IsRequest(object obj)
        {
            if (!IsValidMessage) return false;
            
            foreach (var type in _requestTypes)
            {
                if (type.IsInstanceOfType(obj)) return true;
            }
            
            return false;
        }
    }
}
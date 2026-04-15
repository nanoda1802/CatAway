using System;

namespace _Scripts.Shared.UI
{
    public interface IButtonAction<T> where T : Enum
    {
        T ButtonType { get; }
        public void OnClick();
    }
}
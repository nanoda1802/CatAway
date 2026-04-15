using System.Collections.Generic;
using System.Threading;
using _Scripts.Shared._Enums;

namespace _Scripts.Shared._Messages
{
    public readonly struct DialogMessage
    {
        private readonly DialogButtonType _activeBtnTypes;

        public string Header { get; }
        public string Text { get; }
        public string InputPlaceholder { get; }
        public CancellationTokenSource Cts { get; }

        public bool NeedCancellation => Cts != null;
        public bool HasText => !string.IsNullOrEmpty(Text);
        public bool ShowInputField => !string.IsNullOrEmpty(InputPlaceholder);
        public List<DialogButtonType> ActiveBtnTypes
        {
            get
            {
                List<DialogButtonType> btnTypeList = new List<DialogButtonType>();
            
                uint uBtnTypes = (uint)_activeBtnTypes;

                while (uBtnTypes > 0)
                {
                    uint lsb = uBtnTypes & (uint)-(int)uBtnTypes;

                    DialogButtonType curType = (DialogButtonType) lsb;

                    btnTypeList.Add(curType);

                    uBtnTypes ^= lsb; 
                }

                return btnTypeList;
            }
        }

        public DialogMessage(
            string header,
            string text,
            string inputPlaceholder,
            DialogButtonType activeBtnTypes,
            CancellationTokenSource cts = null)
        {
            Header = header;
            Text = text;
            InputPlaceholder = inputPlaceholder;
            Cts = cts;
            _activeBtnTypes = activeBtnTypes;
        }
    }
}
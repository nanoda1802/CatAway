using System.Collections.Generic;
using _Scripts.Shared._Enums;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Shared.UI.QuickMenu
{
    public class QuickMenu : MonoBehaviour
    {
        [SF] private SerializedDictionary<QuickMenuButtonType, Button> buttons;
        
        [Inject]
        private void Construct(IReadOnlyList<IButtonAction<QuickMenuButtonType>> buttonActions)
        {
            foreach (var action in buttonActions)
            {
                if (!buttons.TryGetValue(action.ButtonType, out var btn)) continue;
                
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action.OnClick);
                
                btn.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            foreach (var btn in buttons.Values)
            {
                btn.onClick.RemoveAllListeners();
                btn.gameObject.SetActive(false);
            }
        }
    }
}
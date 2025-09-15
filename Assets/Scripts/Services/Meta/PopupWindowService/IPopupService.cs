using System;
using Cysharp.Threading.Tasks;

namespace Game.Services.Meta
{
    public interface IPopupService
    {
        UniTask ShowPopup(string txt, string title = null, Action action = null, string buttonText = null, PopupState state = PopupState.Information);
    }
} 
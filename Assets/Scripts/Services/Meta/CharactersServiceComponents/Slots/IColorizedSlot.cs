using UnityEngine;

namespace Game.Services.Meta
{
    public interface IColorizedSlot
    {
        bool SetColor(Color color);
        Color GetCurrentColor();
    }
} 
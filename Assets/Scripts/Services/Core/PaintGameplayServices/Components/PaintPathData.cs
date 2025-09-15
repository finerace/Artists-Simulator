using UnityEngine;

namespace Game.Services.Core
{
    [CreateAssetMenu(fileName = "PaintPathData", menuName = "Create PaintPathData", order = 1)]
    public class PaintPathData : ScriptableObject
    {
        [SerializeField] private string prefabPath;
        
        [Space]
        
        [SerializeField] private int difficulty;
        [SerializeField] private bool is45DegreeAllowed;
        [SerializeField] private bool isXFlipAllowed;
        [SerializeField] private bool isYFlipAllowed;
        
        public string PrefabPath => prefabPath;
        
        public int Difficulty => difficulty;
        public bool Is45DegreeAllowed => is45DegreeAllowed;
        public bool IsXFlipAllowed => isXFlipAllowed;
        public bool IsYFlipAllowed => isYFlipAllowed;
        
    }
}
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Additional.MagicAttributes;


public class ButtonAnimation : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField] private RectTransform buttonT;

    [Header("Scale Multipliers")]
    
    [SerializeField] private float onEnterScaleMultiplier = 1.05f;
    [SerializeField] private float onDownScaleMultiplier = 0.95f;
    private Vector3 defaultScale;
    
    [Header("Animations Time")]

    [SerializeField] private float onEnterTime = 0.25f;
    [SerializeField] private float onExitTime = 0.25f;
    
    [Space]
    
    [SerializeField] private float onDownTime = 0.25f;
    [SerializeField] private float onUpTime = 0.25f;

    [Header("Color settings")] 
    
    [SerializeField] private Image colorImage;
    
    [Space]
    
    [SerializeField] private Color defaultColor = Color.clear;
    [SerializeField] private Color onEnterColor = Color.white;
    [SerializeField] private Color onDownColor = Color.white;

    [Header("Positions Modifiers")]
    
    [SerializeField] private Vector3 onEnterPosModifier;
    private Vector3 defaultPos;
    
    [Header("Easing")] 
    
    [SerializeField] private Ease onEnterEase = Ease.InQuad;
    [SerializeField] private Ease onExitEase = Ease.InQuad;
    
    [SerializeField] private Ease onUpEase = Ease.InQuad;
    [SerializeField] private Ease onDownEase = Ease.InQuad;
    
    private bool isDown;
    
    private void Awake()
    {
        defaultScale = buttonT.localScale;
        defaultPos = buttonT.anchoredPosition;
        
        colorImage.color = defaultColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isDown)
            return;

        buttonT.DOScale(defaultScale * onEnterScaleMultiplier,onEnterTime)
            .From(defaultScale)
            .SetEase(onEnterEase);

        colorImage.DOColor(onEnterColor, onEnterTime)
            .SetEase(onEnterEase);

        if (onEnterPosModifier != Vector3.zero)
            buttonT.DOAnchorPos(defaultPos + onEnterPosModifier, onEnterTime)
                .SetEase(onEnterEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(isDown)
            return;
        
        buttonT.DOScale(defaultScale,onExitTime)
            .From(defaultScale)
            .SetEase(onExitEase);

        colorImage.DOColor(defaultColor, onExitTime)
            .SetEase(onExitEase);
        
        SetPosToDefault();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        
        buttonT.DOScale(defaultScale * onDownScaleMultiplier,onDownTime)
            .From(defaultScale)
            .SetEase(onDownEase);

        colorImage.DOColor(onDownColor, onDownTime)
            .SetEase(onDownEase);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        
        buttonT.DOScale(defaultScale,onUpTime)
            .From(buttonT.localScale)
            .SetEase(onUpEase);

        colorImage.DOColor(defaultColor, onUpTime)
            .SetEase(onUpEase);
        
        SetPosToDefault();
    }

    private void SetPosToDefault()
    {
        if (onEnterPosModifier != Vector3.zero)
            buttonT.DOAnchorPos(defaultPos, onExitTime)
                .SetEase(onExitEase);
    }
    
}

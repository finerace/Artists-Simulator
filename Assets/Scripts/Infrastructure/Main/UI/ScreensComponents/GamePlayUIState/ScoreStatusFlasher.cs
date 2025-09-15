using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Services.Core;
using Game.Services.Common;
using Game.Additional.MagicAttributes;


public class ScoreStatusFlasher : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusBg;

    [Header("Services")] 
    [SerializeField] private PseudoEnemyService pseudoEnemyService;
    [SerializeField] private LocalizationService localizationService;
    
    [Header("Localization Keys")] 
    [SerializeField] private string winKey = "SCORE_STATUS_WIN";
    [SerializeField] private string loseKey = "SCORE_STATUS_LOSE";
    [SerializeField] private string drawKey = "SCORE_STATUS_DRAW";

    [Header("Player Score Source")] 
    [SerializeField] private Func<float> getPlayerScore;

    [Header("Win State")] 
    [SerializeField] private Color winColor = Color.green;
    [SerializeField] private float winScale = 1.3f;
    [SerializeField] private float winShakeStrength = 30f;
    [SerializeField] private float winShakeDuration = 0.5f;
    [SerializeField] private float winRotateAngle = 10f;
    [Header("Lose State")] 
    [SerializeField] private Color loseColor = Color.red;
    [SerializeField] private float loseScale = 0.8f;
    [SerializeField] private float loseBounceHeight = 30f;
    [SerializeField] private float loseBounceDuration = 0.5f;

    [Header("Draw State")] 
    [SerializeField] private Color drawColor = Color.yellow;
    [SerializeField] private float drawScale = 1.0f;
    [SerializeField] private float drawBulgeStrength = 0.05f;
    [SerializeField] private float drawBulgeDuration = 1.2f;

    [Header("Common")] 
    [SerializeField] private float updateInterval = 0.2f;

    private float lastPlayerScore;
    private float lastEnemyScore;
    private Tween currentTween;
    private enum State { Win, Lose, Draw }
    private State lastState;

    private void Start()
    {
        UpdateStatus(true);
        InvokeRepeating(nameof(UpdateStatus), updateInterval, updateInterval);
        if (pseudoEnemyService != null)
            pseudoEnemyService.OnEnemyScoreChanged += _ => UpdateStatus();
    }

    private void OnDestroy()
    {
        if (pseudoEnemyService != null)
            pseudoEnemyService.OnEnemyScoreChanged -= _ => UpdateStatus();
        currentTween?.Kill();
    }

    private void UpdateStatus() => UpdateStatus(false);

    private void UpdateStatus(bool force = false)
    {
        float playerScore = getPlayerScore != null ? getPlayerScore() : 0f;
        float enemyScore = pseudoEnemyService != null ? pseudoEnemyService.EnemyScore : 0f;
        if (!force && Mathf.Approximately(playerScore, lastPlayerScore) && Mathf.Approximately(enemyScore, lastEnemyScore))
            return;
        lastPlayerScore = playerScore;
        lastEnemyScore = enemyScore;
        State state = State.Draw;
        if (playerScore > enemyScore) state = State.Win;
        else if (playerScore < enemyScore) state = State.Lose;
        if (state != lastState || force)
        {
            ApplyState(state);
            lastState = state;
        }
    }

    void ApplyState(State state)
    {
        currentTween?.Kill();
        switch (state)
        {
            case State.Win:
                statusText.text = localizationService != null ? localizationService.GetText(winKey) : "WIN";
                statusText.color = winColor;
                if (statusBg != null) statusBg.color = winColor;
                AnimateWin();
                break;
            case State.Lose:
                statusText.text = localizationService != null ? localizationService.GetText(loseKey) : "LOSE";
                statusText.color = loseColor;
                if (statusBg != null) statusBg.color = loseColor;
                AnimateLose();
                break;
            case State.Draw:
                statusText.text = localizationService != null ? localizationService.GetText(drawKey) : "DRAW";
                statusText.color = drawColor;
                if (statusBg != null) statusBg.color = drawColor;
                AnimateDraw();
                break;
        }
    }

    void AnimateWin()
    {
        var seq = DOTween.Sequence();
        seq.Append(statusText.rectTransform.DOShakeRotation(winShakeDuration, new Vector3(0, 0, winRotateAngle)))
           .Join(statusText.rectTransform.DOShakeScale(winShakeDuration, winShakeStrength))
           .Join(statusText.rectTransform.DOScale(winScale, winShakeDuration / 2).SetLoops(2, LoopType.Yoyo));
        currentTween = seq.SetLoops(-1);
    }

    void AnimateLose()
    {
        var seq = DOTween.Sequence();
        seq.Append(statusText.rectTransform.DOScale(loseScale, loseBounceDuration / 2).SetLoops(2, LoopType.Yoyo))
           .Join(statusText.rectTransform.DOAnchorPosY(-loseBounceHeight, loseBounceDuration / 2).SetLoops(2, LoopType.Yoyo));
        currentTween = seq.SetLoops(-1);
    }

    void AnimateDraw()
    {
        var seq = DOTween.Sequence();
        seq.Append(statusText.rectTransform.DOScale(drawScale + drawBulgeStrength, drawBulgeDuration / 2).SetLoops(2, LoopType.Yoyo));
        currentTween = seq.SetLoops(-1);
    }
} 
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using System.Threading;

public class AlertManager : Singleton<AlertManager>
{
    [SerializeField] private TextMeshProUGUI AlertText;
    private Sequence _currentAnimation;
    private CancellationTokenSource _hideCancellationTokenSource;
    private CancellationTokenSource _showCancellationTokenSource;
    private float _displayDuration = 2f;

    private void OnDestroy()
    {
        ClearPreviousAlert();
    }

    public void ShowAlert(string alertText, float duration = -1)
    {
        if (AlertText == null)
        {
            Debug.LogError("AlertText is not assigned!");
            return;
        }

        ClearPreviousAlert();

        AlertText.text = alertText;
        AlertText.gameObject.SetActive(true);

        PlayShowAnimation(duration >= 0 ? duration : _displayDuration).Forget();
    }

    private async UniTaskVoid PlayShowAnimation(float duration)
    {
        try
        {
            _showCancellationTokenSource = new CancellationTokenSource();

            // 현재 애니메이션 초기화
            _currentAnimation = DOTween.Sequence()
                .Append(AlertText.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack))
                .SetAutoKill(false);

            var animationTask = _currentAnimation.AsyncWaitForCompletion().AsUniTask();
            var cancellationTask = UniTask.WaitUntilCanceled(_showCancellationTokenSource.Token);

            await UniTask.WhenAny(animationTask, cancellationTask);

            if (!_showCancellationTokenSource.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration),
                    cancellationToken: (_hideCancellationTokenSource = new CancellationTokenSource()).Token);
                await PlayHideAnimation();
            }
        }
        catch (OperationCanceledException)
        {
            // 정상적인 취소 처리
        }
        catch (Exception ex)
        {
            Debug.LogError($"Alert animation error: {ex}");
        }
    }

    private async UniTask PlayHideAnimation()
    {
        try
        {
            _currentAnimation = DOTween.Sequence()
                .Append(AlertText.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack))
                .OnComplete(() => AlertText.gameObject.SetActive(false))
                .SetAutoKill(false);

            await _currentAnimation.AsyncWaitForCompletion();
        }
        finally
        {
            AlertText.gameObject.SetActive(false);
        }
    }

    private void ClearPreviousAlert()
    {
        _showCancellationTokenSource?.Cancel();
        _hideCancellationTokenSource?.Cancel();

        if (_currentAnimation != null && _currentAnimation.IsActive())
        {
            _currentAnimation.Kill(true);
        }

        _showCancellationTokenSource?.Dispose();
        _hideCancellationTokenSource?.Dispose();

        _showCancellationTokenSource = null;
        _hideCancellationTokenSource = null;
        _currentAnimation = null;
    }

    public void HideAlert()
    {
        ClearPreviousAlert();
        if (AlertText != null)
        {
            AlertText.gameObject.SetActive(false);
        }
    }
}
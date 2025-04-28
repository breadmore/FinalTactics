using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using System.Threading;

public class AlertManager : Singleton<AlertManager>
{
    public TextMeshProUGUI AlertText;
    private Sequence _currentAnimation;
    private CancellationTokenSource _hideCancellationTokenSource;
    private CancellationTokenSource _showCancellationTokenSource;
    private float _displayDuration = 2f; // 기본 표시 시간

    public void ShowAlert(string alertText, float duration = -1)
    {
        // 같은 텍스트면 애니메이션만 재시작
        if (AlertText.text == alertText && AlertText.gameObject.activeSelf)
        {
            HideAlert();
            return;
        }

        // 등장 중인 알림 즉시 정리
        ClearPreviousAlert();

        // 새 알림 설정
        AlertText.text = alertText;
        AlertText.gameObject.SetActive(true);

        // 등장 애니메이션 시작
        PlayShowAnimation(duration >= 0 ? duration : _displayDuration).Forget();
    }

    private async UniTaskVoid PlayShowAnimation(float duration)
    {
        _showCancellationTokenSource = new CancellationTokenSource();

        try
        {
            _currentAnimation = DOTween.Sequence();

            _currentAnimation.Append(
                AlertText.transform.DOScale(1.5f, 0.5f)
                    .SetEase(Ease.OutBack)
            );

            // 기다리면서 외부 취소를 직접 감시한다
            await UniTask.WhenAny(
                _currentAnimation.AsyncWaitForCompletion().AsUniTask(),
                UniTask.WaitUntilCanceled(_showCancellationTokenSource.Token)
            );

            if (_showCancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            _hideCancellationTokenSource = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _hideCancellationTokenSource.Token);

            await PlayHideAnimation();
        }
        catch (OperationCanceledException)
        {
            // 등장 애니메이션이나 대기 중 취소됐을 때 처리
        }
    }


    private async UniTask PlayHideAnimation()
    {
        _currentAnimation = DOTween.Sequence();

        _currentAnimation.Append(
            AlertText.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
        );

        await _currentAnimation.AsyncWaitForCompletion();

        AlertText.gameObject.SetActive(false);
    }

    private void RestartAnimation()
    {
        ClearPreviousAlert();
        PlayShowAnimation(_displayDuration).Forget();
    }

    private void ClearPreviousAlert()
    {
        // 진행 중인 등장 애니메이션 정리
        if (_currentAnimation != null && _currentAnimation.IsActive())
        {
            _currentAnimation.Kill();
        }

        // 등장 애니메이션 취소
        if (_showCancellationTokenSource != null)
        {
            _showCancellationTokenSource.Cancel();
            _showCancellationTokenSource.Dispose();
            _showCancellationTokenSource = null;
        }

        // 대기 딜레이 취소
        if (_hideCancellationTokenSource != null)
        {
            _hideCancellationTokenSource.Cancel();
            _hideCancellationTokenSource.Dispose();
            _hideCancellationTokenSource = null;
        }
    }

    // 수동으로 알림 숨기기
    public void HideAlert()
    {
        ClearPreviousAlert();
        AlertText.gameObject.SetActive(false);
    }
}

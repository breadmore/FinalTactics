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
    private float _displayDuration = 2f; // �⺻ ǥ�� �ð�

    public void ShowAlert(string alertText, float duration = -1)
    {
        // ���� �ؽ�Ʈ�� �ִϸ��̼Ǹ� �����
        if (AlertText.text == alertText && AlertText.gameObject.activeSelf)
        {
            HideAlert();
            return;
        }

        // ���� ���� �˸� ��� ����
        ClearPreviousAlert();

        // �� �˸� ����
        AlertText.text = alertText;
        AlertText.gameObject.SetActive(true);

        // ���� �ִϸ��̼� ����
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

            // ��ٸ��鼭 �ܺ� ��Ҹ� ���� �����Ѵ�
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
            // ���� �ִϸ��̼��̳� ��� �� ��ҵ��� �� ó��
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
        // ���� ���� ���� �ִϸ��̼� ����
        if (_currentAnimation != null && _currentAnimation.IsActive())
        {
            _currentAnimation.Kill();
        }

        // ���� �ִϸ��̼� ���
        if (_showCancellationTokenSource != null)
        {
            _showCancellationTokenSource.Cancel();
            _showCancellationTokenSource.Dispose();
            _showCancellationTokenSource = null;
        }

        // ��� ������ ���
        if (_hideCancellationTokenSource != null)
        {
            _hideCancellationTokenSource.Cancel();
            _hideCancellationTokenSource.Dispose();
            _hideCancellationTokenSource = null;
        }
    }

    // �������� �˸� �����
    public void HideAlert()
    {
        ClearPreviousAlert();
        AlertText.gameObject.SetActive(false);
    }
}

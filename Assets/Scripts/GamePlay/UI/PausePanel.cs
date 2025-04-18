using UnityEngine;

public class PausePanel : BaseAnimatedPanel
{
    public void OnPause()
    {
        Show();
        Time.timeScale = 0f;
    }

    public void OnResume()
    {
        Hide();
        Time.timeScale = 1f;
    }
}

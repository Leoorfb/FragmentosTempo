using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI instance;

    [SerializeField] GameObject endGameScreen;
    [SerializeField] GameObject gameoverText;
    [SerializeField] GameObject winText;
    [SerializeField] GameObject tryAgainButton;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void OnTryAgainButtonClick()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        LoadingScreenManager.Instance.SwitchToScene(1);
    }

    public void OnMainMenuButtonClick()
    {
        Time.timeScale = 1f;
        LoadingScreenManager.Instance.SwitchToScene(0);
    }

    public void GameOverScreen()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;

        endGameScreen.SetActive(true);
        gameoverText.SetActive(true);
        winText.SetActive(false);
        tryAgainButton.SetActive(true);
    }

    public void WinScreen()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;

        endGameScreen.SetActive(true);
        gameoverText.SetActive(false);
        winText.SetActive(true);
        tryAgainButton.SetActive(false);
    }
}

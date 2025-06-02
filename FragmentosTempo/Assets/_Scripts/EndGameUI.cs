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
        LoadingScreenManager.Instance.SwitchToScene(1);
    }

    public void OnMainMenuButtonClick()
    {
        LoadingScreenManager.Instance.SwitchToScene(0);
    }

    public void GameOverScreen()
    {
        endGameScreen.SetActive(true);
        gameoverText.SetActive(true);
        winText.SetActive(false);
        tryAgainButton.SetActive(true);
    }

    public void WinScreen()
    {
        endGameScreen.SetActive(true);
        gameoverText.SetActive(false);
        winText.SetActive(true);
        tryAgainButton.SetActive(false);
    }
}

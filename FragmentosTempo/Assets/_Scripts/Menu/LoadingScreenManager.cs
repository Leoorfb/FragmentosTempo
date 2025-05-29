using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;
    public GameObject m_LoadingScreenObject;
    public Slider progressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void SwitchToScene(int id)
    {
        m_LoadingScreenObject.SetActive(true);
        progressBar.value = 0;
        StartCoroutine(SwitchToSceneAsync(id));
    }

    IEnumerator SwitchToSceneAsync(int id)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);
        asyncLoad.allowSceneActivation = false;

        float targetProgress = 0f;

        while (asyncLoad.progress < 0.9f)
        {
            targetProgress = asyncLoad.progress;
            progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.deltaTime * 0.3f); // velocidade controlada
            yield return null;
        }

        // Quando a cena estiver praticamente carregada, fazer o progresso ir até 100%
        targetProgress = 1f;
        while (progressBar.value < 0.99f)
        {
            progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.deltaTime * 0.3f);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // opcional: tempo extra para suavidade

        m_LoadingScreenObject.SetActive(false);

        asyncLoad.allowSceneActivation = true;
    }
}
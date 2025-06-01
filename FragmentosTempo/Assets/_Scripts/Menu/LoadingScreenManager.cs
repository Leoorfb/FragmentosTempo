using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;                                // Inst�ncia �nica (Singleton) para acesso global.
    public GameObject m_LoadingScreenObject;                                    // Objeto que representa a tela de carregamento.
    public Slider progressBar;                                                  // Barra de progresso visual da carga da cena.

    private void Awake()
    {
        if (Instance != null && Instance != this)                               // Se j� existir uma inst�ncia e n�o for esta, destrua para manter Singleton.
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;                                                    // Define a inst�ncia e impede que seja destru�da ao trocar de cena.
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;                          // Adiciona o evento de cena carregada.
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;                          // Remove o evento ao destruir para evitar m�ltiplos registros.
        }
    }

    public void SwitchToScene(int id)                                           // M�todo p�blico para iniciar a troca de cena.
    {
        m_LoadingScreenObject.SetActive(true);                                  // Ativa a tela de carregamento.
        progressBar.value = 0;                                                  // Zera a barra de progresso.
        StartCoroutine(SwitchToSceneAsync(id));                                 // Inicia a coroutine de troca ass�ncrona.
    }

    IEnumerator SwitchToSceneAsync(int id)                                      // Coroutine respons�vel por carregar a cena ass�ncronamente.
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);             // Inicia o carregamento da cena.
        asyncLoad.allowSceneActivation = false;                                 // Impede a ativa��o autom�tica da cena at� que a tela de loading termine.

        float targetProgress = 0f;

        while (asyncLoad.progress < 0.9f)                                      // Enquanto a cena n�o estiver carregada at� o limite t�cnico (90%), atualiza suavemente a barra de progresso.
        {
            targetProgress = asyncLoad.progress;
            progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.deltaTime * 0.3f); // Move gradualmente o valor atual da barra em dire��o ao progresso real.
            yield return null;
        }

        targetProgress = 1f;                                                    // Quando a cena estiver quase carregada, leva o progresso visual at� 100%.
        while (progressBar.value < 0.99f)
        {
            progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.deltaTime * 0.3f);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);                                  // Pequena pausa opcional para suavizar a transi��o.

        m_LoadingScreenObject.SetActive(false);                                 // Desativa a tela de carregamento.

        asyncLoad.allowSceneActivation = true;                                  // Permite a ativa��o da nova cena.
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BossTrice")                                          // Se a cena carregada for a "BossTrice".
        {
            MusicManager.Instance.PlayMusic("BossTrice", 1f);                   // Inicia a troca de m�sica com fade.
        }
    }
}
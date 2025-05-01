using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthManager : MonoBehaviour
{
    public static BossHealthManager Instance;                                   // Singleton acess�vel de qualquer lugar do c�digo.

    [SerializeField] private GameObject bossHealthBarPrefab;                    // Prefab da UI de vida do boss.
    private GameObject currentBossBarInstance;                                  // Refer�ncia para a inst�ncia atual da barra de vida.

    private void Awake()
    {
        if (Instance == null)                                                   // Garante que apenas uma inst�ncia de BossHealthManager exista.
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);                                      // Impede que o objeto seja destru�do ao trocar de cena.
        }
        else
        {
            Destroy(gameObject);                                                // Destroi c�pias duplicadas.
        }
    }

    public BossHealthBarUI SpawnBar()                                           // M�todo respons�vel por criar (instanciar) a barra de vida do Boss.
    {
        if (currentBossBarInstance != null)                                     // Se j� existe uma barra instanciada, destr�i antes de criar uma nova.
        {
            Destroy(currentBossBarInstance);
        }

        currentBossBarInstance = Instantiate(bossHealthBarPrefab);              // Instancia a barra de vida na cena.
        currentBossBarInstance.SetActive(false);                                // Deixa a barra inicialmente desativada, para ativar no momento certo.
        return currentBossBarInstance.GetComponent<BossHealthBarUI>();          // Retorna a refer�ncia do componente BossHealthBarUI da barra rec�m-instanciada.
    }

    public void ShowBar(float delay)                                            // M�todo para aguardar um tempo antes de ativar visualmente a barra de vida.
    {
        if (currentBossBarInstance != null)
        {
            Invoke(nameof(ActivateBar), delay);                                 // Usa Invoke para chamar o m�todo ActivateBar ap�s o tempo indicado.
        }
    }

    private void ActivateBar()                                                  // M�todo para ativar visualmente a barra de vida.
    {
        if (currentBossBarInstance != null)
        {
            currentBossBarInstance.SetActive(true);
        }
    }

    public void DestroyBar()                                                    // M�todo para destruir a barra de vida da cena.
    {
        if (currentBossBarInstance != null)
        {
            Destroy(currentBossBarInstance);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthManager : MonoBehaviour
{
    public static BossHealthManager Instance;                                   // Singleton acessível de qualquer lugar do código.

    [SerializeField] private GameObject bossHealthBarPrefab;                    // Prefab da UI de vida do boss.
    private GameObject currentBossBarInstance;                                  // Referência para a instância atual da barra de vida.

    private void Awake()
    {
        if (Instance == null)                                                   // Garante que apenas uma instância de BossHealthManager exista.
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);                                      // Impede que o objeto seja destruído ao trocar de cena.
        }
        else
        {
            Destroy(gameObject);                                                // Destroi cópias duplicadas.
        }
    }

    public BossHealthBarUI SpawnBar()                                           // Método responsável por criar (instanciar) a barra de vida do Boss.
    {
        if (currentBossBarInstance != null)                                     // Se já existe uma barra instanciada, destrói antes de criar uma nova.
        {
            Destroy(currentBossBarInstance);
        }

        currentBossBarInstance = Instantiate(bossHealthBarPrefab);              // Instancia a barra de vida na cena.
        currentBossBarInstance.SetActive(false);                                // Deixa a barra inicialmente desativada, para ativar no momento certo.
        return currentBossBarInstance.GetComponent<BossHealthBarUI>();          // Retorna a referência do componente BossHealthBarUI da barra recém-instanciada.
    }

    public void ShowBar(float delay)                                            // Método para aguardar um tempo antes de ativar visualmente a barra de vida.
    {
        if (currentBossBarInstance != null)
        {
            Invoke(nameof(ActivateBar), delay);                                 // Usa Invoke para chamar o método ActivateBar após o tempo indicado.
        }
    }

    private void ActivateBar()                                                  // Método para ativar visualmente a barra de vida.
    {
        if (currentBossBarInstance != null)
        {
            currentBossBarInstance.SetActive(true);
        }
    }

    public void DestroyBar()                                                    // Método para destruir a barra de vida da cena.
    {
        if (currentBossBarInstance != null)
        {
            Destroy(currentBossBarInstance);
        }
    }
}

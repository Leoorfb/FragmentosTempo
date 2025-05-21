using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthManager : MonoBehaviour
{
    public static BossHealthManager Instance;                                   // Singleton acessível de qualquer lugar do código.

    [SerializeField] private GameObject bossHealthBarPrefab;                    // Prefab da UI de vida do boss.
    private GameObject currentBossBarInstance;                                  // Referência para a instância atual da barra de vida.

    public event System.Action<BossHealthBarUI> OnBarSpawned;                   // Evento que notifica outros scripts quando a barra de vida é criada.

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

    public BossHealthBarUI SpawnBar()                                           // Método que instancia uma nova barra de vida do boss.
    {
        if (currentBossBarInstance != null)                                     // Se já existe uma barra instanciada, destrói antes de criar uma nova.
        {
            Destroy(currentBossBarInstance);
        }

        currentBossBarInstance = Instantiate(bossHealthBarPrefab);              // Instancia a barra de vida na cena.
        currentBossBarInstance.SetActive(false);                                // Deixa a barra inicialmente desativada, para ativar no momento certo.

        var barUI = currentBossBarInstance.GetComponent<BossHealthBarUI>();     // Obtém o script responsável pela UI da barra.
        OnBarSpawned?.Invoke(barUI);                                            // Dispara o evento.
        return barUI;                                                           // Retorna a barra recém-instanciada.
    }

    public void ShowBar(float delay)                                            // Método para aguardar um tempo antes de ativar visualmente a barra de vida.
    {
        if (currentBossBarInstance != null)
        {
            Invoke(nameof(ActivateBar), delay);                                 // Chama ActivateBar depois do delay especificado.
        }
    }

    public void ShowBar()                                                       // Método sem delay, ativa imediatamente.
    {
        ShowBar(0f);                                                            // Chama a versão com delay 0.
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

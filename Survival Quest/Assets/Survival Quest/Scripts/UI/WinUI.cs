using SurvivalGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace SurvivalGame.UI
{

public class WinUI : MonoBehaviour
{
    [SerializeField, Space]
    private GameplayData m_GameplayData;
    void Start()
    {

    }

    void Update()
    {
    }

    public void Continue()
    {
        m_GameplayData.LevelNumber++;
        if (m_GameplayData.LevelNumber > 4)
        {
            m_GameplayData.LevelNumber = 0;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public GameObject childPanel;
    
    public Button menuButton;
    public Button restartBtn;
    public Button exitBtn;
    public Button closeBtn;

    public void Start()
    {
        menuButton.onClick.AddListener(() =>
        {
            childPanel.SetActive(!childPanel.activeInHierarchy);
        });
        closeBtn.onClick.AddListener(() =>
        {
            childPanel.SetActive(false);
        });
        restartBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Scenes/TD_Demo");
        });
        exitBtn.onClick.AddListener(Application.Quit);
    }
}

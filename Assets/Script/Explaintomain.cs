using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class Explaintomain : MonoBehaviour
{
    public Button play;

    void Start()
    {
        if (play != null)
        {
            play.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button not set in the inspector!");
        }
    }
    
    void OnButtonClick()
    {
        SceneManager.LoadScene("Play_Scene");
    }
}
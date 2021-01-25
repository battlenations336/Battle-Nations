
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
  public bool loadAsync = true;
  public string sceneName;
  public bool loadOnStart;

  public void LoadScene()
  {
    if (this.loadAsync)
      SceneManager.LoadSceneAsync(this.sceneName, LoadSceneMode.Single);
    else
      SceneManager.LoadScene(this.sceneName, LoadSceneMode.Single);
  }

  private void Start()
  {
    if (!this.loadOnStart)
      return;
    this.LoadScene();
  }
}


using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AssetHolder : MonoBehaviour
{
  public Sprite[] prefabsToHold;

  private void OnEnable()
  {
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnLevelFinishedLoading);
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnLevelFinishedLoading);
  }

  private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
  {
    Debug.Log((object) "Level Loaded");
    Debug.Log((object) scene.name);
    Debug.Log((object) mode);
    SceneManager.LoadScene("Menu");
  }
}

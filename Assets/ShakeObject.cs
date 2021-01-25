
using System.Collections;
using UnityEngine;

public class ShakeObject : MonoBehaviour
{
  private bool objectIs2D = true;
  public GameObject GameObjectToShake;
  private Transform objTransform;
  private Vector3 defaultPos;
  private Quaternion defaultRot;
  private const float speed = 0.05f;
  private const float angleRot = 3f;
  private float counter;
  private bool shaking;

  private IEnumerator shakeGameObjectCOR(
    GameObject objectToShake,
    float totalShakeDuration,
    float decreasePoint,
    bool objectIs2D = false)
  {
    if ((double) decreasePoint >= (double) totalShakeDuration)
    {
      Debug.LogError((object) "decreasePoint must be less than totalShakeDuration...Exiting");
    }
    else
    {
      this.objTransform = objectToShake.transform;
      this.defaultPos = this.objTransform.position;
      this.defaultRot = this.objTransform.rotation;
      float counter = 0.0f;
      while ((double) counter < (double) totalShakeDuration)
      {
        counter += Time.deltaTime;
        float num1 = 0.05f;
        if (objectIs2D)
        {
          Vector3 vector3 = this.defaultPos + Random.insideUnitSphere * num1;
          vector3.z = this.defaultPos.z;
          this.objTransform.position = vector3;
          this.objTransform.rotation = this.defaultRot * Quaternion.AngleAxis(Random.Range(-3f, 3f), new Vector3(0.0f, 0.0f, 1f));
        }
        else
        {
          this.objTransform.position = this.defaultPos + Random.insideUnitSphere * num1;
          this.objTransform.rotation = this.defaultRot * Quaternion.AngleAxis(Random.Range(-3f, 3f), new Vector3(1f, 1f, 1f));
        }
        yield return (object) null;
        if ((double) counter >= (double) decreasePoint)
        {
          Debug.Log((object) "Decreasing shake");
          counter = 0.0f;
          while ((double) counter <= (double) decreasePoint)
          {
            counter += Time.deltaTime;
            float num2 = Mathf.Lerp(0.05f, 0.0f, counter / decreasePoint);
            float max = Mathf.Lerp(3f, 0.0f, counter / decreasePoint);
            Debug.Log((object) ("Decrease Value: " + (object) num2));
            if (objectIs2D)
            {
              Vector3 vector3 = this.defaultPos + Random.insideUnitSphere * num2;
              vector3.z = this.defaultPos.z;
              this.objTransform.position = vector3;
              this.objTransform.rotation = this.defaultRot * Quaternion.AngleAxis(Random.Range(-max, max), new Vector3(0.0f, 0.0f, 1f));
            }
            else
            {
              this.objTransform.position = this.defaultPos + Random.insideUnitSphere * num2;
              this.objTransform.rotation = this.defaultRot * Quaternion.AngleAxis(Random.Range(-max, max), new Vector3(1f, 1f, 1f));
            }
            yield return (object) null;
          }
          break;
        }
      }
      this.objTransform.position = this.defaultPos;
      this.objTransform.rotation = this.defaultRot;
      this.shaking = false;
      Debug.Log((object) "Done!");
    }
  }

  private void shakeGameObject(GameObject objectToShake, float shakeDuration, float decreasePoint)
  {
    if (this.shaking)
      return;
    this.shaking = true;
    this.counter = 0.0f;
    this.objTransform = objectToShake.transform;
    this.defaultPos = this.objTransform.position;
    this.defaultRot = this.objTransform.rotation;
  }

  public void StartShake()
  {
    this.shakeGameObject(this.gameObject, 5f, 3f);
  }

  public void StopShake()
  {
    this.objTransform.position = this.defaultPos;
    this.objTransform.rotation = this.defaultRot;
    this.shaking = false;
  }

  private void Update()
  {
    if (!this.shaking)
      return;
    this.counter += Time.deltaTime;
    float num = 0.05f;
    if (this.objectIs2D)
    {
      Vector3 vector3 = this.defaultPos + Random.insideUnitSphere * num;
      vector3.z = this.defaultPos.z;
      this.objTransform.position = vector3;
      this.objTransform.rotation = this.defaultRot * Quaternion.AngleAxis(Random.Range(-3f, 3f), new Vector3(0.0f, 0.0f, 1f));
    }
    else
    {
      this.objTransform.position = this.defaultPos + Random.insideUnitSphere * num;
      this.objTransform.rotation = this.defaultRot * Quaternion.AngleAxis(Random.Range(-3f, 3f), new Vector3(1f, 1f, 1f));
    }
  }
}

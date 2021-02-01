
using UnityEngine;
using UnityEngine.UI;

public class StatLineCtrl : MonoBehaviour
{
  public Text Name;
  public Text Value;

  public void Show(string _name, string _value)
  {
    this.Name.text = _name;
    this.Value.text = _value;
  }
}

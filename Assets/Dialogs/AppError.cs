using UnityEngine;
using UnityEngine.UI;

public class AppError : DialogPanel
{
    GameObject newObject;
    GameObject part_prefab;

    public Text ErrorMessage;
    public Button OKButton;

    public new void Open()
    {
        base.Open();

        part_prefab = (GameObject)Resources.Load("DialogText");
        part_prefab.name = "Title";

        newObject = Instantiate(part_prefab);
        newObject.GetComponent<Text>().text = "ERROR!";
        newObject.transform.SetParent(Container.transform);
        newObject.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.25f), getOrigin().y + (getHeight() * 0.5f) - (getMargin_V() * 0.25f), 0);


        part_prefab = (GameObject)Resources.Load("DialogButton");
        part_prefab.name = "Button";

        newObject = Instantiate(part_prefab);
        newObject.GetComponentInChildren<Text>().text = "OK";
        newObject.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        newObject.transform.SetParent(Container.transform, true);
        newObject.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.25f), getOrigin().y + (getMargin_V() * 0.25f), 0);
        OKButton = newObject.GetComponent<Button>();
    }

    public void SetErrorMessage(string text)
    {
        ErrorMessage.text = text;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MOTD : DialogPanel
{
    GameObject newObject;
    GameObject part_prefab;
    
    public Button OKButton;

    public new void Open()
    {
        base.Open();

        part_prefab = (GameObject)Resources.Load("DialogText");
        part_prefab.name = "Title";

        newObject = Instantiate(part_prefab);
        newObject.GetComponent<Text>().text = "WARNING!";
        newObject.transform.SetParent(Container.transform);
        newObject.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.25f), getOrigin().y + (getHeight() * 0.5f) - (getMargin_V() * 0.25f), 0);


        part_prefab = (GameObject)Resources.Load("DialogButton");
        part_prefab.name = "Button";

        newObject = Instantiate(part_prefab);
        newObject.GetComponentInChildren<Text>().text = "I UNDERSTAND";
        newObject.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        newObject.transform.SetParent(Container.transform, true);
        newObject.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.35f), getOrigin().y + (getMargin_V() * 0.25f), 0);
        OKButton = newObject.GetComponent<Button>();
    }
}

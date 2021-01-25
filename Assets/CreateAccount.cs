using UnityEngine;
using UnityEngine.UI;

public class CreateAccount : DialogPanel
{
    GameObject title;
    GameObject part_prefab;

    public InputField Name;
    public InputField Password;

    public Button CancelButton;
    public Button OKButton;
    
    public new void Open()
    {
        base.Open();

        part_prefab = (GameObject)Resources.Load("DialogText");
        part_prefab.name = "Title";

        title = Instantiate(part_prefab);
        title.GetComponent<Text>().text = "Create New Account";
        title.transform.SetParent(Container.transform);
        title.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.25f), getOrigin().y + (getHeight() * 0.5f) - (getMargin_V() * 0.25f), 0);

        title = Instantiate(part_prefab);
        title.GetComponent<Text>().text = "Scores and stats will be stored on Z3Online servers";
        title.transform.SetParent(Container.transform);
        title.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.25f), getOrigin().y + (getHeight() * 0.5f) - (getMargin_V() * 0.40f), 0);
        title.GetComponent<Text>().fontSize = 14;

        part_prefab = (GameObject)Resources.Load("DialogButton");
        part_prefab.name = "Button";

        title = Instantiate(part_prefab);
        title.GetComponentInChildren<Text>().text = "Cancel";
        title.transform.SetParent(Container.transform);
        title.transform.position = new Vector3(getOrigin().x + (getMargin_H() * 0.75f), getOrigin().y + (getMargin_V() * 0.25f), 0);
        CancelButton = title.GetComponent<Button>();

        title = Instantiate(part_prefab);
        title.GetComponentInChildren<Text>().text = "OK";
        title.transform.SetParent(Container.transform, true);
        title.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.35f), getOrigin().y + (getMargin_V() * 0.25f), 0);
        OKButton = title.GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

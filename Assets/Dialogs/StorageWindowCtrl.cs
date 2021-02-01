using BNR;
using UnityEngine;
using UnityEngine.UI;

public class StorageWindowCtrl : MonoBehaviour
{
    public Text Title;
    public Text PopRequirement;
    public Text Info;

    public Text StoneAmount;
    public Text WoodAmount;
    public Text IronAmount;
    public Text OilAmount;
    public Text ConcreteAmount;
    public Text LumberAmount;
    public Text SteelAmount;
    public Text CoalAmount;

    public void Show(BuildingEntity warehouse)
    {
        Title.text = GameData.GetText(warehouse.composition.componentConfigs.StructureMenu.name);
        PopRequirement.text = warehouse.composition.componentConfigs.RequireWorkers.workers.ToString();
        Info.text = string.Format("This warehouse contributes {0} storage for each resource",
            warehouse.composition.componentConfigs.ResourceCapacity.contribution.resources.stone);

        UpdateResourceIndicator(StoneAmount, Resource.stone);
        UpdateResourceIndicator(WoodAmount, Resource.wood);
        UpdateResourceIndicator(IronAmount, Resource.iron);
        UpdateResourceIndicator(OilAmount, Resource.oil);
        UpdateResourceIndicator(ConcreteAmount, Resource.concrete);
        UpdateResourceIndicator(LumberAmount, Resource.lumber);
        UpdateResourceIndicator(SteelAmount, Resource.steel);
        UpdateResourceIndicator(CoalAmount, Resource.coal);
    }

    void UpdateResourceIndicator(Text text, Resource type)
    {
        text.text = string.Format("{0}/{1}", GameData.Player.Storage.GetResource(type),
            GameData.Player.Storage.GetMaxResource(type));
        if (GameData.Player.Storage.ResourceFull(type))
            text.color = Functions.GetColor(200, 3, 3);
        else
            text.color = Functions.GetColor(50, 50, 50);
    }
}

using BNR;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CategoryPanelCtrl : MonoBehaviour
{
    public EventHandler<PurchaseEventArgs> OnButtonClick { get; set; }

    public GameObject CategoryPanel;
    public GameObject IconPanel;
    public GameObject IconGrid;
    public GameObject ItemPanel;
    public GameObject ItemCard;
    public GameObject ItemDetail;
    public GameObject BackHost;
    public GameObject ExitHost;
    public Text Title;
    public Image BackgroundImage;

    public GameObject TopPanel;
    public GameObject BottomPanel;

    GameObject part_prefab;
    GameObject card_prefab;
    CategoryButtonCtrl[] buttons;
    ItemCardCtrl[] itemCards;
    ItemDetailCtrl itemDetailCtrl;

    float topRow;
    float bottomRow;

    float columnOne;
    float columnTwo;
    float columnThree;
    float columnFour;

    float margin = 10;

    bool transitioning = false;
    float speed = 2000;

    DepotMode depotMode;
    enum DepotMode
    {
        Constructing,
        Opening,
        Category,
        Item
    }

    private void Awake()
    {
        buttons = new CategoryButtonCtrl[8];
    }
    public void Init()
    {
        depotMode = DepotMode.Opening;
        BackHost.SetActive(false);

        part_prefab = (GameObject)Resources.Load("CategoryButton");
        part_prefab.name = "CategoryButton";
        card_prefab = (GameObject)Resources.Load("ItemCard");
        card_prefab.name = "ItemButton";
        Title.text = "Building Depot";
        BackgroundImage.sprite = GameData.GetSprite("squareBlank");

        RectTransform rt = part_prefab.transform as RectTransform;

        topRow = Screen.height / 2 + rt.rect.height / 2;
        bottomRow = Screen.height / 2 - rt.rect.height / 2;

        columnOne = Screen.width / 2 - rt.rect.width * 1.5f;
        float bx = Camera.main.ViewportToScreenPoint(new Vector3(0, 0, 0)).x;
        if (columnOne - (rt.rect.width / 2) < bx)
            columnOne = bx + rt.rect.width / 2 + margin;
        columnTwo = columnOne + rt.rect.width;
        columnThree = columnTwo + rt.rect.width;
        columnFour = columnThree + rt.rect.width;

        for (int x = 0; x < 8; x++)
        {
            if (buttons[x] != null)
            {
                Destroy(buttons[x]);
                buttons[x] = null;
            }
        }
        AddCategoryButton(Category.bmCat_houses, "housingCategory@2x", "housingIcon@2x", topRow, columnOne, rt.rect.width, 1);
        AddCategoryButton(Category.bmCat_shops, "goodsCategory@2x", "goodsIcon@2x", bottomRow, columnOne, rt.rect.width, 2);

        AddCategoryButton(Category.bmCat_military, "militaryCategory@2x", "militaryIcon@2x", topRow, columnTwo, rt.rect.width, 3);
        AddCategoryButton(Category.bmCat_boosts, "boostsCategory@2x", "boostIcon@2x", bottomRow, columnTwo, rt.rect.width, 4);

        AddCategoryButton(Category.bmCat_resources, "resourceCategory@2x", "resourcesIcon@2x", topRow, columnThree, rt.rect.width, 5);
        AddCategoryButton(Category.bmCat_store, "nanopodCategory@2x", "nanopodIcon@2x", bottomRow, columnThree, rt.rect.width, 6);

        AddCategoryButton(Category.bmCat_decorations, "decorationCategory@2x", "decorationsIcon@2x", topRow, columnFour, rt.rect.width, 7);
        AddCategoryButton(Category.bmCat_expansion, "expansionCategory@2x", "expansionIcon@2x", bottomRow, columnFour, rt.rect.width, 8);

        IconPanel.SetActive(false);

        itemDetailCtrl = ItemDetail.GetComponent<ItemDetailCtrl>();
        if (itemDetailCtrl.OnBuildClick == null)
            itemDetailCtrl.OnBuildClick += ItemDetail_OnClick;
    }

    public void InitPanels()
    {
        Vector3 newpos;

        TopPanel.GetComponent<TransitionPanel>().EndPos = TopPanel.transform.position;
        newpos = new Vector3(TopPanel.transform.position.x, Screen.height + (BackHost.GetComponent<Image>().rectTransform.rect.height / 2.0f), 0);
        TopPanel.transform.position = newpos;

        float by = Camera.main.ViewportToScreenPoint(new Vector3(0, 0, 0)).y;
        BottomPanel.GetComponent<TransitionPanel>().EndPos = BottomPanel.transform.position;
        newpos = new Vector3(BottomPanel.transform.position.x, by - (BottomPanel.GetComponent<Image>().rectTransform.rect.height / 2.0f), 0);
        BottomPanel.transform.position = newpos;

        TopPanel.SetActive(true);
        BottomPanel.SetActive(true);
    }

    void InitCategories()
    { 
        SetScroll(false);
        buttons[0].Play();
    }

    public void ResetPanel()
    {
        TopPanel.SetActive(false);
        BottomPanel.SetActive(false);

        if (itemCards != null)
        {
            foreach (ItemCardCtrl cardButton in itemCards)
            {
                if (cardButton != null && cardButton.gameObject != null)
                    Destroy(cardButton.gameObject);
            }
        }

        foreach (CategoryButtonCtrl catButton in buttons)
        {
            Destroy(catButton.gameObject);
        }

        itemDetailCtrl.OnBuildClick -= ItemDetail_OnClick;
    }

    private void Update()
    {
        if (transitioning)
            UpdateTransition();
    }

    public void StartTransition()
    {
        transitioning = true;
        gameObject.GetComponent<AudioSource>().Play();
    }

    void UpdateTransition()
    {
        float delta = Time.deltaTime * speed;

        if (transform.position.y < Screen.height / 2.0f)
        {
            if (transform.position.y + delta > Screen.height/2.0f)
            {
                Vector3 tempPos = transform.position;
                tempPos.y = Screen.height / 2.0f;
                transform.position = tempPos;
            }
            else
                transform.position += new Vector3(0, delta, 0);
        }
        else
        {
            transitioning = false;
            Init();
            InitCategories();
            InitPanels();
            TopPanel.GetComponent<TransitionPanel>().Play();
            BottomPanel.GetComponent<TransitionPanel>().Play();
        }
    }

    void SetScroll(bool _toggle)
    {
        IconPanel.GetComponent<ScrollRect>().horizontal = _toggle;
    }

    void AddCategoryButton(Category _category, string iconName, string symbolName, float row, float column, float offset, int buttonNo)
    {
        GameObject iconButton;
        Sprite icon = Resources.Load<Sprite>("UI/" + iconName) as Sprite;
        Sprite symbol = Resources.Load<Sprite>("UI/" + symbolName) as Sprite;

        iconButton = Instantiate(part_prefab);
        
        var button = iconButton.GetComponent<Button>() as Button;
        CategoryButtonCtrl categoryButtonCtrl = iconButton.GetComponent<CategoryButtonCtrl>();
        categoryButtonCtrl.Icon.sprite = icon;
        categoryButtonCtrl.Text.text = GameData.BMCatToString(_category);
        categoryButtonCtrl.Symbol.sprite = symbol;
        categoryButtonCtrl.Id = buttonNo;
        categoryButtonCtrl.OnComplete += CategoryButton_OnComplete;
        button.onClick.AddListener(delegate { ExecuteButton(_category); });

        iconButton.transform.SetParent(CategoryPanel.transform, false);
        categoryButtonCtrl.StartPos = new Vector3(column + Screen.width, row, 0);
        categoryButtonCtrl.EndPos = new Vector3(column, row, 0);
        categoryButtonCtrl.Offset = offset * 2;

        buttons[buttonNo - 1] = categoryButtonCtrl;
    }

    void ExecuteButton(Category _category)
    {
        if (_category == Category.bmCat_boosts || _category == Category.bmCat_store)
            return;

        int count = 0;
        GameObject iconButton;
        StructureMenu structureMenu = GameData.StructureMenu.Where(X => X.title == _category.ToString()).FirstOrDefault();
        float column = 0.0f;

        depotMode = DepotMode.Item;
        RectTransform rt = card_prefab.transform as RectTransform;
        column = margin + rt.rect.width / 2;
        itemCards = new ItemCardCtrl[structureMenu.options.Count()];

        float width = IconGrid.GetComponent<Image>().rectTransform.rect.width;
        float newWidth = (margin + rt.rect.width) * structureMenu.options.Count();
        if (width < newWidth)
            IconGrid.transform.localScale = new Vector3(newWidth / width, 1.0f, 1.0f);
        else
            IconGrid.transform.localScale = new Vector3(0.5f, 1.0f, 1.0f);

        float cardScale = 1.0f / (newWidth / width);
        foreach (string itemKey in structureMenu.options)
        {
            Composition config = GameData.Compositions[itemKey];
            string iconName = GameData.NormaliseIconName(string.Format("{0}", config.componentConfigs.StructureMenu.icon));

            Sprite icon = Resources.Load<Sprite>("Icons/" + iconName) as Sprite;

            iconButton = Instantiate(card_prefab);

            var button = iconButton.GetComponent<Button>() as Button;
            ItemCardCtrl itemCardCtrl = iconButton.GetComponent<ItemCardCtrl>();
            itemCardCtrl.InitFromComposition(itemKey, config);
            itemCardCtrl.MainIcon.sprite = icon;
            itemCardCtrl.Name.text = GameData.GetText(config.componentConfigs.StructureMenu.name.ToLower());
            Sprite spr = Resources.Load<Sprite>("UI/" + GameData.NormaliseIconName(config.componentConfigs.StructureMenu.roleIconName));
            if (spr == null)
            {
                Debug.Log("Symbol " + GameData.NormaliseIconName(config.componentConfigs.StructureMenu.roleIconName) + " not found");
                spr = Resources.Load<Sprite>("squareBlank");
            }
            itemCardCtrl.TypeIcon.sprite = spr;
            itemCardCtrl.OnComplete += ItemCard_OnComplete;
            itemCardCtrl.Id = count;
            
            button.onClick.AddListener(delegate { ExecuteItemButton(itemKey); });

            iconButton.transform.SetParent(IconGrid.transform, false);

            itemCardCtrl.StartPos = new Vector3(column + Screen.width, Screen.height / 2, 0);
            itemCardCtrl.EndPos = new Vector3(column, Screen.height / 2, 0);
            itemCardCtrl.Offset = rt.rect.width * 2;

            iconButton.transform.position = itemCardCtrl.StartPos;
            iconButton.transform.localScale = new Vector3(cardScale, 1.0f, 1.0f);
            column += rt.rect.width + margin;

            itemCards[count] = itemCardCtrl;
            count++;
        }
        

        BackHost.SetActive(true);
        IconPanel.SetActive(true);

        buttons[0].StartPos = new Vector3(columnOne - rt.rect.width, topRow, 0);
        buttons[0].EndPos = new Vector3(columnOne - Screen.width - rt.rect.width, topRow, 0);
        buttons[1].StartPos = new Vector3(columnOne - rt.rect.width, bottomRow, 0);
        buttons[1].EndPos = new Vector3(columnOne - Screen.width - rt.rect.width, bottomRow, 0);
        buttons[2].StartPos = new Vector3(columnTwo - rt.rect.width, topRow, 0);
        buttons[2].EndPos = new Vector3(columnTwo - Screen.width - rt.rect.width, topRow, 0);
        buttons[3].StartPos = new Vector3(columnTwo - rt.rect.width, bottomRow, 0);
        buttons[3].EndPos = new Vector3(columnTwo - Screen.width - rt.rect.width, bottomRow, 0);
        buttons[4].StartPos = new Vector3(columnThree - rt.rect.width, topRow, 0);
        buttons[4].EndPos = new Vector3(columnThree - Screen.width - rt.rect.width, topRow, 0);
        buttons[5].StartPos = new Vector3(columnThree - rt.rect.width, bottomRow, 0);
        buttons[5].EndPos = new Vector3(columnThree - Screen.width - rt.rect.width, bottomRow, 0);
        buttons[6].StartPos = new Vector3(columnFour - rt.rect.width, topRow, 0);
        buttons[6].EndPos = new Vector3(columnFour - Screen.width - rt.rect.width, topRow, 0);
        buttons[7].StartPos = new Vector3(columnFour - rt.rect.width, bottomRow, 0);
        buttons[7].EndPos = new Vector3(columnFour - Screen.width - rt.rect.width, bottomRow, 0);

        SetScroll(false);
        buttons[0].Play();

        Title.text = GameData.BMCatToString(_category);
        BackgroundImage.sprite = GameData.GetSprite(GameData.BMCatToBackground(_category));
    }

    void UpdateItemPanel(string configName)
    {
        Composition config = GameData.Compositions[configName];
        string iconName = GameData.NormaliseIconName(string.Format("{0}", config.componentConfigs.StructureMenu.icon));

        Sprite icon = Resources.Load<Sprite>("Icons/" + iconName) as Sprite;

        ItemCardCtrl itemCardCtrl = ItemCard.GetComponent<ItemCardCtrl>();
        itemCardCtrl.InitFromComposition(configName, config);
        itemCardCtrl.MainIcon.sprite = icon;
        itemCardCtrl.Name.text = GameData.GetText(config.componentConfigs.StructureMenu.name.ToLower());
        Sprite spr = Resources.Load<Sprite>("UI/" + GameData.NormaliseIconName(config.componentConfigs.StructureMenu.roleIconName));
        if (spr == null)
        {
            Debug.Log("Symbol " + GameData.NormaliseIconName(config.componentConfigs.StructureMenu.roleIconName) + " not found");
            spr = Resources.Load<Sprite>("squareBlank");
        }
        itemCardCtrl.TypeIcon.sprite = spr;
        itemCardCtrl.Id = 1;
        
        ItemDetailCtrl itemDetailCtrl = ItemDetail.GetComponent<ItemDetailCtrl>();
        itemDetailCtrl.InitFromComposition(configName, config);
//        itemDetailCtrl.BuildButton.onClick.RemoveAllListeners();
//        itemDetailCtrl.BuildButton.onClick.RemoveListener(ItemButton_OnClick);
//        itemDetailCtrl.BuildButton.onClick.AddListener(delegate { ItemButton_OnClick(configName); });
    }

    void ExecuteItemButton(string _item)
    {
        ItemPanel.SetActive(true);
        UpdateItemPanel(_item);
    }

    public void BackButton_OnClick()
    {
        int firstCard = -1;
        int lastCard = 100;

        float bx = Camera.main.ViewportToScreenPoint(new Vector3(0, 0, 0)).x;

        for (int k = itemCards.Length - 1; k >= 0; k--)
        {
            if (itemCards[k].gameObject.transform.position.x > Screen.width || itemCards[k].gameObject.transform.position.x < bx)
            {
                Destroy(itemCards[k].gameObject);
            }
            else
            {
                if (firstCard < 0)
                    firstCard = k;
                if (lastCard > k)
                    lastCard = k;
                itemCards[k].StartPos = itemCards[k].gameObject.transform.position;
                itemCards[k].EndPos = itemCards[k].gameObject.transform.position + new Vector3(Screen.width, 0, 0);
            }
        }

        if (itemCards[lastCard].Id != 0)
            itemCards[lastCard].Id = 0;

        if (firstCard > -1)
        {
            SetScroll(false);
            itemCards[firstCard].Play();
        }

        depotMode = DepotMode.Category;
        BackHost.SetActive(false);
        Title.text = "Building Depot";
        BackgroundImage.sprite = GameData.GetSprite("squareBlank");
    }

    void CategoryButton_OnComplete(object sender, EventArgs e)
    {
        CategoryButtonCtrl categoryButtonCtrl;

        categoryButtonCtrl = sender as CategoryButtonCtrl;

        if (depotMode == DepotMode.Item || depotMode == DepotMode.Opening)
        {
            if (categoryButtonCtrl.Id < 8)
                buttons[categoryButtonCtrl.Id].Play();
            else
            {
                if (depotMode == DepotMode.Opening)
                    SetScroll(true);
            }

            if (depotMode == DepotMode.Item && categoryButtonCtrl.Id == 7)
            {
                itemCards[0].Play();
            }
        }
        else
        {
            if (categoryButtonCtrl.Id > 1)
                buttons[categoryButtonCtrl.Id - 2].Play();
            else
                buttons[0].Play();
        }
    }

    void ItemCard_OnComplete(object sender, EventArgs e)
    {
        ItemCardCtrl itemCardCtrl;

        itemCardCtrl = sender as ItemCardCtrl;

        if (depotMode == DepotMode.Item)
        {
            if (itemCardCtrl.Id < (itemCards.Length - 1))
            {
                if (itemCards[itemCardCtrl.Id + 1].EndPos.x > Screen.width)
                    itemCards[itemCardCtrl.Id + 1].AutoComplete();
                else
                    itemCards[itemCardCtrl.Id + 1].Play();
            }
            else
                SetScroll(true);
        }
        else
        {
            if (itemCardCtrl.Id > 0)
                itemCards[itemCardCtrl.Id - 1].Play();
            if (itemCardCtrl.Id == 0)
            {
                IconPanel.SetActive(false);

                buttons[0].EndPos = new Vector3(columnOne, topRow, 0);
                buttons[0].StartPos = new Vector3(columnOne - Screen.width, topRow, 0);
                buttons[1].EndPos = new Vector3(columnOne, bottomRow, 0);
                buttons[1].StartPos = new Vector3(columnOne - Screen.width, bottomRow, 0);
                buttons[2].EndPos = new Vector3(columnTwo, topRow, 0);
                buttons[2].StartPos = new Vector3(columnTwo - Screen.width, topRow, 0);
                buttons[3].EndPos = new Vector3(columnTwo, bottomRow, 0);
                buttons[3].StartPos = new Vector3(columnTwo - Screen.width, bottomRow, 0);
                buttons[4].EndPos = new Vector3(columnThree, topRow, 0);
                buttons[4].StartPos = new Vector3(columnThree - Screen.width, topRow, 0);
                buttons[5].EndPos = new Vector3(columnThree, bottomRow, 0);
                buttons[5].StartPos = new Vector3(columnThree - Screen.width, bottomRow, 0);
                buttons[6].EndPos = new Vector3(columnFour, topRow, 0);
                buttons[6].StartPos = new Vector3(columnFour - Screen.width, topRow, 0);
                buttons[7].EndPos = new Vector3(columnFour, bottomRow, 0);
                buttons[7].StartPos = new Vector3(columnFour - Screen.width, bottomRow, 0);

                buttons[7].Play();
            }
            Destroy(itemCardCtrl.gameObject);
        }
    }

    public void ItemDetail_OnClose()
    {
        ItemDetailCtrl itemDetailCtrl = ItemDetail.GetComponent<ItemDetailCtrl>();
        itemDetailCtrl.ResetPanel();
        //itemDetailCtrl.OnBuildClick -= ItemDetail_OnClick;
        ItemPanel.SetActive(false);        
    }

    public void ItemDetail_OnClick(object sender, ButtonEventArgs args)
    {
        PurchaseEventArgs purchaseEventArgs = new PurchaseEventArgs(args.StringValue);

        if (OnButtonClick != null)
            OnButtonClick(this, purchaseEventArgs);

        ItemDetail_OnClose();
    }
}

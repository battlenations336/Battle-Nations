
using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AssetLoader : MonoBehaviour
{
  private Queue<string> loadQueue = new Queue<string>();
  public static AssetLoader instance;
  private int sheetNo;
  private int frameCount;
  private Sprite[] frameSet;
  private SpriteType loadType;
  private string loadAnimationName;
  private BNR.AnimationInfo loadInfo;
  private int frameWidth;
  private int frameHeight;
  private int framesPerLine;
  private int lines;
  private int numFrames;

  private void Awake()
  {
    if ((UnityEngine.Object) AssetLoader.instance == (UnityEngine.Object) null)
      AssetLoader.instance = this;
    else if ((UnityEngine.Object) AssetLoader.instance != (UnityEngine.Object) this)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    Addressables.InitializeAsync();
    this.loadQueue = new Queue<string>();
  }

  public void Start()
  {
  }

  public void Init()
  {
    this.sheetNo = 0;
    this.frameCount = 0;
  }

  public async void LoadAnimation(string addressName)
  {
    if (!string.IsNullOrEmpty(this.loadAnimationName))
    {
      this.loadQueue.Enqueue(addressName);
    }
    else
    {
      this.loadAnimationName = addressName;
      IList<IResourceLocation> task = await Addressables.LoadResourceLocationsAsync((object) this.GetAssetAddress(addressName), (Type) null).Task;
      if (task != null && task.Count > 0)
      {
        this.Init();
        this.loadInfo = !GameData.AnimationInfo.ContainsKey(addressName) ? GameData.AnimationInfo[addressName.ToLower()] : GameData.AnimationInfo[addressName];
        this.loadType = SpriteType.Idle_Building;
        this.frameWidth = this.loadInfo.width;
        this.frameHeight = this.loadInfo.height;
        this.numFrames = this.loadInfo.numOfFrames;
        this.frameSet = new Sprite[this.numFrames];
        this.LoadAsset(addressName);
      }
      else
      {
        Debug.Log((object) string.Format("Cannot find asset {0}", (object) this.GetAssetAddress(addressName)));
        this.loadAnimationName = string.Empty;
        this.Init();
        if (this.loadQueue.Count <= 0)
          return;
        string addressName1 = this.loadQueue.Dequeue();
        if (string.IsNullOrEmpty(addressName1))
          return;
        this.LoadAnimation(addressName1);
      }
    }
  }

  private void LoadAsset(string addressName)
  {
    Addressables.LoadAsset<Texture2D>((object) this.GetAssetAddress(this.loadAnimationName)).Completed += new Action<AsyncOperationHandle<Texture2D>>(this.TextureHandle_Completed);
  }

  private async void TextureHandle_Completed(AsyncOperationHandle<Texture2D> handle)
  {
    if (handle.Status == AsyncOperationStatus.Succeeded && (UnityEngine.Object) handle.Result != (UnityEngine.Object) null && this.sheetNo <= 10)
    {
      Texture2D result = handle.Result;
      if (!((UnityEngine.Object) result != (UnityEngine.Object) null))
        return;
      this.framesPerLine = result.width / this.frameWidth;
      this.lines = result.height / this.frameHeight;
      int num1 = this.framesPerLine * this.lines;
      for (int index = 0; index < num1; ++index)
      {
        int num2 = index / this.framesPerLine;
        int num3 = this.frameWidth * (index % this.framesPerLine);
        int num4 = this.frameHeight * (this.lines - num2 - 1);
        if (this.frameCount < this.numFrames)
          this.frameSet[this.frameCount] = this.loadType != SpriteType.AOE ? (!this.IsBuilding(this.loadType) ? Sprite.Create(result, new Rect((float) num3, (float) num4, (float) this.frameWidth, (float) this.frameHeight), this.setPivot(this.loadInfo), 100f) : Sprite.Create(result, new Rect((float) num3, (float) num4, (float) this.frameWidth, (float) this.frameHeight), this.setPivot_B(this.loadInfo), 100f)) : Sprite.Create(result, new Rect((float) num3, (float) num4, (float) this.frameWidth, (float) this.frameHeight), new Vector2(0.5f, 0.5f), 100f);
        ++this.frameCount;
      }
      ++this.sheetNo;
      IList<IResourceLocation> task = await Addressables.LoadResourceLocationsAsync((object) this.GetAssetAddress(this.loadAnimationName), (Type) null).Task;
      if (task != null && task.Count > 0)
      {
        this.LoadAsset(this.GetAssetAddress(this.loadAnimationName));
      }
      else
      {
        if (AssetCache.compositionAnimation.ContainsKey(this.loadAnimationName))
        {
          Debug.Log((object) string.Format("Loaded asset {0}", (object) this.GetAssetAddress(this.loadAnimationName)));
          AssetCache.LoadAsset(this.loadAnimationName, this.frameSet);
        }
        else if (AssetCache.compositionAnimation.ContainsKey(this.loadAnimationName.ToLower()))
        {
          Debug.Log((object) string.Format("Loaded asset {0}", (object) this.GetAssetAddress(this.loadAnimationName.ToLower())));
          AssetCache.LoadAsset(this.loadAnimationName.ToLower(), this.frameSet);
        }
        this.loadAnimationName = string.Empty;
        this.Init();
        if (this.loadQueue.Count <= 0)
          return;
        string addressName = this.loadQueue.Dequeue();
        if (string.IsNullOrEmpty(addressName))
          return;
        this.LoadAnimation(addressName);
      }
    }
    else
    {
      if (this.frameCount >= 0 && AssetCache.compositionAnimation.ContainsKey(this.loadAnimationName))
        AssetCache.compositionAnimation[this.loadAnimationName] = this.frameSet;
      this.loadAnimationName = string.Empty;
    }
  }

  private string GetAssetAddress(string assetName)
  {
    string empty = string.Empty;
    return string.Format("{1}_{2}", (object) this.GetFolder(this.loadType), (object) assetName, (object) this.sheetNo.ToString());
  }

  private Vector2 setPivot(BNR.AnimationInfo _info)
  {
    int num = 0;
    Vector3 vector3 = new Vector3((float) (_info.left / 100), (float) (_info.top / 100 * -1 - (_info.height / 100 - num)), 0.0f);
    vector3.y = (float) (_info.height / 100 - Math.Abs(_info.top / 100) + num);
    return (Vector2) new Vector3(Math.Abs(vector3.x) / (float) (_info.width / 100), Math.Abs(vector3.y / 100f) / (float) (_info.height / 100));
  }

  private Vector2 setPivot_B(BNR.AnimationInfo _info)
  {
    Vector3 vector3 = new Vector3((float) _info.left, (float) (_info.top * -1 - _info.height), 0.0f);
    vector3.y = (float) (_info.height - Math.Abs(_info.top));
    return (Vector2) new Vector3(Math.Abs(vector3.x) / (float) _info.width, Math.Abs(vector3.y) / (float) _info.height);
  }

  public static bool AssetExists(object key)
  {
    if (Application.isPlaying)
    {
      foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
      {
        if (resourceLocator.Locate(key, (Type) null, out IList<IResourceLocation> _))
          return true;
      }
      return false;
    }
    if (Application.isEditor)
    {
      int num = Application.isPlaying ? 1 : 0;
    }
    return false;
  }

  private string GetFolder(SpriteType type)
  {
    string empty = string.Empty;
    string str;
    switch (type)
    {
      case SpriteType.Default:
        str = "Animations/Buildings/Idle/";
        break;
      case SpriteType.Idle:
        str = "Animations/Units/Idle/";
        break;
      case SpriteType.Attack:
        str = "Animations/Units/Attack/";
        break;
      case SpriteType.Damage:
        str = "Animations/Units/Damage/";
        break;
      case SpriteType.Idle_Building:
        str = "Animations/Buildings/Idle/";
        break;
      case SpriteType.Busy:
        str = "Animations/Buildings/Busy/";
        break;
      case SpriteType.Construction1:
        str = "Animations/Buildings/Construction/";
        break;
      case SpriteType.Construction2:
        str = "Animations/Buildings/Construction/";
        break;
      default:
        str = "Animations/";
        break;
    }
    return str;
  }

  private bool IsBuilding(SpriteType _type)
  {
    return _type == SpriteType.Idle_Building || _type == SpriteType.Construction1 || (_type == SpriteType.Construction2 || _type == SpriteType.Busy);
  }

  public class Animation
  {
    public BNR.AnimationInfo info;
    public Sprite[] Anim_Sprites;
    public Vector3 position;
    public Vector3 pivot;
    public int soundFrame;
    public string sound;
    public SpriteType type;
    public bool active;
  }
}

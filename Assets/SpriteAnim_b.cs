
using BNR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpriteAnim_b : MonoBehaviour
{
    private string animationKey = string.Empty;
    private string defaultAnimationKey = string.Empty;
    private bool playSound = true;
    private BattleMapCtrl BM;
    private PlayerMap PM;
    private WorldMap WM;
    public int CellNo;
    public int AttackerId;
    public bool flip;
    public SpriteRenderer AnimatedGameObject;
    private AudioSource audioSource;
    private AudioClip attackClip;
    private Dictionary<string, SpriteAnim_b.Animation> entityAnimation;
    private bool loop;
    private Texture2D spriteSheet;
    private Sprite currentSprite;
    public int First_SpriteID;
    private int Cur_SpriteID;
    public int Last_SpriteID;
    private int lines;
    private int left;
    private int top;
    private int frameWidth;
    private int frameHeight;
    private int framesPerLine;
    private int frameIndex;
    private int numFrames;
    private Rect frameRect;
    private GameObject parent;
    private Camera mainCamera;
    private int sheetNo;
    private int frameCount;
    private Sprite[] frameSet;
    private SpriteType loadType;
    private string loadAnimationName;
    private BNR.AnimationInfo loadInfo;

    private void Awake()
    {
        this.AnimatedGameObject = this.GetComponent<SpriteRenderer>();
        this.audioSource = this.GetComponent<AudioSource>();
        this.entityAnimation = new Dictionary<string, SpriteAnim_b.Animation>();
        this.BM = BattleMapCtrl.instance;
        this.PM = PlayerMap.instance;
        this.WM = WorldMap.instance;
        if ((UnityEngine.Object)this.BM != (UnityEngine.Object)null)
        {
            this.parent = this.BM.TopObject;
            this.mainCamera = this.BM.MainCamera;
        }
        else if ((UnityEngine.Object)this.PM != (UnityEngine.Object)null)
        {
            this.parent = this.PM.TopObject;
            this.mainCamera = this.PM.MainCamera;
        }
        else
        {
            this.parent = this.WM.TopObject;
            this.mainCamera = this.WM.MainCamera;
        }
    }

    public EventHandler OnClick { get; set; }

    public Vector3 GetMarkerPosition()
    {
        Vector3 vector3 = new Vector3();
        Vector3 position = this.gameObject.transform.position;
        position.y += (float)((double)this.entityAnimation[this.animationKey].info.height * 1.5 / 100.0);
        return position;
    }

    private void init(BNR.AnimationInfo _info)
    {
        this.frameWidth = _info.width;
        this.frameHeight = _info.height;
        this.numFrames = _info.numOfFrames;
    }

    private void OnMouseUp()
    {
        if (this.OnClick == null)
            return;
        this.OnClick((object)this, new EventArgs());
    }

    public void LoadAnimation(
      Vector3 _position,
      string _animationKey,
      string _animationName,
      bool _default,
      SpriteType _type)
    {
        this.LoadAnimation(_position, _animationKey, _animationName, _default, _type, 0, string.Empty);
    }

    private void LoadAsset(string addressName)
    {
        Addressables.LoadAsset<Texture2D>((object)addressName).Completed += new Action<AsyncOperationHandle<Texture2D>>(this.TextureHandle_Completed);
    }

    private void TextureHandle_Completed(AsyncOperationHandle<Texture2D> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            for (Texture2D texture = handle.Result; (UnityEngine.Object)texture != (UnityEngine.Object)null; texture = Resources.Load<Texture2D>(this.GetFolder(this.loadType) + this.loadAnimationName + "_" + this.sheetNo.ToString()))
            {
                this.framesPerLine = texture.width / this.frameWidth;
                this.lines = texture.height / this.frameHeight;
                int num1 = this.framesPerLine * this.lines;
                for (int index = 0; index < num1; ++index)
                {
                    int num2 = index / this.framesPerLine;
                    int num3 = this.frameWidth * (index % this.framesPerLine);
                    int num4 = this.frameHeight * (this.lines - num2 - 1);
                    if (this.frameCount < this.numFrames)
                        this.frameSet[this.frameCount] = this.loadType != SpriteType.AOE ? (!this.IsBuilding(this.loadType) ? Sprite.Create(texture, new Rect((float)num3, (float)num4, (float)this.frameWidth, (float)this.frameHeight), this.setPivot(this.loadInfo), 100f) : Sprite.Create(texture, new Rect((float)num3, (float)num4, (float)this.frameWidth, (float)this.frameHeight), this.setPivot_B(this.loadInfo), 100f)) : Sprite.Create(texture, new Rect((float)num3, (float)num4, (float)this.frameWidth, (float)this.frameHeight), new Vector2(0.5f, 0.5f), 100f);
                    ++this.frameCount;
                }
                ++this.sheetNo;
            }
        }
        else
        {
            int frameCount = this.frameCount;
        }
    }

    public void LoadAnimation(
      Vector3 _position,
      string _animationKey,
      string _animationName,
      bool _default,
      SpriteType _type,
      int _frameNo,
      string _sound)
    {
        SpriteAnim_b.Animation animation = new SpriteAnim_b.Animation();
        string empty = string.Empty;
        if (this.entityAnimation.ContainsKey(_animationKey))
            return;
        string key = _animationName;
        if (_animationName == "mammothhit")
            _animationName = string.Empty;
        if (key == null)
            key = string.Empty;
        if (!string.IsNullOrEmpty(key) && !GameData.AnimationInfo.ContainsKey(key))
        {
            key = key.ToLower();
            if (key != string.Empty && !GameData.AnimationInfo.ContainsKey(key))
            {
                if (key == "deco_tree_02")
                    key = "deco_tree_2";
                if (key != string.Empty && !GameData.AnimationInfo.ContainsKey(key))
                    return;
            }
        }
        if (GameData.AnimationInfo.ContainsKey(key))
        {
            animation.info = GameData.AnimationInfo[key];
            animation.Name = key;
            if (!AssetCache.compositionAnimation.ContainsKey(key))
            {
                Sprite[] spriteArray = this.initAnimation(_animationKey, _type, _animationName, animation.info);
                AssetCache.compositionAnimation.Add(key, spriteArray);
            }
        }
        else
        {
            animation.info = new BNR.AnimationInfo();
            animation.Name = key;
        }
        animation.type = _type;
        animation.sound = _sound;
        animation.soundFrame = _frameNo;
        this.entityAnimation.Add(_animationKey, animation);
        if (GameData.AnimationInfo.ContainsKey(key))
            animation.position = !this.IsBuilding(_type) ? _position : new Vector3(_position.x, _position.y, 0.0f);
        if (_default)
            this.defaultAnimationKey = _animationKey;
        this.Cur_SpriteID = this.First_SpriteID = 0;
        this.Last_SpriteID = this.numFrames - 1;
        if (this.numFrames == 0)
            this.Last_SpriteID = 5;
        this.frameIndex = 0;
    }

    private Sprite[] initAnimation(
      string _key,
      SpriteType _type,
      string _animationName,
      BNR.AnimationInfo _info)
    {
        int num1 = 0;
        int index1 = 0;
        string empty = string.Empty;
        this.init(_info);
        Sprite[] spriteArray = new Sprite[this.numFrames];
        this.loadAnimationName = _animationName;
        this.loadType = _type;
        this.loadInfo = _info;
        this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(_type) + _animationName + "_0");
        if ((UnityEngine.Object)this.spriteSheet == (UnityEngine.Object)null && _type == SpriteType.Idle_Building)
            this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(SpriteType.Busy) + _animationName + "_0");
        if ((UnityEngine.Object)this.spriteSheet == (UnityEngine.Object)null && _type != SpriteType.Idle_Building)
            this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(SpriteType.Idle_Building) + _animationName + "_0");
        for (; (UnityEngine.Object)this.spriteSheet != (UnityEngine.Object)null; this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(_type) + _animationName + "_" + num1.ToString()))
        {
            this.framesPerLine = this.spriteSheet.width / this.frameWidth;
            this.lines = this.spriteSheet.height / this.frameHeight;
            int num2 = this.framesPerLine * this.lines;
            for (int index2 = 0; index2 < num2; ++index2)
            {
                int num3 = index2 / this.framesPerLine;
                int num4 = this.frameWidth * (index2 % this.framesPerLine);
                int num5 = this.frameHeight * (this.lines - num3 - 1);
                if (index1 < this.numFrames)
                    spriteArray[index1] = _type != SpriteType.AOE ? (!this.IsBuilding(_type) ? Sprite.Create(this.spriteSheet, new Rect((float)num4, (float)num5, (float)this.frameWidth, (float)this.frameHeight), this.setPivot(_info), 100f) : Sprite.Create(this.spriteSheet, new Rect((float)num4, (float)num5, (float)this.frameWidth, (float)this.frameHeight), this.setPivot_B(_info), 100f)) : Sprite.Create(this.spriteSheet, new Rect((float)num4, (float)num5, (float)this.frameWidth, (float)this.frameHeight), new Vector2(0.5f, 0.5f), 100f);
                ++index1;
            }
            ++num1;
        }
        if (index1 == 0)
            spriteArray = new Sprite[0];
        return spriteArray;
    }

    public bool PlayAnimation(string key, bool _loop)
    {
        float num = 1.2f;
        if (!this.entityAnimation.ContainsKey(key) || AssetCache.compositionAnimation[this.entityAnimation[key].Name].Length == 0)
            return false;
        SpriteAnim_b.Animation animation = this.entityAnimation[key];
        this.animationKey = key;
        this.numFrames = animation.info.numOfFrames;
        this.Cur_SpriteID = this.First_SpriteID = 0;
        this.Last_SpriteID = this.numFrames - 1;
        if (this.numFrames == 0)
            this.Last_SpriteID = 5;
        this.attackClip = Resources.Load<AudioClip>("Audio/" + this.entityAnimation[this.animationKey].sound);
        if (this.entityAnimation[this.animationKey].sound != null && this.entityAnimation[this.animationKey].sound != string.Empty && (UnityEngine.Object)this.attackClip == (UnityEngine.Object)null)
            Debug.Log((object)string.Format("Sound file missing: {0}", (object)this.entityAnimation[this.animationKey].sound));
        if (this.IsBuilding(animation.type))
        {
            this.gameObject.transform.position = new Vector3(this.entityAnimation[this.animationKey].position.x - this.setPivot_B(this.entityAnimation[this.animationKey].info).x, this.entityAnimation[this.animationKey].position.y, 0.0f);
            this.AnimatedGameObject.GetComponent<BoxCollider2D>();
        }
        else
        {
            Vector3 position = this.entityAnimation[this.animationKey].position;
            position.z = 0.0f;
            this.gameObject.transform.position = position;
        }
        if (animation.type == SpriteType.AOE)
            num = 2f;
        if (this.IsBuilding(animation.type))
            num = 1f;
        this.gameObject.transform.localScale = new Vector3(num / this.parent.transform.localScale.x, num / this.parent.transform.localScale.y, 0.0f);
        this.loop = _loop;
        this.StopCoroutine("AnimateSprite");
        this.StartCoroutine("AnimateSprite");
        return true;
    }

    public void SetPosition(Vector3 newPos)
    {
        if (this.animationKey == null || !(this.animationKey != string.Empty))
            return;
        Vector2 vector2 = this.setPivot_B(this.entityAnimation[this.animationKey].info);
        this.gameObject.transform.position = new Vector3(newPos.x - vector2.x, newPos.y - vector2.y, 0.0f);
    }

    public void Move(Vector3 _position)
    {
        foreach (string key in this.entityAnimation.Keys)
        {
            Vector2 vector2 = this.setPivot_B(this.entityAnimation[key].info);
            this.entityAnimation[key].position = new Vector3(_position.x - vector2.x, _position.y - vector2.y, 0.0f);
        }
    }

    public void Shift()
    {
        if (this.animationKey == null || !(this.animationKey != string.Empty))
            return;
        Vector3 vector3 = new Vector3(0.0f, (float)this.entityAnimation[this.animationKey].info.height / 100f, 0.0f);
        vector3 = new Vector3(0.0f, 1f, 0.0f);
        this.gameObject.transform.position = new Vector3(this.transform.position.x + vector3.x, this.transform.position.y + vector3.y, 0.0f);
    }

    public SpriteAnim_b.Animation GetAnimation(string key)
    {
        return this.entityAnimation.ContainsKey(key) ? this.entityAnimation[key] : (SpriteAnim_b.Animation)null;
    }

    private Vector2 setPivot(BNR.AnimationInfo _info)
    {
        int num = 0;
        Vector3 vector3 = new Vector3((float)(_info.left / 100), (float)(_info.top / 100 * -1 - (_info.height / 100 - num)), 0.0f);
        vector3.y = (float)(_info.height / 100 - Math.Abs(_info.top / 100) + num);
        return (Vector2)new Vector3(Math.Abs(vector3.x) / (float)(_info.width / 100), Math.Abs(vector3.y / 100f) / (float)(_info.height / 100));
    }

    private Vector2 setPivot_B(BNR.AnimationInfo _info)
    {
        Vector3 vector3 = new Vector3((float)_info.left, (float)(_info.top * -1 - _info.height), 0.0f);
        vector3.y = (float)(_info.height - Math.Abs(_info.top));
        return (Vector2)new Vector3(Math.Abs(vector3.x) / (float)_info.width, Math.Abs(vector3.y) / (float)_info.height);
    }

    private IEnumerator AnimateSprite()
    {
        SpriteAnim_b spriteAnimB = this;
        SpriteAnim_b.Animation animation = spriteAnimB.entityAnimation[spriteAnimB.animationKey];
        double num = 1032.0 / (double)Screen.width;
        yield return (object)new WaitForSeconds(0.03f);
        if (spriteAnimB.numFrames > 0)
        {
            spriteAnimB.AnimatedGameObject.GetComponent<SpriteRenderer>().sprite = AssetCache.compositionAnimation[spriteAnimB.entityAnimation[spriteAnimB.animationKey].Name][spriteAnimB.Cur_SpriteID];
            spriteAnimB.AnimatedGameObject.GetComponent<SpriteRenderer>().flipX = spriteAnimB.flip;
        }
        if (spriteAnimB.numFrames > 1)
        {
            if ((UnityEngine.Object)spriteAnimB.attackClip != (UnityEngine.Object)null && spriteAnimB.entityAnimation[spriteAnimB.animationKey].soundFrame == spriteAnimB.Cur_SpriteID && spriteAnimB.playSound)
            {
                if ((UnityEngine.Object)spriteAnimB.audioSource == (UnityEngine.Object)null)
                    spriteAnimB.audioSource = spriteAnimB.gameObject.AddComponent<AudioSource>();
                spriteAnimB.audioSource.PlayOneShot(spriteAnimB.attackClip, 0.7f);
                spriteAnimB.playSound = false;
            }
            ++spriteAnimB.Cur_SpriteID;
            if (spriteAnimB.Cur_SpriteID > spriteAnimB.Last_SpriteID)
            {
                if (spriteAnimB.loop)
                {
                    spriteAnimB.Cur_SpriteID = spriteAnimB.First_SpriteID;
                    spriteAnimB.StartCoroutine(nameof(AnimateSprite));
                }
                else if (spriteAnimB.defaultAnimationKey != null && spriteAnimB.defaultAnimationKey != string.Empty)
                    spriteAnimB.PlayAnimation(spriteAnimB.defaultAnimationKey, true);
            }
            else
                spriteAnimB.StartCoroutine(nameof(AnimateSprite));
        }
    }

    private string GetFolder(SpriteType type)
    {
        string empty = string.Empty;
        string str;
        switch (type)
        {
            case SpriteType.Default:
                str = "Animations/Buildings/Default/";
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
        public string Name;
        public Vector3 position;
        public Vector3 pivot;
        public int soundFrame;
        public string sound;
        public SpriteType type;
        public bool active;
    }
}

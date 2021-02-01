
using BNR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpriteAnim : MonoBehaviour
{
    private string animationKey = string.Empty;
    private string defaultAnimationKey = string.Empty;
    private BattleMapCtrl BM;
    private PlayerMap PM;
    private WorldMap WM;
    public int DamageIndex;
    public int CellNo;
    public int AttackerId;
    public bool flip;
    public SpriteRenderer AnimatedGameObject;
    public ShakeObject shakeObject;
    private AudioSource audioSource;
    private AudioClip attackClip;
    private Dictionary<string, SpriteAnim.Animation> entityAnimation;
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
    public bool showHit;
    private Color normalColor;
    private Color hitLowColor;
    private Color hitHighColor;
    private int hitCounter;

    public void SOffset(Vector3 _offset)
    {
        Debug.Log((object)string.Format("{0} pos {1},{2}", (object)"Depot", (object)this.entityAnimation[this.animationKey].position.x, (object)this.entityAnimation[this.animationKey].position.y));
        this.entityAnimation[this.animationKey].position += _offset;
        Vector3 worldPoint = this.mainCamera.ScreenToWorldPoint(this.entityAnimation[this.animationKey].position);
        Debug.Log((object)string.Format("{0} pos changed {1},{2}", (object)"Depot", (object)this.entityAnimation[this.animationKey].position.x, (object)this.entityAnimation[this.animationKey].position.y));
        worldPoint.z = 0.0f;
        this.gameObject.transform.position = worldPoint;
    }

    private void Awake()
    {
        this.AnimatedGameObject = this.GetComponent<SpriteRenderer>();
        this.audioSource = this.GetComponent<AudioSource>();
        this.entityAnimation = new Dictionary<string, SpriteAnim.Animation>();
        this.BM = BattleMapCtrl.instance;
        this.PM = PlayerMap.instance;
        this.WM = WorldMap.instance;
        if ((UnityEngine.Object)this.BM != (UnityEngine.Object)null)
        {
            this.parent = this.BM.TopObject;
            this.mainCamera = this.BM.MainCamera;
        }
        else
        {
            this.parent = this.PM.TopObject;
            this.mainCamera = this.PM.MainCamera;
        }
        if ((UnityEngine.Object)this.parent == (UnityEngine.Object)null)
        {
            this.parent = this.WM.TopObject;
            this.mainCamera = this.WM.MainCamera;
        }
        this.normalColor = Functions.GetColor((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
        this.hitLowColor = Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f, 200f);
        this.hitHighColor = Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f, 100f);
    }

    public EventHandler AttackComplete { get; set; }

    public EventHandler DeathComplete { get; set; }

    private void init(BNR.AnimationInfo _info)
    {
        this.frameWidth = _info.width;
        this.frameHeight = _info.height;
        this.numFrames = _info.numOfFrames;
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

    public void LoadAnimation(
      Vector3 _position,
      string _animationKey,
      string _animationName,
      bool _default,
      SpriteType _type,
      int _frameNo,
      string _sound)
    {
        SpriteAnim.Animation animation = new SpriteAnim.Animation();
        if (this.entityAnimation.ContainsKey(_animationKey))
            return;
        if (_animationName == "mammothhit")
            _animationName = string.Empty;
        if (_animationName != string.Empty && !GameData.AnimationInfo.ContainsKey(_animationName))
        {
            _animationName = _animationName.ToLower();
            if (_animationName != string.Empty && !GameData.AnimationInfo.ContainsKey(_animationName))
            {
                Debug.Log((object)string.Format("{0} animation {1} missing", (object)_animationKey, (object)_animationName));
                return;
            }
        }
        if (GameData.AnimationInfo.ContainsKey(_animationName))
        {
            animation.info = GameData.AnimationInfo[_animationName];
            animation.Anim_Sprites = this.initAnimation(_animationKey, _type, _animationName, animation.info);
        }
        else
            animation.info = new BNR.AnimationInfo();
        animation.type = _type;
        animation.sound = _sound;
        animation.soundFrame = _frameNo;
        if (animation.soundFrame < 0)
            animation.soundFrame = 0;
        this.entityAnimation.Add(_animationKey, animation);
        if (GameData.AnimationInfo.ContainsKey(_animationName))
            animation.position = _type != SpriteType.Idle_Building ? _position : new Vector3(_position.x, _position.y, 0.0f);
        if (_default)
            this.defaultAnimationKey = _animationKey;
        this.Cur_SpriteID = this.First_SpriteID = 0;
        this.Last_SpriteID = this.numFrames - 1;
        if (this.numFrames == 0)
            this.Last_SpriteID = 5;
        this.frameIndex = 0;
    }

    private Sprite[] initAnimation1(
      string _key,
      SpriteType _type,
      string _animationName,
      BNR.AnimationInfo _info)
    {
        int num1 = 0;
        int frameCount = 0;
        this.init(_info);
        Sprite[] spriteArray = new Sprite[this.numFrames];
        for (this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(_type) + _animationName + "_0"); (UnityEngine.Object)this.spriteSheet != (UnityEngine.Object)null; this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(_type) + _animationName + "_" + num1.ToString()))
        {
            this.framesPerLine = this.spriteSheet.width / this.frameWidth;
            this.lines = this.spriteSheet.height / this.frameHeight;
            if (_animationName == "raider_front_idle")
            {
                this.framesPerLine = 6;
                this.lines = 7;
            }
            int framesPerLine = this.framesPerLine;
            int lines = this.lines;
            for (int index1 = 0; index1 < this.lines; ++index1)
            {
                for (int index2 = 0; index2 < this.framesPerLine; ++index2)
                {
                    if (_animationName == "raider_front_idleX")
                        Debug.Log((object)string.Format("Processing frame {0}:{1}", (object)num1, (object)frameCount));
                    int num2 = this.frameWidth * index2;
                    int num3 = this.frameHeight * index1;
                    if (frameCount < this.numFrames)
                    {
                        spriteArray[frameCount] = _type != SpriteType.AOE ? Sprite.Create(this.spriteSheet, new Rect((float)num2, (float)num3, (float)this.frameWidth, (float)this.frameHeight), this.setPivot(_info), 100f) : Sprite.Create(this.spriteSheet, new Rect((float)num2, (float)num3, (float)this.frameWidth, (float)this.frameHeight), new Vector2(0.5f, 0.5f), 100f);
                        if (_animationName == "raider_front_idle")
                        {
                            Debug.Log((object)string.Format(" F{4} Grabbed {0}, {1} - ({2},{3})", (object)num2, (object)num3, (object)index2, (object)index1, (object)frameCount));
                            this.saveSprite(spriteArray[frameCount].texture, _animationName, frameCount, new Rect(0.0f, 0.0f, (float)this.frameWidth, (float)this.frameHeight));
                        }
                    }
                    ++frameCount;
                }
            }
            ++num1;
        }
        if (frameCount == 0)
            Debug.Log((object)string.Format("Animation {0} {1} not found", (object)this.GetFolder(_type), (object)_animationName));
        return spriteArray;
    }

    private Sprite[] initAnimation(
      string _key,
      SpriteType _type,
      string _animationName,
      BNR.AnimationInfo _info)
    {
        int num1 = 0;
        int frameCount = 0;
        this.init(_info);
        Sprite[] spriteArray = new Sprite[this.numFrames];
        for (this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(_type) + _animationName + "_0"); (UnityEngine.Object)this.spriteSheet != (UnityEngine.Object)null; this.spriteSheet = Resources.Load<Texture2D>(this.GetFolder(_type) + _animationName + "_" + num1.ToString()))
        {
            this.framesPerLine = this.spriteSheet.width / this.frameWidth;
            this.lines = this.spriteSheet.height / this.frameHeight;
            int num2 = this.framesPerLine * this.lines;
            for (int index = 0; index < num2; ++index)
            {
                if (_animationName == "tf_hero_heavy_back_attack1X")
                    Debug.Log((object)string.Format("Processing frame {0}:{1}", (object)num1, (object)frameCount));
                int num3 = index / this.framesPerLine;
                int num4 = this.frameWidth * (index % this.framesPerLine);
                int num5 = this.frameHeight * (this.lines - num3 - 1);
                if (frameCount < this.numFrames)
                {
                    spriteArray[frameCount] = _type != SpriteType.AOE ? Sprite.Create(this.spriteSheet, new Rect((float)num4, (float)num5, (float)this.frameWidth, (float)this.frameHeight), this.setPivot(_info), 100f) : Sprite.Create(this.spriteSheet, new Rect((float)num4, (float)num5, (float)this.frameWidth, (float)this.frameHeight), new Vector2(0.5f, 0.5f), 100f);
                    if (_animationName == "tf_hero_heavy_back_attack1X")
                    {
                        Debug.Log((object)string.Format("Grabbed {0}, {1} - line {2}", (object)num4, (object)num5, (object)num3));
                        this.saveSprite(spriteArray[frameCount].texture, _animationName, frameCount, new Rect(0.0f, 0.0f, (float)this.frameWidth, (float)this.frameHeight));
                    }
                }
                ++frameCount;
            }
            ++num1;
        }
        if (frameCount == 0)
            Debug.Log((object)string.Format("Animation {0} {1} not found", (object)this.GetFolder(_type), (object)_animationName));
        return spriteArray;
    }

    private void saveSpriteToFile(Sprite sprite, string _animationName, int frameCount)
    {
        byte[] png = sprite.texture.EncodeToPNG();
        File.WriteAllBytes(string.Format("c:/dev/temp/Img/{1}{2}.png", (object)Application.dataPath, (object)_animationName, (object)frameCount), png);
    }

    private void saveSprite(Texture2D texture, string _animationName, int frameCount, Rect tRect)
    {
        RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit((Texture)texture, temporary);
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = temporary;
        Texture2D tex = new Texture2D((int)tRect.width, (int)tRect.height);
        tex.ReadPixels(tRect, 0, 0);
        tex.Apply();
        RenderTexture.active = active;
        RenderTexture.ReleaseTemporary(temporary);
        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(string.Format("c:/dev/temp/Img/{1}_{2}.png", (object)Application.dataPath, (object)_animationName, (object)frameCount), png);
    }

    public void Move(Vector3 pos)
    {
        this.gameObject.transform.position = new Vector3(pos.x, pos.y, 0.0f);
    }

    public void NewCellPosition(Vector3 pos)
    {
        Vector3 vector3 = new Vector3(pos.x, pos.y, 0.0f);
        this.gameObject.transform.position = vector3;
        foreach (SpriteAnim.Animation animation in this.entityAnimation.Values)
            animation.position = vector3;
    }

    public void PlayAnimation(string key, bool _loop)
    {
        float num = 1f;
        SpriteAnim.Animation animation = this.entityAnimation[key];
        this.animationKey = key;
        this.numFrames = animation.info.numOfFrames;
        this.Cur_SpriteID = this.First_SpriteID = 0;
        this.Last_SpriteID = this.numFrames - 1;
        if (this.numFrames == 0)
            this.Last_SpriteID = 5;
        this.attackClip = Resources.Load<AudioClip>("Audio/" + this.entityAnimation[this.animationKey].sound);
        if (this.entityAnimation[this.animationKey].sound != null && this.entityAnimation[this.animationKey].sound != string.Empty && (UnityEngine.Object)this.attackClip == (UnityEngine.Object)null)
            Debug.Log((object)string.Format("Sound file missing: {0}", (object)this.entityAnimation[this.animationKey].sound));
        if (animation.type == SpriteType.Idle_Building)
        {
            this.gameObject.transform.position = new Vector3((float)((double)this.entityAnimation[this.animationKey].position.x / 100.0 - 0.600000023841858), this.entityAnimation[this.animationKey].position.y / 100f, 0.0f);
        }
        else
        {
            Vector3 position = this.entityAnimation[this.animationKey].position;
            position.z = 0.0f;
            this.gameObject.transform.position = position;
        }
        if (animation.type == SpriteType.AOE)
            num = 2f;
        if (animation.type == SpriteType.Idle_Building)
            num = 1f;
        this.gameObject.transform.localScale = new Vector3(num / this.parent.transform.localScale.x, num / this.parent.transform.localScale.y, 0.0f);
        this.loop = _loop;
        this.StopCoroutine("AnimateSprite");
        this.StartCoroutine("AnimateSprite");
    }

    public SpriteAnim.Animation GetAnimation()
    {
        return this.entityAnimation[this.animationKey];
    }

    private Vector2 setPivot(BNR.AnimationInfo _info)
    {
        int num = 50;
        Vector3 vector3 = new Vector3((float)_info.left, (float)(_info.top * -1 - (_info.height - num)), 0.0f);
        vector3.y = (float)(_info.height - Math.Abs(_info.top) + num);
        return (Vector2)new Vector3(Math.Abs(vector3.x) / (float)_info.width, Math.Abs(vector3.y) / (float)_info.height);
    }

    private Vector2 setPivot_B(BNR.AnimationInfo _info)
    {
        Vector3 vector3 = new Vector3((float)_info.left, (float)(_info.top * -1 - _info.height), 0.0f);
        vector3.y = (float)(_info.height - Math.Abs(_info.top));
        return (Vector2)new Vector3(Math.Abs(vector3.x) / (float)_info.width, Math.Abs(vector3.y) / (float)_info.height);
    }

    public IEnumerator AnimateSprite()
    {
        SpriteAnim spriteAnim = this;
        yield return (object)new WaitForSeconds(0.03f);
        SpriteType type = spriteAnim.entityAnimation[spriteAnim.animationKey].type;
        double num = 1032.0 / (double)Screen.width;
        if (spriteAnim.numFrames > 0)
        {
            if ((UnityEngine.Object)spriteAnim.entityAnimation[spriteAnim.animationKey].Anim_Sprites[spriteAnim.Cur_SpriteID] != (UnityEngine.Object)null)
                spriteAnim.AnimatedGameObject.GetComponent<SpriteRenderer>().sprite = spriteAnim.entityAnimation[spriteAnim.animationKey].Anim_Sprites[spriteAnim.Cur_SpriteID];
            if (spriteAnim.entityAnimation[spriteAnim.animationKey].info.animationName == "raider_front_idleX")
                Debug.Log((object)string.Format("Show frame {0}", (object)spriteAnim.Cur_SpriteID));
            SpriteRenderer component = spriteAnim.AnimatedGameObject.GetComponent<SpriteRenderer>();
            component.flipX = spriteAnim.flip;
            if (spriteAnim.showHit)
            {
                if (spriteAnim.hitCounter % 2 == 0)
                    component.color = !(component.color == spriteAnim.hitHighColor) ? spriteAnim.hitHighColor : spriteAnim.hitLowColor;
                ++spriteAnim.hitCounter;
            }
        }
        if ((UnityEngine.Object)spriteAnim.attackClip != (UnityEngine.Object)null && spriteAnim.entityAnimation[spriteAnim.animationKey].soundFrame == spriteAnim.Cur_SpriteID)
            spriteAnim.audioSource.PlayOneShot(spriteAnim.attackClip, 0.7f);
        ++spriteAnim.Cur_SpriteID;
        if (spriteAnim.Cur_SpriteID > spriteAnim.Last_SpriteID)
        {
            if (spriteAnim.loop)
            {
                spriteAnim.Cur_SpriteID = spriteAnim.First_SpriteID;
                spriteAnim.StartCoroutine(nameof(AnimateSprite));
            }
            else
            {
                if (type == SpriteType.Damage && spriteAnim.AttackComplete != null)
                {
                    spriteAnim.AttackComplete((object)spriteAnim, new EventArgs());
                    spriteAnim.AnimatedGameObject.GetComponent<SpriteRenderer>().sprite = (Sprite)null;
                }
                if (type == SpriteType.Attack && spriteAnim.AttackComplete != null)
                {
                    if (spriteAnim.numFrames > 0)
                        spriteAnim.AnimatedGameObject.GetComponent<SpriteRenderer>().sprite = (Sprite)null;
                    spriteAnim.AttackComplete((object)spriteAnim, new EventArgs());
                }
                if ((type == SpriteType.Death || type == SpriteType.Encounter) && spriteAnim.DeathComplete != null)
                    spriteAnim.DeathComplete((object)spriteAnim, new EventArgs());
                if (spriteAnim.defaultAnimationKey != null && spriteAnim.defaultAnimationKey != string.Empty)
                    spriteAnim.PlayAnimation(spriteAnim.defaultAnimationKey, true);
            }
        }
        else
            spriteAnim.StartCoroutine(nameof(AnimateSprite));
    }

    public void HitAnimationOn()
    {
        this.showHit = true;
        this.hitCounter = 0;
        this.shakeObject.StartShake();
    }

    public void HitAnimationOff()
    {
        this.shakeObject.StopShake();
        this.showHit = false;
        this.AnimatedGameObject.GetComponent<SpriteRenderer>().color = this.normalColor;
    }

    private string GetFolder(SpriteType type)
    {
        string empty = string.Empty;
        string str;
        switch (type)
        {
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
            default:
                str = "Animations/";
                break;
        }
        return str;
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
    }
}

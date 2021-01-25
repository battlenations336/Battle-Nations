namespace BNR
{
    public enum EntityType
    {
        Unit,
        Building
    }

    public enum MapSize
    {
        fiveByThree,
        threeByThree
    }

    public enum BattleResult
    {
        Victory,
        Defeat,
        Surrender
    }

    public enum ArmySource
    {
        Player,
        Mission,
        Occupation,
        PvP
    }

    public enum AnimationType
    {
        Idle,
        Busy,
        Attack,
        Death
    }

    public enum BattleMapType
    {
        ThreeByThree,
        ThreeByFive
    }

    public enum HighlightType
    {
        None,
        InRange,
        Selected,
        Damage,
        SplashDamage,
        Hit,
        Splashed,
    }

    public enum BattleType
    {
        Training,
        Encounter,
        Occupation,
        VsFriend,
        VsRandom
    }

    public enum BattleMode
    {
        PlayerSetup,
        Queued,
        Joined,
        PlayerSelect,
        PlayerAttack,
        WaitForPlayerAttack,
        PlayerPostAttack,
        EnemySelect,
        WaitForEnemyAttack,
        EnemyAttack,
        EnemyPostAttack,  
        Paused,
        Surrender,
        Rewards,
        XP,
        Complete
    }

    public enum Resource
    {
        bars,
        chem,
        coal,
        concrete,
        gear,
        heart,
        iron,
        lumber,
        oil,
        sbars,
        sgear,
        skull,
        sskull,
        star,
        steel,
        stone,
        stooth,
        tooth,
        wood
    }

    public enum Category
    {
        bmCat_houses,
        bmCat_shops,
        bmCat_military,
        bmCat_boosts,
        bmCat_resources,
        bmCat_store,
        bmCat_decorations,
        bmCat_expansion
    }

    public enum SpriteType
    {
        Default,
        Idle,
        Attack,
        Damage,
        Death,
        Idle_Building,
        Busy,
        Construction1,
        Construction2,
        AOE,
        Encounter
    }

    public enum WorldMode
    {
        None,
        Dragging,
        Moving,
        Select
    }

    public enum ButtonValue
    {
        OK,
        Cancel,
        Turn,
        Sell,
        String
    }
}

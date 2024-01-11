/// <summary>
/// 资产获得来源
/// </summary>
public static class ResourceGot
{
    public const string initial = "initial";// 初始赠送
    public const string level_pass = "level_pass";//过关时结算的
    public const string level_pass_rv = "level_pass_rv";//过关时观看rv翻倍的金币
    public const string star_chest = "star_chest";//   星星宝箱
    public const string level_chest = "level_chest";//等级宝箱
    public const string daily_bonus = "daily_bonus";//   签到
    public const string clover_treasure = "clover_treasure";  // 四叶草奖励
    public const string shop_rv = "shop_rv";   //商城界面看rv
    public const string debug = "debug";//    后台发放
    public const string piggy = "piggy";//  小猪
    public const string race = "race";  //火箭竞赛
    public const string bonus_level = "bonus_level";//奖励关卡
    public const string bonus_level_rv = "bonus_level_rv";// 奖励关卡观看RV获得的金币
    public const string rocket_treasure_free = "rocket_treasure_free";// 无尽付费活动的免费奖励
    public const string rocket_treasure_iap = "rocket_treasure_iap";//无尽付费活动的付费奖励
    public const string daily_sale = "daily_sale";//
    public const string weekly_contest = "weekly_contest";//    周赛活动奖励
    public const string team_help = "team_help";//  赠送给其他玩家体力时获得的金币
    public const string island = "island";//  海岛连胜活动
    public const string balloon = "balloon";//  气球活动
    public const string hunt = "hunt";// 兑换商城活动
    public const string infinite_life = "infinite_life"; //    从无限体力活的（体力来源专用）

    public const string use_coin = "use_coin_buy";//使用金币购买的
    
    public const string team_request = "team_request";//玩家赠送体力
    public const string team_free_life = "team_free_life";//免费赠送体力

    //付费礼包传礼包id
}

/// <summary>
/// 消耗资产的用途 match只记录金币的
/// </summary>
public static class ResourceUse
{
    public const string buy_life = "buy_life";//     买体力

    public const string buy_undo = "buy_undo";//   买暂停道具

    public const string buy_bomb = "buy_bomb";//    买炸弹道具

    public const string buy_magnifier = "buy_magnifier";//   买放大镜道具

    public const string buy_freeze = "buy_freeze";//    买雪花道具

    public const string buy_lightning = "buy_lightning";//     买闪电道具

    public const string buy_clock = "buy_clock";//    买闹钟道具

    public const string buy_magnet = "buy_magnet";// 买磁铁道具

    public const string buy_unstuck = "buy_unstuck";//     卡死时花费金币

    public const string buy_overtime = "buy_overtime";//     超时时花费金币

    public const string create_team = "create_team";//    创建工会
}
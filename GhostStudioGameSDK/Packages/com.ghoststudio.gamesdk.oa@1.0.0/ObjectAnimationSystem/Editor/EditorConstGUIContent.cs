using UnityEngine;

namespace AppBase.OA.WorldPositionMoveModule.Editor
{
    public class EditorConstGUIContent
    {
        public static GUIContent ConfigAllParts = new GUIContent("所有动画成分：");
        public static GUIContent ConfigTimeScaleTimes = new GUIContent("倍速系数：");
        public static GUIContent ConfigAutoPlay = new GUIContent("自动播放");
        public static GUIContent ConfigDelaySeconds = new GUIContent("延时：");
        
        public static GUIContent ConfigDes = new GUIContent("此配置用于：");
        public static GUIContent ConfigEnabled = new GUIContent("是否启用：");
        public static GUIContent IsBaseDuration = new GUIContent("是否指定为基础时长动画：");
        public static GUIContent ConfigShowPath = new GUIContent("是否显示路径线");
        public static GUIContent ConfigShowWps = new GUIContent("是否显示路标位置信息");

        public static GUIContent ConfigPathType = new GUIContent("选择路径模式(PathType)：",
            "<b>Auto:</b>始末点中间根据下面的参数加一个点作为贝塞尔曲线的三个控制点，自动生成一个平滑路径;\n" +
            "<b>BezierSmoothedWps:</b>Wps中设置3个及以上的路标点，直接作为贝塞尔曲线的控制点，生成平滑路径，此时会忽略startValue和endValue，将以Wps列表中的第一个和最后一个世界坐标点作为始末点\n" +
            "<b>StrictWPs:</b>将严格按照Wps中的位置进行移动");
        
        public static GUIContent ConfigDuration = new GUIContent("时长（duration）");
        public static GUIContent ConfigCurve = new GUIContent("缓动曲线（speedCurve）");
        public static GUIContent ConfigTimeScale = new GUIContent("基准时间缩放系数（timeScale）");
        public static GUIContent ConfigDurationCorrect = new GUIContent("矫正时长策略（duration）");
        public static GUIContent ConfigDelay = new GUIContent("延时多少秒（delayTime）");
        public static GUIContent ConfigHelpAutoStrategy = new GUIContent(
            "自动模式下的策略，默认是在始末点中间再增加一个点作为构成贝塞尔曲线的3个控制点。下面的参数都是为了确定中间这个点的位置而设置的。可以创建不同的策略来提供贝塞尔曲线的控制点");
        public static GUIContent ConfigStartVal = new GUIContent("起点世界坐标");
        public static GUIContent ConfigEndVal = new GUIContent("终点世界坐标");
        public static GUIContent ConfigAutoWeight = new GUIContent("中间控制点权重");
        public static GUIContent ConfigHelpAutoWeight = new GUIContent("y大于0，曲线永远在起点和终点的连线上方");
        public static string ConfigHVStrategy = "如何处理始末点连线呈垂直和水平线的情况";
        
        public static GUIContent ConfigHelpAsVertical = new GUIContent("始末点连线SE与y轴角度小于多少时视为垂直线，需特殊处理");
        public static GUIContent ConfigHelpAsHor = new GUIContent("始末点连线SE与x轴角度小于多少时视为水平线，需特殊处理");
        public static GUIContent ConfigHelpVerticalStrategy = new GUIContent("当始末点连线被视为垂直线时，x为第三个点与起点的连线SM与y轴的夹角（度数），正在右，负在左；y为SM在y轴上的分量/SE在y轴上的分量");
        public static GUIContent ConfigHelpHorStrategy = new GUIContent("当始末点连线被视为水平线时，x为第三个点与起点的连线SM与x轴的夹角（度数），正在，负在下；y为SM在x轴上的分量/SE在x轴上的分量");
        public static GUIContent ConfigHelpAdaptStartEnd = new GUIContent("起始点改变时，路径的自适应策略");
        
        
        public static GUIContent ConfigUseDelayCurve = new GUIContent("使用延迟曲线");
        public static GUIContent ConfigAnimationName = new GUIContent("动画名");
        public static GUIContent ConfigDelayCurve = new GUIContent("当前延迟曲线");
        public static GUIContent ConfigDelayRange = new GUIContent("延迟范围");
        public static GUIContent ConfigHelpDelay = new GUIContent("使用延迟曲线，可以使成员动画有不同的延迟时间。可以通过调整曲线或者延迟范围的数值来达到无延迟或者等距离延迟的效果");
        public static GUIContent ConfigDefaultAnimation = new GUIContent("默认播放的动画");
        public static GUIContent ConfigSimuCamera = new GUIContent("将要操作的Camera拖到此处");
        
        public static GUIContent ConfigUseSeperateAxis = new GUIContent("是否分轴设置曲线");
        public static GUIContent ConfigXAxis = new GUIContent("x轴曲线");
        public static GUIContent ConfigYAxis = new GUIContent("y轴曲线");
        public static GUIContent ConfigZAxis = new GUIContent("z轴曲线");
        public static GUIContent ConfigConbinedAxis = new GUIContent("统一曲线");
        public static GUIContent ConfigPositionType = new GUIContent("位置类型");
        public static GUIContent ConfigForceConfigPosition = new GUIContent("强制使用配置中的位置");
        public static GUIContent ConfigUseUnscaledDeltaTime = new GUIContent("不受时间缩放影响");
        
    }
}
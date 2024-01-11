using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

//避免直接引用SRDebugger中的属性，减少HotfixAsm的引用数
namespace AppBase.Debugging
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class DisplayNameAttribute : PreserveAttribute
    {
        public readonly string DisplayName;
        public readonly int SortPriority;

        public DisplayNameAttribute(string displayName, [CallerLineNumber]int sortPriority = 0)
        {
            DisplayName = displayName;
            SortPriority = sortPriority;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class CategoryAttribute : PreserveAttribute
    {
        public readonly string Category;

        public CategoryAttribute(string category)
        {
            Category = category;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class NumberRangeAttribute : PreserveAttribute
    {
        public readonly double Max;
        public readonly double Min;

        public NumberRangeAttribute(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IncrementAttribute : PreserveAttribute
    {
        public readonly double Increment;

        public IncrementAttribute(double increment)
        {
            Increment = increment;
        }
    }
}
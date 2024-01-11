//using System;
//using BetaFramework.obb;

//public class TkObbDownloadListener:ObbDownloadListener
//{
//    public Action<int> stateChanged;
//    public Action<long,long,long,float> progressChanged;
//    public void onDownloadStateChanged(int newState)
//    {
//        if (stateChanged != null)
//        {
//            stateChanged.Invoke(newState);
//        }
//    }

//    public void onDownloadProgress(long overallTotal, long overallProgress, long timeRemaining, float currentSpeed)
//    {
//        if (progressChanged != null)
//        {
//            progressChanged.Invoke(overallTotal, overallProgress, timeRemaining, currentSpeed);
//        }
//    }

//    public TkObbDownloadListener(Action<int> stateChanged,Action<long,long,long,float> progressChanged)
//    {
//        this.stateChanged = stateChanged;
//        this.progressChanged = progressChanged;
//    }
    
//}
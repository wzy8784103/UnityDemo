using System;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// 时间管理表现层，不和逻辑层往一起写，太乱
/// </summary>
public class TimeShowManager : SingletonBase<TimeShowManager>
{
    private Dictionary<long, TimeTextInfo> timeTextDict = new Dictionary<long, TimeTextInfo>();
    private Dictionary<long, TimeSliderInfo> timeSliderDict = new Dictionary<long, TimeSliderInfo>();
    public override void InitSingleton()
    {
        ScriptBridge.Singleton.AddUpdate(Update, null);
    }
    public void CreateTimeText(long key, Text text, string endShow = "", string followStr = "")
    {
        if(!timeTextDict.ContainsKey(key))
        {
            TimeTextInfo info = new TimeTextInfo();
            info.endShow = endShow;
            info.followStr = followStr;
            timeTextDict.Add(key, info);
        }
        timeTextDict[key].labelSet.Add(text);
    }

    public void CreateTimeSlider(long key, Slider slider, long totalTime)
    {
        if (!timeSliderDict.ContainsKey(key))
        {
            TimeSliderInfo info = new TimeSliderInfo();
            info.totalTime = totalTime;
            timeSliderDict.Add(key, info);
        }
        timeSliderDict[key].sliderSet.Add(slider);
    }

    private HashSet<long> removeSet = new HashSet<long>();

    private void Update()
    {
        foreach(var kv in timeTextDict)
        {
            TimeInfo info = TimeManager.Singleton.GetTimeInfo(kv.Key);
            if(info != null)
            {
                foreach(var item in kv.Value.labelSet)
                {
                    string showTime = GetShowTime(info.endTime - TimeTools.GetCurMilliseconds());
                    UITools.SetText(item, kv.Value.followStr == "" ? showTime : string.Format(kv.Value.followStr, showTime));
                }
            }
            else
            {
                foreach (var item in kv.Value.labelSet)
                {
                    //第一次时间不存在时，则代表结束，设置结束str
                    UITools.SetText(item, kv.Value.endShow);
                }
                removeSet.Add(kv.Key);
            }
        }
        foreach (var kv in timeSliderDict)
        {
            TimeInfo info = TimeManager.Singleton.GetTimeInfo(kv.Key);
            if (info != null)
            {
                foreach (var item in kv.Value.sliderSet)
                {
                    UITools.SetSlider(item, 1 - (info.endTime - TimeTools.GetCurMilliseconds()) / (float)kv.Value.totalTime);
                }
            }
            else
            {
                removeSet.Add(kv.Key);
            }
        }
        if (removeSet.Count > 0)
        {
            foreach(var item in removeSet)
            {
                if(timeTextDict.ContainsKey(item))
                {
                    timeTextDict.Remove(item);
                }
                if (timeSliderDict.ContainsKey(item))
                {
                    timeSliderDict.Remove(item);
                }
            }
            removeSet.Clear();
        }
    }

    public static string GetShowTime(long parmTime)
    {
        //做个矫正,四舍五入
        long finalMSecond = (long)Math.Round(parmTime / 1000d) * 1000;
        //大于一天
        long days = finalMSecond / ( TimeSpan.TicksPerDay / TimeSpan.TicksPerMillisecond);
        TimeSpan span = new TimeSpan(finalMSecond * TimeSpan.TicksPerMillisecond);
        //这里必须用\\ 暂时不知道为什么 datetime就不用
        return (days > 0 ? (days.ToString() + "D ") : "") + span.ToString("hh\\:mm\\:ss");
    }
}

public class TimeTextInfo
{
    //跟随的文本，比如有些倒计时前面要加一些字，由于多语言的影响无法定死位置，合成一个text锚点直接定为中心比较方便
    //注意需要是格式化格式，即必须要带{0}
    public string followStr = "";
    //倒计时结束后显示的文本
    public string endShow = "";
    public HashSet<Text> labelSet = new HashSet<Text>();
}
public class TimeSliderInfo
{
    //总时间，做进度条用
    public long totalTime;
    public HashSet<Slider> sliderSet = new HashSet<Slider>();
}
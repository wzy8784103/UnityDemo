
public class SettingModel : SingletonBase<SettingModel>
{
    #region 声音相关
    private int backGroundVolume;
    private bool backGroundMute;
    private int effectVolume;
    private bool effectMute;
    private int talkVolume;
    private bool talkMute;
    public int BackGroundVolume { get => backGroundVolume; set => backGroundVolume = value; }
    public bool BackGroundMute { get => backGroundMute; set => backGroundMute = value; }
    public int EffectVolume { get => effectVolume; set => effectVolume = value; }
    public bool EffectMute { get => effectMute; set => effectMute = value; }
    public int TalkVolume { get => talkVolume; set => talkVolume = value; }
    public bool TalkMute { get => talkMute; set => talkMute = value; }
    #endregion
}

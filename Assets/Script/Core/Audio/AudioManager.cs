using UnityEngine;

public class AudioManager : SingletonBase<AudioManager>
{
    private GameObject parentObj;
    //背景音乐
    private AudioSource backGroundAudio;
    //音效
    private AudioSource effectAudio;
    //人物对话，比如引导什么的
    private AudioSource talkAudio;
    public override void InitSingleton()
    {
        parentObj = new GameObject();
        parentObj.name = "MainAudioListener";
        parentObj.AddComponent<AudioListener>();
        backGroundAudio = CreateAudioSource("BackGround", parentObj, true);
        effectAudio = CreateAudioSource("Effect", parentObj, false);
        talkAudio = CreateAudioSource("Talk", parentObj, false);
    }
	public override void OnRelease()
	{
		if(parentObj != null)
		{
            GameObject.Destroy(parentObj);
		}
	}
	private AudioSource CreateAudioSource(string name, GameObject parent, bool isLoop)
    {
        GameObject obj = new GameObject();
        obj.name = name;
        obj.transform.parent = parent.transform;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.rolloffMode = AudioRolloffMode.Custom;
        source.loop = isLoop;
        return source;
    }
    
    public void PlayBackGround(string path)
    {
        if (SettingModel.Singleton.BackGroundMute)
        {
            return;
        }
        AudioClip audioClip = AssetManager.AssetLoad.LoadClearAsset<AudioClip>(path);
        AudioSource source = backGroundAudio.GetComponent<AudioSource>();
        source.clip = audioClip;
        source.volume = SettingModel.Singleton.BackGroundVolume;
        source.Play();
    }
    public void PlayEffect(string path)
    {
        if(SettingModel.Singleton.EffectMute)
        {
            return;
        }
        AudioClip audioClip = AssetManager.AssetLoad.LoadClearAsset<AudioClip>(path);
        AudioSource source = effectAudio.GetComponent<AudioSource>();
        source.volume = SettingModel.Singleton.EffectVolume;
        source.PlayOneShot(audioClip);
    }
    public void PlayTalk(string path)
    {
        if (SettingModel.Singleton.TalkMute)
        {
            return;
        }
        AudioClip audioClip = AssetManager.AssetLoad.LoadClearAsset<AudioClip>(path);
        AudioSource source = talkAudio.GetComponent<AudioSource>();
        source.volume = SettingModel.Singleton.TalkVolume;
        source.PlayOneShot(audioClip);
    }
    public void PauseBackGround()
    {
        AudioSource source = backGroundAudio.GetComponent<AudioSource>();
        source.Pause();
    }
}

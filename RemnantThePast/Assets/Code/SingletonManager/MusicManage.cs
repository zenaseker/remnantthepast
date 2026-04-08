using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

/// <summary>
/// 音频管理器
/// </summary>
public class MusicManage : Singleton<MusicManage>
{
    public AudioSource music;
    public GameObject effectaudio;
    Dictionary<string, GameObject> Sourse = new Dictionary<string, GameObject>();
    Dictionary<string,AudioClip> Audioes = new Dictionary<string, AudioClip>();
    public Queue<string> LaterMusic = new Queue<string>();
    public Queue<bool> LaterMusicLoop = new Queue<bool>();
    protected override void OnAwake()
    {
        base.OnAwake();
        GameObject.DontDestroyOnLoad(this.gameObject);
    }
    public void FixedUpdate()
    {
        if (!music.isPlaying && LaterMusic.Count > 0)
        {
            music.clip = GetAudio(LaterMusic.Dequeue());
            music.loop = LaterMusicLoop.Dequeue();
            music.Play();
        }
    }
    /// <summary>
    /// 立即更新BGM
    /// </summary>
    /// <param name="BGM"></param>
    /// <param name="loop"></param>
    public void ChangeBGM(string BGM,bool loop)
    {
        Debug.Log("ChangeBGM:" + BGM);
        music.clip = GetAudio(BGM);
        music.loop = loop;
        music.Play();
    }
    /// <summary>
    /// 在上个音频播放完毕后播放BGM
    /// </summary>
    /// <param name="BGM"></param>
    /// <param name="loop"></param>
    public void LaterBGM(string BGM,bool loop)
    {
        Debug.Log("LaterBGMAdd:" + BGM);
        LaterMusic.Enqueue(BGM);
        LaterMusicLoop.Enqueue(loop);
    }
    /// <summary>
    /// 播放临时音频
    /// </summary>
    /// <param name="effect">音频</param>
    /// <param name="name">音频id，同id的音频不会重复播放。未设置时以effect为id</param>
    public void PlayEffect(string effect,string name = null)
    {
        if (name == null) name = effect;
        GameObject effectobj;
        if (Sourse.TryGetValue(name, out effectobj))
        {
            if (effectobj.activeSelf)
            {
                return;
            }
        }
        else
        {
            effectobj = GameObject.Instantiate(effectaudio, this.transform);
            effectobj.name = name;
            Sourse.Add(name, effectobj);
        }
        effectobj.SetActive(true);
        AudioSource audio = effectobj.GetComponent<AudioSource>();
        audio.clip = GetAudio(effect);
        audio.Play();
        TimeDestory.Instance.Add(effectobj, audio.clip.length, TimeDestory.EndExecute.DisActive);
    }
    /// <summary>
    /// 播放随机音频
    /// </summary>
    /// <param name="effects"></param>
    public void PlayRamdonEffect(params string[] effects)
    {
        int randomIndex = Random.Range(0, effects.Length);
        PlayEffect(effects[randomIndex]);
    }
    public void PlayEffectHasTag(string effects, string tag)
    {
        List<GameObject> values = Sourse.Values.Where(x => x.name == tag && x.activeSelf).ToList();
        if (values.Count > 0) return;
        PlayEffect(effects,tag);
    }
    public void PlayRamdonEffectHasTag(string tag, params string[] effects)
    {
        List<GameObject> values = Sourse.Values.Where(x => x.name == tag && x.activeSelf).ToList();
        if (values.Count > 0) return;
        int randomIndex = Random.Range(0, effects.Length);
        PlayEffect(effects[randomIndex],tag);
    }
    /// <summary>
    /// 停止或继续BGM
    /// </summary>
    /// <param name="flag"></param>
    public void BGMCheck(bool flag)
    {
        if (flag)
        {
            music.Play();
        }
        else
        {
            music.Pause();
        }
    }
    /// <summary>
    /// 停止所有临时音频
    /// </summary>
    public void StopAllEffect()
    {
        for(int i = 0;i < transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    public AudioClip GetAudio(string name)
    {
        if (!Audioes.ContainsKey(name))
        {
            Audioes.Add(name, Resources.Load<AudioClip>("Audio/" + name));
        }
        return Audioes[name];
    }
}

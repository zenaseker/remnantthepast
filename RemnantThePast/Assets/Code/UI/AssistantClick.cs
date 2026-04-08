using Spine;
using Spine.Unity;
using UnityEngine;

public class AssistantClick : MonoBehaviour
{
    public SkeletonAnimation Spine;
    float waittime = 0f; 
    private bool isPlayingSpecial = false;
    private TrackEntry currentSpecialEntry;
    private void Start()
    {
        Spine.state.SetAnimation(0, "Start", false);
        Spine.state.AddAnimation(0, "Idle", true, 1f);
        MusicManage.Instance.PlayEffectHasTag("voice/char_4134_cetsyr_epoque#50/CN_042", "Assistant");
    }
    private void Update()
    {
        waittime += Time.deltaTime;
        if (waittime > 60f)
        {
            PlaySpecialAnimation("Interact");
            MusicManage.Instance.PlayEffectHasTag("voice/char_4134_cetsyr_epoque#50/CN_010", "Assistant");
            waittime = 0f;
        }
    }
    public void OnClick()
    {
        PlaySpecialAnimation("Special");
        MusicManage.Instance.PlayRamdonEffectHasTag("Assistant", "voice/char_4134_cetsyr_epoque#50/CN_002",
            "voice/char_4134_cetsyr_epoque#50/CN_003",
            "voice/char_4134_cetsyr_epoque#50/CN_004",
            "voice/char_4134_cetsyr_epoque#50/CN_007",
            "voice/char_4134_cetsyr_epoque#50/CN_008",
            "voice/char_4134_cetsyr_epoque#50/CN_009");
        waittime = 0f;
    }
    public void PlaySpecialAnimation(string an)
    {
        if (isPlayingSpecial) return;
        isPlayingSpecial = true;
        currentSpecialEntry = Spine.state.SetAnimation(0, an, false);
        currentSpecialEntry.MixDuration = 0.5f;
        currentSpecialEntry.Complete += OnSpecialAnimationComplete;
    }
    private void OnSpecialAnimationComplete(TrackEntry entry)
    {
        if (entry != currentSpecialEntry) return;
        entry.Complete -= OnSpecialAnimationComplete;
        currentSpecialEntry = Spine.state.SetAnimation(0, "Idle", true);
        currentSpecialEntry.MixDuration = 0.5f;
        isPlayingSpecial = false;
        currentSpecialEntry = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSSC;

public class UIControlsForOSSCExample : MonoBehaviour
{

    public SoundController soundController;
    public PlaySoundSettings beatLoop;
    public PlaySoundSettings coins;
    public PlaySoundSettings effectSequence;
    public PlaySoundSettings minigun;
    public PlaySoundSettings coinRandomPitch;
    public PlaySoundSettings coinRandomVolume;
    
    private ISoundCue beatLoopCue;
    private ISoundCue coinsCue;
    private ISoundCue minigunCue;
    private ISoundCue effectSequenceCue;

    public void PlayBeatLoop() {
        beatLoopCue = soundController.Play(beatLoop);
    }

    public void PauseBeatLoop() {
        beatLoopCue.Pause();
    }

    public void ResumeBeatLoop() {
        beatLoopCue.Resume();
    }

    public void PlayRandomCoin() {
        coinsCue = soundController.Play(coins);
    }

    public void PauseRandomCoin() {
        coinsCue.Pause();
    }

    public void ResumeRandomCoin() {
        coinsCue.Resume();
    }

    public void PlayEffectSequence() {
        effectSequenceCue = soundController.Play(effectSequence);
    }

    public void PauseEffectSequence() {
        effectSequenceCue.Pause();
    }

    public void ResumeEffectSequence() {
        effectSequenceCue.Resume();
    }

    public void StopEffectSequence() {
        effectSequenceCue?.StopSequence();
    }

    public void PlayMinigun() {
        minigunCue = soundController.Play(minigun);
    }

    public void PauseMinigun() {
        minigunCue.Pause();
    }

    public void ResumeMinigun() {
        minigunCue.Resume();
    }

    public void StopMinigun() {
        minigunCue?.StopSequence();
    }

    public void PlayRandomPitch() {
        minigunCue = soundController.Play(coinRandomPitch);
    }

    public void PlayRandomVolume() {
        minigunCue = soundController.Play(coinRandomVolume);
    }

    public void StopAll() {
        soundController.StopAll();
    }
}

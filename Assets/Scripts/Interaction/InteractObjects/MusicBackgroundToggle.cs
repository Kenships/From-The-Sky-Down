using Interaction.InteractObjects;
using Interaction.Interfaces;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicBackgroundToggle : InteractObjectBase {
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private string NameOverride;
    AudioSource soundEffect;
    bool musicPlaying = true;

    private void Start() {
        soundEffect = GetComponent<AudioSource>();
        Name = NameOverride;
    }
    public override void Interact() {
        base.Interact();

        if (soundEffect != null) {
            soundEffect.clip = musicClip;
            if (musicPlaying) {
                soundEffect.Stop();
                musicPlaying = false;
                Debug.Log("music stopped");
            } else {
                soundEffect.Play();
                musicPlaying = true;
                Debug.Log("music playing");
            }

        }
        else {
            Debug.Log("error: music not working properly");
        }

    }

}

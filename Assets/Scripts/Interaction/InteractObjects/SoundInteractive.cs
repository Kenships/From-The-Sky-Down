using Interaction.InteractObjects;
using Interaction.Interfaces;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundInteractive : InteractObjectBase
{
    [SerializeField] private AudioClip soundClip;
    [SerializeField] private string NameOverride;
    AudioSource soundEffect;

    private void Start() {
        soundEffect = GetComponent<AudioSource>();
        Name = NameOverride;
    }
    public override void Interact() {
        base.Interact();
        
        if (soundEffect != null) {
            soundEffect.Stop();
            soundEffect.clip = soundClip;
            soundEffect.Play();
            Debug.Log("sound effect played");
        } else {
            Debug.Log("error: sound effect not played");
        }

    }

}

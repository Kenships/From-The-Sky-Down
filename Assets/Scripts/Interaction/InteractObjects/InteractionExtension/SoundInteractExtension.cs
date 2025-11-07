using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundInteractExtension : InteractExtensionBase {

    [SerializeField] private AudioClip musicClip;
    AudioSource soundEffect;

    protected override void Start() {
        base.Start();

    }

    private void Awake() {
        soundEffect = GetComponent<AudioSource>();
    }
    protected override void OnInteract() {
        if (soundEffect != null) {
            soundEffect.Stop();
            soundEffect.clip = musicClip;
            soundEffect.Play();
            Debug.Log("extension sound effect played");
        } else {
            Debug.Log("error: extension sound effect not played");
        }
    }

    protected override void OnCancelInteract() {
        
    }
}

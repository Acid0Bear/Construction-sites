using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSoundEffect : MonoBehaviour
{
    [SerializeField] private AudioClip ContactClip = null;
    [SerializeField] private float ColBorder = 5;

    private AudioSource src = null;

    private void Awake()
    {
        src = this.gameObject.AddComponent<AudioSource>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > ColBorder)
        {
            if (ContactClip == null) { Debug.LogWarning("Contact clip is not assigned on " + this.gameObject); return; }
            src.volume = PlayerPrefs.GetFloat("CarsVolume");
            src.clip = ContactClip;
            src.Play();
        } 
    }
}

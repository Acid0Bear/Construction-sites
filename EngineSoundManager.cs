using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EngineSoundManager : MonoBehaviour
{
    [Header("Multiplying maxPitch on this value")]
    [SerializeField] private float MaxPitchMultiplayer = 1.5f;
    [Header("Vehicle idling lower border")]
    [Range(0.0f, 3.0f)]
    [SerializeField] private float minPitch = 0.7f;
    [Header("Vehicle pitch changing speed")]
    [Range(0.0f, 0.1f)]
    [SerializeField] private float pitchSpeed = 0.05f;
    [Header("Engine start clip")]
    [SerializeField] private AudioClip EngineStart = null;
    [Header("Engine clip")]
    [SerializeField] private AudioClip EngineRolling = null;
    [Header("Parts clip")]
    [SerializeField] private AudioClip PartsRolling = null;
    [Header("Parts clip")]
    [SerializeField] private AudioClip PartAtCap = null;
    [Header("Backward sound clip")]
    [SerializeField] private AudioClip BackwardSound = null;

    private float VehicleSpeed => (M_Controlller is HeliController)? ((HeliController)M_Controlller).HeliSpeed : this.transform.parent.GetComponentInChildren<WheelJoint2D>().motor.motorSpeed;
    private Controller M_Controlller => GetComponentInParent<Controller>();
    private AudioSource _source = null;
    private AudioSource FXsource = null;
    private AudioSource PartsSource = null;
    private AudioSource BackwardSource = null;
    void Start()
    {
        _source = this.gameObject.AddComponent<AudioSource>();
        PartsSource = this.gameObject.AddComponent<AudioSource>();
        FXsource = this.gameObject.AddComponent<AudioSource>();
        BackwardSource = this.gameObject.AddComponent<AudioSource>();
        _source.pitch = 1;
        if (PlayerPrefs.HasKey("CarsVolume"))
        {
            _source.volume = PlayerPrefs.GetFloat("CarsVolume");
            PartsSource.volume = PlayerPrefs.GetFloat("CarsVolume");
            FXsource.volume = PlayerPrefs.GetFloat("CarsVolume");
            BackwardSource.volume = PlayerPrefs.GetFloat("CarsVolume");
        }
        else
        {
            PlayerPrefs.SetFloat("CarsVolume", 0.5f);
            _source.volume = PlayerPrefs.GetFloat("CarsVolume");
            PartsSource.volume = PlayerPrefs.GetFloat("CarsVolume");
            FXsource.volume = PlayerPrefs.GetFloat("CarsVolume");
            BackwardSource.volume = PlayerPrefs.GetFloat("CarsVolume");
        }

        BackwardSource.loop = true;

        PartsSource.Stop();
        PartsSource.loop = true;
        if (PartsRolling != null)
            PartsSource.clip = PartsRolling;
        _source.clip = EngineStart;
        _source.Play();
    }

    public void PartIsAtCapSound()
    {
        FXsource.Stop();
        FXsource.volume = PlayerPrefs.GetFloat("CarsVolume");
        FXsource.clip = PartAtCap;
        FXsource.Play();
    }

    void Update()
    {
        #region Main Engine Sector
        _source.volume = PlayerPrefs.GetFloat("CarsVolume");
        if (M_Controlller != Controller.M_Controller && !M_Controlller.Sides.Contains(Controller.M_Controller))
        {
            if (_source.isPlaying)
            {
                _source.Stop();
                _source.pitch = minPitch;
            }   
            if(PartsSource.isPlaying)
                PartsSource.Stop();
            if (BackwardSource.isPlaying)
                BackwardSource.Stop();
            if (FXsource.isPlaying)
                FXsource.Stop();
            return;
        }

        if (_source.clip == EngineStart && _source.isPlaying) return;
        
        if (!_source.isPlaying || _source.clip == null)
        {
            _source.clip = EngineRolling;
            _source.Play();
        }
        if (M_Controlller.Directional != 0 || M_Controlller.XDirectional != 0)
        {
            _source.pitch = Mathf.Lerp(_source.pitch, minPitch + Mathf.Abs(VehicleSpeed / 100) * MaxPitchMultiplayer, pitchSpeed);
        }
        else if (M_Controlller.Directional == 0 && M_Controlller.XDirectional == 0)
            _source.pitch = Mathf.Lerp(_source.pitch, minPitch, pitchSpeed);
        #endregion

        #region Parts Engine Sector
        PartsSource.volume = PlayerPrefs.GetFloat("CarsVolume");
        if (!PartsSource.isPlaying && (M_Controlller.XParam != 0 || M_Controlller.YParam != 0 || M_Controlller.AdditionalParam != 0))
        {
            PartsSource.Play();
        }
        else if (M_Controlller.XParam == 0 && M_Controlller.YParam == 0 && M_Controlller.AdditionalParam == 0)
            PartsSource.Stop();
        #endregion

        #region BackwardsSound
        if(!BackwardSource.isPlaying && (M_Controlller.Directional * M_Controlller.GetFacingDir() < 0))
        {
            BackwardSource.volume = PlayerPrefs.GetFloat("CarsVolume");
            BackwardSource.clip = BackwardSound;
            BackwardSource.Play();
        }
        else if(M_Controlller.Directional * M_Controlller.GetFacingDir() >= 0)
        {
            BackwardSource.Stop();
        }
        #endregion
    }
}


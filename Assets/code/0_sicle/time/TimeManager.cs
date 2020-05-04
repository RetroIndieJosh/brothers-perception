using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PostProcessing;

public class TimeManager : MonoBehaviour {
    static public TimeManager instance = null;

    [SerializeField]
    private float m_startHour = 12.0f;

    [SerializeField]
    private float m_hourLengthSec = 10.0f;

    [SerializeField]
    private float m_sunriseStart = 7.5f;

    [SerializeField]
    private float m_sunriseLength = 1.0f;

    [SerializeField]
    private float m_sunsetStart = 21.5f;

    [SerializeField]
    private float m_sunsetLength = 1.0f;

    [SerializeField]
    private float m_minVignetteIntensity = 0.1f;

    [SerializeField]
    private float m_maxVignetteIntensity = 0.5f;

    public UnityEvent OnSunrise;
    public UnityEvent OnSunset;

    public float SunsetLengthSec {  get { return m_sunsetLength * m_hourLengthSec; } }
    public float SunriseLengthSec {  get { return m_sunriseLength * m_hourLengthSec; } }
    public bool IsDaytime { get { return m_hoursElapsedToday > m_sunriseStart && m_hoursElapsedToday < SunsetEnd; } }
    public bool IsSunrise { get { return m_hoursElapsedToday > m_sunriseStart && m_hoursElapsedToday < SunriseEnd; } }
    public bool IsSunset { get { return m_hoursElapsedToday > m_sunsetStart && m_hoursElapsedToday < SunsetEnd; } }

    public float SunrisePercent {
        get {
            if ( !IsSunrise ) return IsDaytime ? 1.0f : 0.0f;
            var diff = SunriseEnd - m_hoursElapsedToday;
            return 1.0f - diff / m_sunriseLength;
        }
    }

    public float SunsetPercent {
        get {
            if ( !IsSunset ) return IsDaytime ? 0.0f : 1.0f;
            var diff = SunsetEnd - m_hoursElapsedToday;
            return 1.0f - diff / m_sunsetLength;
        }
    }

    public float Day { get; private set; }
    public string TimeString { get { return string.Format( "{0,2:00}:{1,2:00}", Hour, Minute ); } }

    private float SunriseEnd { get { return m_sunriseStart + m_sunriseLength; } }
    private float SunsetEnd { get { return m_sunsetStart + m_sunsetLength; } }

    private float Hour { get { return Mathf.Floor( m_hoursElapsedToday ); } }
    private float Minute { get { return ( m_hoursElapsedToday - Hour ) * 60.0f; } }

    private float m_hoursElapsedToday = 0.0f;

    private void Awake() {
        if( instance != null ) {
            Destroy( gameObject );
            Debug.LogErrorFormat( "Duplicate TimeManager in {0}", gameObject );
            return;
        }
        instance = this;

        m_hoursElapsedToday = m_startHour;
        Day = 0;
        if ( IsDaytime ) SetVignetteIntensity( 0.0f );
        else SetVignetteIntensity( 1.0f );
    }

    bool m_didSunrise = false;
    bool m_didSunset = false;

    private void Update() {
        m_hoursElapsedToday += Time.deltaTime / m_hourLengthSec;
        /*
        Debug.LogFormat( "Current time {0,2:00}:{1,2:00} | {2}% sunrise | {3}% sunset | {4}", Hour, Minute,
            Mathf.Floor( SunrisePercent * 100 ), Mathf.Floor( SunsetPercent * 100 ), 
            IsDaytime ? "Daytime" : "Nighttime");
            */

        if ( IsSunrise ) {
            if( !m_didSunrise) {
                m_didSunrise = true;
                OnSunrise.Invoke();
            }
            SetVignetteIntensity( 1.0f - SunrisePercent );
        } else if ( IsSunset ) {
            if( !m_didSunset) {
                m_didSunset = true;
                OnSunset.Invoke();
            }
            SetVignetteIntensity( SunsetPercent );
        }

        if ( m_hoursElapsedToday > 24.0f ) {
            m_didSunrise = m_didSunset = false;
            m_hoursElapsedToday -= 24.0f;
            ++Day;
        }
    }

    private void SetVignetteIntensity(float a_intensityPercent ) {
        var profile = Camera.main.GetComponent<PostProcessingBehaviour>().profile;
        var settings = profile.vignette.settings;
        var range = m_maxVignetteIntensity - m_minVignetteIntensity;
        settings.intensity = a_intensityPercent * range + m_minVignetteIntensity;
        profile.vignette.settings = settings;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Song : MonoBehaviour {
    [System.Serializable]
    class Stem
    {
        public AudioClip clip = null;
        public bool isInitiallyActive = false;
    }

    [SerializeField]
    private List<Stem> m_stemList = new List<Stem>();

    [SerializeField]
    private float m_maxVolume = 0.7f;

    public float MaxVolume {  get { return m_maxVolume; } }

    [SerializeField]
    private float m_bpm = 120;

    [SerializeField]
    private float m_beatsPerBar = 4;

    [SerializeField]
    private bool m_loop = true;
    public bool Loop { set { m_loop = value; } }

    public IntEvent OnBar = new IntEvent();
    public IntEvent OnBeat = new IntEvent();
    public UnityEvent OnLoop = new UnityEvent();
    public UnityEvent OnEnd = new UnityEvent();
    public UnityEvent OnPlay = new UnityEvent();

    public int Id { get { return SongManager.instance.GetSongId( this ); } }
    public float Volume {
        set {
            foreach ( var audioSource in m_audioSourceList ) {
                if ( audioSource.volume > 0.0f ) audioSource.volume = value;
            }
        }
    }

    private List<AudioSource> m_audioSourceList = new List<AudioSource>();

    private int m_curBar = 1;
    private int m_curBeat = 1;
    private float m_songLengthSec = 0;

    private float m_timeElapsed = 0.0f;
    private float m_secPerBeat = 0.0f;

    public bool IsPlaying { get; private set; }

    public void ActivateStem( int a_stemIndex ) {
        if( a_stemIndex < 0 || a_stemIndex >= m_audioSourceList.Count ) {
            Debug.LogErrorFormat( "Tried to activate stem {0} in {1} but index is invalid.", a_stemIndex, this );
            return;
        }
        m_audioSourceList[a_stemIndex].volume = m_maxVolume;
    }

    public void ClearEvents() {
        OnBar.RemoveAllListeners();
        OnBeat.RemoveAllListeners();
        OnEnd.RemoveAllListeners();
        OnLoop.RemoveAllListeners();
        OnPlay.RemoveAllListeners();
    }

    public void DeactivateStem( int a_stemIndex ) {
        if( a_stemIndex < 0 || a_stemIndex >= m_audioSourceList.Count ) {
            Debug.LogErrorFormat( "Tried to deactivate stem {0} in {1} but index is invalid.", a_stemIndex, this );
            return;
        }
        m_audioSourceList[a_stemIndex].volume = 0.0f;
    }

    public void Play() {
        for( int i = 0; i < m_stemList.Count; ++i ) {
            if ( m_stemList[i].isInitiallyActive ) ActivateStem( i );
            else DeactivateStem( i );
        }

        IsPlaying = true;
        OnPlay.Invoke();
    }

    public void Load() {
        foreach ( var audioSource in m_audioSourceList ) audioSource.Play();
    }

    public void Rewind() {
        foreach ( var audioSource in m_audioSourceList ) {
            // this doesn't work for some reason >_<
            //audioSource.timeSamples = 0;

            audioSource.Stop();
            audioSource.Play();
        }

        m_timeElapsed = 0.0f;
        m_totalTimeElapsed = 0.0f;
        m_curBar = m_curBeat = 0;
    }

    public void Silence() {
        for ( int i = 0; i < m_stemList.Count; ++i ) DeactivateStem( i );
    }

    public void Stop() {
        foreach ( var audioSource in m_audioSourceList ) audioSource.Stop();
    }

    private void Awake() {
        if( m_stemList.Count == 0 ) {
            Debug.LogErrorFormat( "Song {0} has no stems. Destroying.", this );
            Destroy( this );
            return;
        }

        m_songLengthSec = m_stemList[0].clip.length;
        m_secPerBeat = 1.0f / ( m_bpm / 60.0f );

        foreach ( var stem in m_stemList ) {
            if( stem.clip == null ) {
                Debug.LogErrorFormat( "Null stem reference in {0}. Destroying song.", this );
                Destroy( this );
                return;
            }
            if( stem.clip.length != m_songLengthSec ) {
                Debug.LogErrorFormat( "Stem time mismatch in {0}. Destroying song.", this );
                Destroy( this );
                return;
            }
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = stem.clip;
            audioSource.loop = false;
            m_audioSourceList.Add( audioSource );
        }
    }

    private void OnDestroy() {
        foreach ( var audioSource in m_audioSourceList ) Destroy( audioSource );
    }

    void Start () {
        if( m_audioSourceList.Count != m_stemList.Count) {
            Debug.LogErrorFormat( "Failed to create audio sources ({0}) for stem clips ({1}). Destroying song {2}.",
                m_audioSourceList.Count, m_stemList.Count, gameObject );
            Destroy( this );
            return;
        }
	}

    private float m_totalTimeElapsed = 0.0f;

    private void Update() {
        m_timeElapsed += Time.deltaTime;
        if ( m_timeElapsed > m_secPerBeat ) {
            ++m_curBeat;
            m_timeElapsed -= m_secPerBeat;
            if ( m_curBeat > m_beatsPerBar ) {
                m_curBeat = 1;
                ++m_curBar;
                OnBar.Invoke( m_curBar );
            }
            OnBeat.Invoke( m_curBeat );
        }

        m_totalTimeElapsed += Time.deltaTime;
        if( m_totalTimeElapsed >= m_songLengthSec ) {
            if ( m_loop ) {
                //Debug.Log( "loop song" );
                Rewind();
                OnLoop.Invoke();
            } else {
                //Debug.Log( "end song" );
                IsPlaying = false;
                OnEnd.Invoke();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongManager : MonoBehaviour
{
    public const int NO_SONG = -1;
    static public SongManager instance = null;

    [System.Serializable]
    public enum TransitionMode {
        Immediate,
        OnBar,
        OnBeat,
        OnLoop
    }

    [SerializeField]
    private bool m_rewindBeforeTransition = false;

    [SerializeField]
    private float m_crossfadeTimeSec = 0.0f;

    [SerializeField]
    List<Song> m_songList = new List<Song>();

    [SerializeField]
    private int m_curSongId = NO_SONG;

    [SerializeField]
    private int m_nextSongId = NO_SONG;

    private Song CurSong {
        get {
            if ( m_curSongId == NO_SONG ) return null;
            return m_songList[m_curSongId];
        }
    }
    private Song NextSong {
        get {
            if ( m_nextSongId == NO_SONG ) return null;
            return m_songList[m_nextSongId];
        }
    }

    private float m_transitionTimeElapsed = 0.0f;
    private bool m_isTransitioning = false;

    public void FadeOut() {
        m_nextSongId = NO_SONG;
        GoToNextSong();
    }

    public int GetSongId( Song a_song ) {
        for ( int i = 0; i < m_songList.Count; ++i )
            if ( a_song.name == m_songList[i].name ) return i;
        return NO_SONG;
    }

    // TODO get rid of this once we have a proper queue
    public Song GetSong(int a_id ) { return m_songList[a_id]; }

    public void GoToNextSong() { GoToNextSong( 0 ); }

    public void GoToNextSong( int a_beatOrBar ) {
        //Debug.Log( "Old song: " + CurSong + " | New song: " + NextSong );
        if ( m_isTransitioning ) {
            Debug.LogWarning( "Tried to change song but still in transition to next (ignoring)." );
            return;
        }

        if ( CurSong != null ) {
            CurSong.OnBar.RemoveListener( GoToNextSong );
            CurSong.OnBeat.RemoveListener( GoToNextSong );
        }

        if ( m_rewindBeforeTransition && NextSong != null ) NextSong.Rewind();
        PlayNext();
        m_transitionTimeElapsed = 0.0f;
        m_isTransitioning = true;
    }

    public void Play() {
        if ( CurSong != null ) CurSong.Play();
    }

    public void PlayNext() {
        if ( NextSong != null ) NextSong.Play();
    }

    // TODO enqueue each song with transition data and change to "queueNext"
    public void SetNext( int a_songId, TransitionMode a_transitionMode = TransitionMode.Immediate ) {
        if ( a_songId < 0 || a_songId >= m_songList.Count ) {
            Debug.LogErrorFormat( "Illegal song ID {0}. Ignoring SetNext().", a_songId );
            return;
        }

        m_nextSongId = a_songId;

        // force immediate transition if this is the first song
        if ( CurSong == null ) a_transitionMode = TransitionMode.Immediate;

        switch ( a_transitionMode ) {
            case TransitionMode.Immediate:
                GoToNextSong();
                return;
            case TransitionMode.OnBar:
                CurSong.OnBar.AddListener( GoToNextSong );
                return;
            case TransitionMode.OnBeat:
                CurSong.OnBeat.AddListener( GoToNextSong );
                return;
            case TransitionMode.OnLoop:
                CurSong.OnLoop.AddListener( GoToNextSong );
                return;
        }
    }

    public void Stop() {
        if ( CurSong == null ) return;

        CurSong.ClearEvents();
        CurSong.Silence();
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogErrorFormat( "{0} is a duplicate Song Manager. Destroying.", gameObject );
            Destroy( this );
            return;
        }

        instance = this;
    }

    private void Start() {
        foreach( var song in m_songList) {
            song.Load();
            song.Silence();
        }
        Play();

        if ( NextSong != null ) GoToNextSong();
    }

    private void Update() {
        if ( !m_isTransitioning ) return;

        m_transitionTimeElapsed += Time.deltaTime;
        var t = m_transitionTimeElapsed / m_crossfadeTimeSec;
        if( CurSong != null ) CurSong.Volume = Mathf.Lerp( CurSong.MaxVolume, 0.0f, t );
        if( NextSong != null ) NextSong.Volume = Mathf.Lerp( 0.0f, NextSong.MaxVolume, t );

        if( m_transitionTimeElapsed >= m_crossfadeTimeSec) {
            m_isTransitioning = false;
            Stop();
            m_curSongId = m_nextSongId;
            m_nextSongId = NO_SONG;
        }
    }
}

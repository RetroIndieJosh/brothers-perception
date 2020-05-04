using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MultiImageFade : MonoBehaviour {
    [System.Serializable]
    enum Mode
    {
        Crossfade,
        Sequential
    }

    [SerializeField]
    Mode m_fadeMode = Mode.Sequential;

    [SerializeField]
    private List<Sprite> m_spriteList = new List<Sprite>();

    [SerializeField]
    private Color m_fadeInColor = Color.white;

    [SerializeField]
    private Color m_fadeOutColor = Color.clear;

    [SerializeField]
    private int m_fadeTimeMs = 1000;

    [SerializeField]
    private int m_holdTimeMs = 1500;

    [SerializeField]
    private UnityEvent m_onStart;

    [SerializeField]
    private UnityEvent m_onFinish;

    private List<SpriteRenderer> m_spriteRendererList = new List<SpriteRenderer>();

    void Start() {
        if ( m_spriteList.Count == 0 ) {
            Debug.LogErrorFormat( "Multi Image Fade requires at least one Sprite defined. Destroying on {0}.", name );
            Destroy( this );
            return;
        }

        // TODO create option to delay start (not on creation) and put this in a function

        m_onStart.Invoke();

        var totalTimeSec = ( m_fadeTimeMs * 2.0f + m_holdTimeMs ) / 1000.0f;
        var halfwayTimeSec = totalTimeSec / 2.0f;

        for( int i = 0; i < m_spriteList.Count; ++i ) {
            var go = new GameObject();
            go.transform.parent = transform;
            go.transform.localScale = transform.localScale;

            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = m_spriteList[i];
            spriteRenderer.color = m_fadeOutColor;

            var timer = go.AddComponent<Timer>();
            timer.PingPongMax = 1;
            timer.TimerStyle = Timer.Style.CountUp;
            timer.MaxMilliseconds = m_fadeTimeMs;
            timer.HoldMilliseconds = m_holdTimeMs;
            if ( i == m_spriteList.Count - 1 ) timer.OnEnd.AddListener( OnFinish );

            var counter = go.AddComponent<Counter>();
            counter.SetRange( 0, 1000 );
            timer.LinkedCounter = counter;

            var startTime = totalTimeSec * i;
            if ( m_fadeMode == Mode.Crossfade ) startTime = halfwayTimeSec * i;
            Debug.LogFormat( "Start timer {0} at {1}", i, startTime );
            timer.StartTimerDelayed( startTime );

            var spriteColorLerp = go.AddComponent<SpriteColorLerp>();

            spriteColorLerp.LerpCounter = counter;
            spriteColorLerp.StartColor = m_fadeOutColor;
            spriteColorLerp.EndColor = m_fadeInColor;

            m_spriteRendererList.Add( spriteRenderer );
        }
    }

    private void OnFinish() {
        m_onFinish.Invoke();
    }
}

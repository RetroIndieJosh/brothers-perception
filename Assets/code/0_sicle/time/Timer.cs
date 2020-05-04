using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Timer))]
public class TimerScriptEditor : ScriptEditor
{
    private void ShowHoldTime() {
        Timer timer = target as Timer;

        EditorGUILayout.LabelField( "Hold Time" );
        EditorGUILayout.BeginHorizontal();
        timer.HoldHours = EditorGUILayout.IntField( "H", timer.HoldTime.Hours );
        timer.HoldMinutes = EditorGUILayout.IntField( "M", timer.HoldTime.Minutes );
        timer.HoldSeconds = EditorGUILayout.IntField( "S", timer.HoldTime.Seconds );
        timer.HoldMilliseconds = EditorGUILayout.IntField( "MS", timer.HoldTime.Milliseconds );
        EditorGUILayout.EndHorizontal();

    }

    private void ShowMaxTime() {
        Timer timer = target as Timer;

        EditorGUILayout.LabelField( "Max Time" );
        EditorGUILayout.BeginHorizontal();
        timer.MaxHours = EditorGUILayout.IntField( "H", timer.MaxTime.Hours );
        timer.MaxMinutes = EditorGUILayout.IntField( "M", timer.MaxTime.Minutes );
        timer.MaxSeconds = EditorGUILayout.IntField( "S", timer.MaxTime.Seconds );
        timer.MaxMilliseconds = EditorGUILayout.IntField( "MS", timer.MaxTime.Milliseconds );
        EditorGUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI() {
        Timer timer = target as Timer;

        var defaultLabelWidth = EditorGUIUtility.labelWidth;
        var defaultFieldWidth = EditorGUIUtility.fieldWidth;

        EditorGUIUtility.labelWidth = 20;
        EditorGUIUtility.fieldWidth = 20;

        ShowMaxTime();
        ShowHoldTime();

        EditorGUIUtility.labelWidth = defaultLabelWidth;
        EditorGUIUtility.fieldWidth = defaultFieldWidth;

        SkipProperty( "MaxHours" );
        SkipProperty( "MaxMinutes" );
        SkipProperty( "MaxSeconds" );
        SkipProperty( "MaxMilliseconds" );
        SkipProperty( "HoldHours" );
        SkipProperty( "HoldMinutes" );
        SkipProperty( "HoldSeconds" );
        SkipProperty( "HoldMilliseconds" );

        var pingPongForever = serializedObject.FindProperty( "m_pingPongForever" ).boolValue;
        if ( pingPongForever ) SkipProperty( "PingPongMax" );

        ShowDefaultInspector();
    }
}
#endif // UNITY_EDITOR

public class Timer : MonoBehaviour
{
    public enum Style
    {
        CountDown,
        CountUp
    }

    [SerializeField]
    private bool m_pingPongForever = false;

    [SerializeField]
    public int PingPongMax = 0;

    [SerializeField]
    public Style TimerStyle = Style.CountDown;

    [SerializeField]
    public bool StartOnCreated = false;

    [SerializeField]
    public Counter LinkedCounter = null;

    [SerializeField]
    public UnityEvent OnEnd = new UnityEvent();

    public System.TimeSpan MaxTime {
        get { return new System.TimeSpan( 0, MaxHours, MaxMinutes, MaxSeconds, MaxMilliseconds ); }
    }
    public int MaxHours = 0;
    public int MaxMinutes = 0;
    public int MaxSeconds = 0;
    public int MaxMilliseconds = 0;

    public System.TimeSpan HoldTime {
        get { return new System.TimeSpan( 0, HoldHours, HoldMinutes, HoldSeconds, HoldMilliseconds ); }
    }
    public int HoldHours = 0;
    public int HoldMinutes = 0;
    public int HoldSeconds = 0;
    public int HoldMilliseconds = 0;

    private System.TimeSpan m_curTime = new System.TimeSpan();
    public System.TimeSpan CurTime {
        get { return m_curTime; }
        private set {
            m_curTime = value;
            if ( LinkedCounter != null ) LinkedCounter.Count = (float)CurTime.TotalMilliseconds;
        }
    }
    public bool IsRunning { get; private set; }

    private int m_cycleCount = 0;
    private int m_pingPongCount = 0;

    public void ResetTimer() {
        switch ( TimerStyle ) {
            case Style.CountDown: CurTime = MaxTime; break;
            case Style.CountUp: CurTime = System.TimeSpan.Zero; break;
        }
    }

    public void StartTimer() {
        if( LinkedCounter != null ) LinkedCounter.SetRange( 0, (float)MaxTime.TotalMilliseconds );

        ResetTimer();
        IsRunning = true;
    }

    public void StartTimerDelayed( float a_delaySec ) {
        StartCoroutine( StartTimerAfter( a_delaySec ) );
    }

    private IEnumerator StartTimerAfter( float a_delaySec ) {
        var elapsedTime = 0.0f;
        while ( elapsedTime < a_delaySec ) {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StartTimer();
    }

    public void StopTimer() {
        IsRunning = false;
    }

    private void Awake() {
        StopTimer();
    }

    private void Start() {
        ResetTimer();
        if ( StartOnCreated ) StartTimer();
    }

    private void Update() {
        if ( !IsRunning ) return;
        switch ( TimerStyle ) {
            case Style.CountDown: TickDown(); break;
            case Style.CountUp: TickUp(); break;
        }
    }

    private void HandlePingPong() {
        if ( !m_pingPongForever ) {
            if ( PingPongMax == 0 ) {
                OnEnd.Invoke();
                return;
            }

            ++m_cycleCount;
            Debug.LogFormat( "Cycle {0}", m_cycleCount );
            if ( m_cycleCount >= 2 ) {
                m_cycleCount = 0;
                ++m_pingPongCount;
                Debug.LogFormat( "Ping pong {0}/{1}", m_pingPongCount, PingPongMax );
                if ( m_pingPongCount >= PingPongMax ) {
                    OnEnd.Invoke();
                    return;
                }
            }
        }

        if ( TimerStyle == Style.CountDown ) TimerStyle = Style.CountUp;
        else TimerStyle = Style.CountDown;

        StartTimerDelayed( (float)HoldTime.TotalSeconds );
    }

    private void TickDown() {
        CurTime -= System.TimeSpan.FromSeconds( Time.deltaTime );
        if ( CurTime.TotalMilliseconds < 0.0f ) {
            StopTimer();
            HandlePingPong();
        }
    }

    private void TickUp() {
        CurTime += System.TimeSpan.FromSeconds( Time.deltaTime );
        if ( CurTime > MaxTime ) {
            StopTimer();
            HandlePingPong();
        }
    }
}

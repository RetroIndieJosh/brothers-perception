using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// TODO clamp count/min/max in custom editor

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Counter))]
public class CounterScriptEditor : ScriptEditor
{
    protected bool m_showCount = true;
    protected bool m_showRange = true;

    public override void OnInspectorGUI() {
        Counter counter = target as Counter;

        var defaultLabelWidth = EditorGUIUtility.labelWidth;
        var defaultFieldWidth = EditorGUIUtility.fieldWidth;

        if ( m_showCount ) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( "Initial Count (Raw)" );
            counter.RawCount = EditorGUILayout.DelayedFloatField( counter.RawCount );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( "Initial Count (Not Raw)" );
            var unscaledCount = EditorGUILayout.DelayedFloatField( counter.Count );
            counter.RawCount = unscaledCount / serializedObject.FindProperty( "m_scale" ).floatValue;
            GUILayout.EndHorizontal();
        }

        if ( m_showRange ) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( "Unscaled Min" );
            var min = EditorGUILayout.DelayedFloatField( serializedObject.FindProperty( "m_minimumUnscaled" ).floatValue );
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( "Unscaled Max" );
            var max = EditorGUILayout.DelayedFloatField( serializedObject.FindProperty( "m_maximumUnscaled" ).floatValue );
            GUILayout.EndHorizontal();

            counter.SetRange( min, max );

            EditorGUILayout.LabelField( "Scaled Range: " + counter.Minimum + ", " + counter.Maximum );
        }

        SkipProperty( "m_count" );
        SkipProperty( "m_minimumUnscaled" );
        SkipProperty( "m_maximumUnscaled" );

        ShowDefaultInspector();
    }
}
#endif // UNITY_EDITOR

public class Counter : MonoBehaviour {
    static List<Counter> m_counterList = new List<Counter>();

    static public Counter FindByName(string a_name ) {
        foreach( var counter in m_counterList)
            if ( counter.CounterName == a_name ) return counter;
        return null;
    }

    [System.Serializable]
    public enum ClampMode
    {
        None,
        Minimum,
        Maximum,
        Both
    }

    [System.Serializable]
    public enum RoundMode
    {
        None,
        Round,
        Floor,
        Ceiling
    }

    [SerializeField]
    [Tooltip("All values are multiplied by this")]
    private float m_scale = 1.0f;

    [SerializeField]
    private bool m_roundChangeValues = false;

    [SerializeField]
    private RoundMode m_roundMode = RoundMode.None;  

    [SerializeField]
    private string m_counterName = "Counter";

    public float RawCount = 0;

    [SerializeField]
    public ClampMode CurClampMode = ClampMode.None;

    [SerializeField]
    [Tooltip( "Set to negative for no minimum" )]
    private float m_minimumUnscaled = 0.0f;
    private float m_minimum = 0.0f;
    public float Minimum {  get { return m_minimum; } }

    [SerializeField]
    [Tooltip("Set to negative for no maximum")]
    private float m_maximumUnscaled = 100.0f;
    private float m_maximum = 100.0f;
    public float Maximum {  get { return m_maximum; } }

    [SerializeField]
    public FloatEvent OnChanged;

    public float Count {
        get { return Round(RawCount); }
        set {
            var clampMin = CurClampMode == ClampMode.Both || CurClampMode == ClampMode.Minimum;
            var clampMax = CurClampMode == ClampMode.Both || CurClampMode == ClampMode.Maximum;
            var min = clampMin ? m_minimum : Mathf.NegativeInfinity;
            var max = clampMax ? m_maximum : Mathf.Infinity;
            RawCount = Mathf.Clamp( value, min, max );
        }
    }

    public float Percent {
        get {
            var range = m_maximum - m_minimum;
            return Count / range;
        }
    }

    public string CounterName {  get { return m_counterName; } }

    public void Add( float a_value ) {
        if ( m_roundChangeValues ) a_value = Round( a_value );

        // set count to the *actual* count plus the given value (Count returns rounded)
        Count = RawCount + a_value;

        OnChanged.Invoke( a_value );
    }

    public void Decrement() { Add( -1 ); }
    public void Increment() { Add( 1 ); }

    public void ResetToMaximum() { Count = m_maximum; }
    public void ResetToMinimum() { Count = m_minimum; }

    // NOTE: do not call in Awake on other objects!
    public void SetRange(float a_minimum, float a_maximum ) {
        if( a_minimum >= a_maximum ) {
            Debug.LogWarningFormat( "Minimum must be less than maximum in Counter on {0}. "
                + "Ignoring change to ({1}, {2}).", name, a_minimum, a_maximum );
            return;
        }

        m_minimumUnscaled = a_minimum;
        m_maximumUnscaled = a_maximum;

        m_minimum = Round( m_minimumUnscaled );
        m_maximum = Round( m_maximumUnscaled );
        //Debug.LogFormat( "Set range to ({0}, {1}) in {2}", m_minimum, m_maximum, name );
    }

    virtual protected void Awake() {
        m_counterList.Add( this );
        SetRange( m_minimumUnscaled, m_maximumUnscaled );
    }

    virtual protected void Start() { }

    virtual protected void Update() {
        //Debug.LogFormat( "Actual/rounded: {0} / {1} / {2}", m_count, Count, Percent );
    }

    // TODO do we want to round first or scale first? maybe have option
    private float Round( float a_value ) {
        switch ( m_roundMode ) {
            case RoundMode.Ceiling: return Mathf.Ceil( a_value ) * m_scale;
            case RoundMode.Floor: return Mathf.Floor( a_value ) * m_scale;
            case RoundMode.Round: return Mathf.Round( a_value ) * m_scale;
            default: return a_value * m_scale;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( Health ) )]
public class HealthScriptEditor : CounterScriptEditor
{
    public override void OnInspectorGUI() {
        var health = target as Health;
        health.CurClampMode = Counter.ClampMode.Both;

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel( "Initial Health" );
        health.Count = EditorGUILayout.DelayedFloatField( health.Count );
        GUILayout.EndHorizontal();

        m_showCount = false;
        SkipProperty( "m_counterName" );
        SkipProperty( "m_clampType" );

        base.OnInspectorGUI();
    }
}
#endif //UNITY_EDITOR

public class Health : Counter
{
    [System.Serializable]
    private enum DeathType
    {
        Destroy,
        DestroyDamage,
        Respawn
    }

    [System.Serializable]
    private enum RegenerationType
    {
        None,
        Regenerate,
        Degenerate
    }

    [SerializeField]
    RegenerationType m_regenerationType = RegenerationType.None;

    [SerializeField]
    private float m_regenerateDelay = 1.0f;

    [SerializeField]
    private float m_regeneratePerSec = 0.1f;

    [SerializeField]
    private int m_lives = 1;

    [SerializeField]
    [Tooltip( "Destroy: Destroy this when health <= 0\n\n"
        + "DestroyDamage: Destroy this when health <= 0 and spawn damager"
        + "Respawn: Respawn at original position when health <= 0\n\n"
    )]
    private DeathType m_deathType = DeathType.Destroy;

    [SerializeField]
    [Tooltip( "The death type we become after running out of lives." )]
    private DeathType m_deathTypeFinal = DeathType.Destroy;

    [SerializeField]
    private float m_destroyDelay = 0.0f;

    [SerializeField]
    private Damager m_damagerPrefab = null;

    [SerializeField]
    public UnityEvent OnDeath = new UnityEvent();

    [SerializeField]
    [Tooltip( "Triggered when we run out of lives" )]
    public UnityEvent OnDeathFinal = new UnityEvent();

    [SerializeField]
    public UnityEvent AfterRespawn = new UnityEvent();

    private Vector2 m_spawnPoint = Vector2.zero;

    private float m_timeToRegen = 0.0f;

    private bool m_isDead = false;

    private Timer m_timer = null;

    public void AddLife( int a_life ) {
        if ( m_isDead ) return;

        m_lives += a_life;
    }

    public void ApplyDamage( float a_damage ) {
        if ( m_isDead ) return;

        //Debug.Log( "Apply " + a_damage + " damage" );
        Add( -a_damage );
        if ( Count <= 0.0f ) Die();
        if ( m_regenerationType == RegenerationType.Regenerate )
            m_timeToRegen = m_regenerateDelay;
    }

    protected override void Awake() {
        CurClampMode = Counter.ClampMode.Both;

        base.Awake();
    }

    override protected void Start() {
        ResetToMaximum();

        m_spawnPoint = transform.position;
        if ( m_regenerationType == RegenerationType.Degenerate )
            m_timeToRegen = m_regenerateDelay;

        // TODO why not just add timer to us?
        var go = new GameObject();
        go.name = name + " Health Timer";
        m_timer = go.AddComponent<Timer>();

        base.Start();
    }

    override protected void Update() {
        base.Update();

        if ( m_regenerationType == RegenerationType.None ) return;

        m_timeToRegen -= Time.deltaTime;
        if ( m_timeToRegen > 0.0f ) return;

        var deltaHealth = m_regeneratePerSec * Time.deltaTime;
        if ( m_regenerationType == RegenerationType.Degenerate ) deltaHealth = -deltaHealth;
        Add( deltaHealth );
    }

    private void Die() {
        m_isDead = true;
        OnDeath.Invoke();

        if ( m_deathType == DeathType.Respawn ) gameObject.SetActive( false );

        m_timer.MaxMilliseconds = Mathf.FloorToInt( m_destroyDelay * 1000.0f );
        m_timer.OnEnd.AddListener( FinishDeath );

        m_timer.StartTimer();
    }

    public void FinishDeath() {
        AddLife( -1 );

        if ( m_lives <= 0 ) {
            gameObject.SetActive( true );
            m_deathType = m_deathTypeFinal;
            OnDeathFinal.Invoke();

            // TODO reset to original number of lives?
            if ( m_deathType == DeathType.Respawn ) m_lives = 1;
        }

        switch ( m_deathType ) {
            case DeathType.Destroy:
                if ( m_lives > 0 ) return;
                Destroy( gameObject );
                Instantiate( m_damagerPrefab, transform.position, Quaternion.identity );
                return;
            case DeathType.DestroyDamage:
                if ( m_lives > 0 ) return;
                Destroy( gameObject );
                return;
            case DeathType.Respawn:
                gameObject.SetActive( true );
                transform.position = m_spawnPoint;
                ResetToMaximum();
                m_isDead = false;
                AfterRespawn.Invoke();
                return;
        }
    }
}

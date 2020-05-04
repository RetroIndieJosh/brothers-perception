using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour {
    [System.Serializable]
    enum AimMode
    {
        Axes,
        Direction,
        Facing
    }

    [SerializeField]
    private Damager m_bulletPrefab;

    [SerializeField]
    private float m_fireSpeed = 5.0f;

    [SerializeField]
    private AimMode m_aimMode = AimMode.Facing;

    [SerializeField]
    private string m_aimAxisX = "Horizontal";

    [SerializeField]
    private string m_aimAxisY = "Vertical";

    [SerializeField]
    private Facing.Direction m_aimDirection = Facing.Direction.North;

    [SerializeField]
    private Facing m_facing = null;

    [SerializeField]
    private float m_lifeTime = 2.0f;

    [SerializeField]
    private Counter m_ammoCounter = null;

    [SerializeField]
    private float m_shotsPerSecond = Mathf.Infinity;

    [SerializeField]
    private bool m_destroyOffscreenBullets = false;

    [SerializeField]
    [Tooltip("0 = no limit; implicitly activates Destroy Offscreen Bullets")]
    private int m_bulletsOnScreenMax = 0;

    public bool IsFiring { get; private set; }

    private float m_secSinceLastShot = Mathf.Infinity;
    private float m_secPerShot = 0.0f;

    private float m_secBeforeStop = 0.0f;

    private int m_bulletsOnScreenCount = 0;

    private Vector2 Velocity {
        get {
            switch( m_aimMode) {
                case AimMode.Axes:
                    var x = Input.GetAxis( m_aimAxisX );
                    var y = Input.GetAxis( m_aimAxisY );
                    return new Vector2( x, y ).normalized * m_fireSpeed;
                case AimMode.Direction:
                    return Facing.DirectionToVector( m_aimDirection ) * m_fireSpeed;
                case AimMode.Facing:
                    return m_facing.DirectionVector.normalized * m_fireSpeed;
                default: return Vector2.zero;
            }
        }
    }

    public void DestroyOffscreenBullet() {
        --m_bulletsOnScreenCount;
    }

    public void Fire() {
        if ( m_bulletPrefab == null ) return;

        Debug.LogFormat( "{0}/{1} bullets", m_bulletsOnScreenCount, m_bulletsOnScreenMax );
        if ( m_secSinceLastShot < m_secPerShot ) return;

        if( m_bulletsOnScreenMax > 0 && m_bulletsOnScreenCount >= m_bulletsOnScreenMax ) return;

        if ( m_ammoCounter != null ) {
            if ( m_ammoCounter.Count == 0 ) return;
            m_ammoCounter.Decrement();
        }

        var bullet = Instantiate( m_bulletPrefab, transform.position, Quaternion.identity );
        var body = bullet.GetComponent<Rigidbody2D>();
        if( body == null ) {
            Debug.LogErrorFormat( "Bullet prefab for Shooter must have Rigid Body 2D. Removing bullet in {0}.", name );
            m_bulletPrefab = null;
            return;
        }
        body.velocity = Velocity;

        if ( m_bulletsOnScreenMax > 0 || m_destroyOffscreenBullets ) {
            ++m_bulletsOnScreenCount;
            var offscreen = bullet.gameObject.AddComponent<OffScreenTrigger>();
            offscreen.OnExitScreen.AddListener( bullet.gameObject.DestroySelf );
            offscreen.OnExitScreen.AddListener( DestroyOffscreenBullet );
        } else {
            Destroy( bullet, m_lifeTime );
        }

        m_secSinceLastShot = 0.0f;
    }

    public void FireForSec(float a_seconds ) {
        StartFire();
        m_secBeforeStop = a_seconds;
    }

    public void StartFire() {
        IsFiring = true;
    }

    public void StopFire() {
        IsFiring = false;
    }

    private void Awake() {
        if( m_facing == null && m_aimMode == AimMode.Facing ) {
            m_facing = GetComponent<Facing>();
            if( m_facing == null ) {
                Debug.LogErrorFormat( 
                    "Aim mode for {0} set to facing, but no facing set or on GO. Destroying shooter component.", 
                    gameObject );
                Destroy( this );
                return;
            }
        }

        IsFiring = false;
        m_secPerShot = 1.0f / m_shotsPerSecond;
    }

    private void Update() {
        m_secSinceLastShot += Time.deltaTime;
        if ( m_secBeforeStop > 0.0f ) {
            m_secBeforeStop -= Time.deltaTime;
            if ( m_secBeforeStop <= 0.0f ) StopFire();
        }

        if ( IsFiring ) Fire();
    }
}

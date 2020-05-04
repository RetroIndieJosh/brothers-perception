using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFacing : MonoBehaviour {
    public SpriteRenderer m_targetRenderer;
    public Sprite spriteEast;
    public Sprite spriteNorth;
    public Sprite spriteSouth;
    public Sprite spriteWest;

    private Facing.Direction m_prevDirection = Facing.Direction.South;
    private Facing m_facing;

    private void Start() {
        m_facing = GetComponent<Facing>();
        syncFacing();
    }

    void Update () {
        if( m_facing.direction != m_prevDirection ) {
            syncFacing();
        }
	}

    void syncFacing() {
        m_prevDirection = m_facing.direction;
        switch ( m_facing.direction ) {
            case Facing.Direction.East:
                m_targetRenderer.sprite = spriteEast;
                return;
            case Facing.Direction.North:
                m_targetRenderer.sprite = spriteNorth;
                return;
            case Facing.Direction.South:
                m_targetRenderer.sprite = spriteSouth;
                return;
            case Facing.Direction.West:
                m_targetRenderer.sprite = spriteWest;
                return;
        }
    }
}

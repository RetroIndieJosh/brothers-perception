using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MultiSprite : MonoBehaviour {
    [System.Serializable]
    class NamedSprite
    {
        public string name;
        public Sprite sprite;
    }

    [SerializeField]
    private List<NamedSprite> m_namedSpriteList = new List<NamedSprite>();

    private SpriteRenderer m_spriteRenderer = null;

    public void AddSprite(string a_name, Sprite a_sprite) {
        if ( HasSprite( a_name ) ) return;

        var namedSprite = new NamedSprite() {
            name = a_name,
            sprite = a_sprite
        };
        m_namedSpriteList.Add( namedSprite );
    }

    public bool HasSprite(string a_name ) {
        foreach( var namedSprite in m_namedSpriteList)
            if ( namedSprite.name == a_name ) return true;
        return false;
    }

    public void SetSprite(string a_name ) {
        foreach( var namedSprite in m_namedSpriteList) {
            if( namedSprite.name == a_name ) {
                m_spriteRenderer.sprite = namedSprite.sprite;
                return;
            }
        }
    }

    private void Awake() {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }
}

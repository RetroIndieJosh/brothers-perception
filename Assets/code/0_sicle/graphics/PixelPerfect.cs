using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelPerfect : MonoBehaviour {
    const float PIXEL_SIZE = 1.0f / 16.0f;

	void LateUpdate () {
        var pos = transform.parent.position;
        pos.x = Mathf.Round( pos.x / PIXEL_SIZE ) * PIXEL_SIZE;
        pos.y = Mathf.Round( pos.y / PIXEL_SIZE ) * PIXEL_SIZE;
        transform.position = pos;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBitmaskConexion : MonoBehaviour
{
    public Sprite[] conexionSprites;
    public SpriteRenderer spriteRenderer;
    public Vector2 tile;
    public void Set(int sprite)
    {
        spriteRenderer.sprite = conexionSprites[Mathf.Clamp(sprite,0,conexionSprites.Length-1)];
    }
}

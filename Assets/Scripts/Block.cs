using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Vector3 pos;
    public Quaternion rot;
    public int id;

    public Block(Vector3 p, Quaternion r, int i)
    {
        this.pos = p;
        this.rot = r;
        this.id = i;
    }
}

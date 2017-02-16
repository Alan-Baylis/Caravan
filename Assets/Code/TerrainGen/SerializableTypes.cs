using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SAbleVector3
{
    public float x, y, z;

    public SAbleVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    public override string ToString()
    {
        return System.String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    public static implicit operator Vector3(SAbleVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator SAbleVector3(Vector3 rValue)
    {
        return new SAbleVector3(rValue.x, rValue.y, rValue.z);
    }
}

[System.Serializable]
public struct SAbleVector2
{
    public float x, y;

    public SAbleVector2(float rX, float rY)
    {
        x = rX;
        y = rY;
    }

    public override string ToString()
    {
        return System.String.Format("[{0}, {1}]", x, y);
    }

    public static implicit operator Vector2(SAbleVector2 rValue)
    {
        return new Vector2(rValue.x, rValue.y);
    }

    public static implicit operator SAbleVector2(Vector2 rValue)
    {
        return new SAbleVector2(rValue.x, rValue.y);
    }
}

[System.Serializable]
public struct SAbleColor
{
    public float r, g, b, a;

    public SAbleColor(float inR, float inG, float inB, float inA)
    {
        r = inR;
        g = inG;
        b = inB;
        a = inA;
    }

    public static implicit operator Color(SAbleColor rValue)
    {
        return new Color(rValue.r, rValue.g, rValue.b, rValue.a);
    }

    public static implicit operator SAbleColor(Color rValue)
    {
        return new SAbleColor(rValue.r, rValue.g, rValue.b, rValue.a);
    }
}
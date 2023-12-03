using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelpers
{
    public static float Spring(float from, float to, float time)
    {
        time = Mathf.Clamp01(time);
        time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
        return from + (to - from) * time;
    }
    public static Vector3 Spring(Vector3 from, Vector3 to, float time)
    {
        return new Vector3(Spring(from.x, to.x, time), Spring(from.y, to.y, time), Spring(from.z, to.z, time));
    }

}
public static class FloatHelper
{
    public static void Clamp(ref this float number, float min, float max)
    {
        number = Mathf.Clamp(number, min, max);
    }

    public static void Clamp(ref this float number, Vector2 minMax)
    {
        number = Mathf.Clamp(number, minMax.x, minMax.y);
    }
    public static void SwizzleVector2(ref this Vector2 vector)
    {
        Vector2 temp = vector;
        vector.x = temp.y;
        vector.y = temp.x;
    }
}
public static class RigidbodyMath
{
    public static Vector3 GetLateralVelocity(this Rigidbody rb)
    {
        return new Vector3(rb.velocity.x, 0, rb.velocity.z);
    }
}
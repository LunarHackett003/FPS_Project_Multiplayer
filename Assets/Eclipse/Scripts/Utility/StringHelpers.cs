using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringHelpers
{
    static string allowedAlphaNum = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    public static string RandomString(int length)
    {
        string ran = "";
        for (int i = 0; i < length; i++)
        {
            ran += allowedAlphaNum[Random.Range(0, length)];
        }
        Debug.Log(ran);
        return ran;
    }
}

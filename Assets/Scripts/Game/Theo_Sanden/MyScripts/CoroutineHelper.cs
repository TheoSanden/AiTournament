using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CoroutineHelper
{
    /// <summary>
    /// Sets the value inputed into the action to said value after amount of seconds. For lambda expression use (result => value = result) as input for value;
    /// </summary>

    public static IEnumerator SetAfterSeconds<T>(Action<T> value, T toSetTo, float time)
    {
        yield return new WaitForSeconds(time);
        value(toSetTo);
    }
}

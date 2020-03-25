using UnityEngine;

class Util
{
    /// <summary>
    /// Has a 50% chance of returning true.
    /// </summary>
    /// <returns></returns>
    public static bool CoinFlip()
    {
        return (Random.Range(0, 2) == 0);
    }
}


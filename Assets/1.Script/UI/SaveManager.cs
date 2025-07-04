using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public List<GameObject> saveTerminalList = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }
}

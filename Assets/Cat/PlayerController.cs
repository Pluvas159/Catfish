using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameObject hook;
    // Start is called before the first frame update
    void Start()
    {
        hook = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }
}

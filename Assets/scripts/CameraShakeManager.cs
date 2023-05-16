using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{   
    public static CameraShakeManager instance { get; private set; }

    // Start is called before the first frame update

    void Awake()
    {
        if (instance != null)
        {
            return;
        }
        instance = this;
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CameraShake(CinemachineImpulseSource source) {
        source.GenerateImpulse();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        WaterShapeController splash = gameObject.GetComponentInChildren<WaterShapeController>();
        if (collision.gameObject.tag == "Hook")
        {
            splash.Splash(collision.transform.position, collision.GetComponent<Rigidbody2D>().velocity.y);
        }

    }
}

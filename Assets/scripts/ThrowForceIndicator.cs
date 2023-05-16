using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowForceIndicator : MonoBehaviour
{
    public Slider forceSlider;
    private float force = 0f;
    private bool isIncreasing = false;
    private Image fillImage;
    private Gradient gradient;

    private float increaseSpeed; // Increasing speed
    public float twitchiness = 0.01f; // How much the force twitches
    

    private void Start()
    {
        increaseSpeed = Random.Range(1f, 2f);
        // Get the Image component of the Fill
        fillImage = forceSlider.fillRect.GetComponent<Image>();

        // Define your gradient here. The following is just an example.
        gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.yellow;
        colorKey[1].time = 1.0f;
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;
        gradient.SetKeys(colorKey, alphaKey);
    }

    void Update()
    {
        if (isIncreasing)
        {
            // make the force go up and down
            force += increaseSpeed * Time.deltaTime;
            if (force > 1f)
            {
                increaseSpeed = - Random.Range(1, 10f);
            } else if (force < 0f)
            {
                increaseSpeed = Random.Range(1, 10f);
            }
        }

        forceSlider.value = force; // Update the slider

        // Change the fill color based on the gradient
        fillImage.color = gradient.Evaluate(force);
    }

    public void SetActive(bool active)
    {
        forceSlider.gameObject.SetActive(active);
        if (!active)
        {
            // Reset the force when the indicator is deactivated
            force = 0f;
            isIncreasing = false;
        }
    }

    public void IsIncreasing(bool increasing)
    {
        isIncreasing = increasing;
    }

    public void SetPosition(Vector2 position)
    {
        // Set the position of the UI element
        transform.position = position;
    }

    public float value
    {
        get { return forceSlider.value; }
    }

}

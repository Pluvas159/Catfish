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

    private void Start()
    {
        // Hide the slider at start
        //forceSlider.gameObject.SetActive(false);

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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Show the slider when Space is first pressed
            //forceSlider.gameObject.SetActive(true);
            isIncreasing = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            // Hide the slider when Space is released
            //forceSlider.gameObject.SetActive(false);
            isIncreasing = false;
            // Call the method to throw the hook here with force as a parameter
            // For example: cat.ThrowHook(force);
            force = 0f;
        }

        if (isIncreasing)
        {
            force += Time.deltaTime / 10; // Increase the force over time
            if (force > 1f) force = 1f; // Don't let the force go over 1
        }

        forceSlider.value = force; // Update the slider

        // Change the fill color based on the gradient
        fillImage.color = gradient.Evaluate(force);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class WaterShapeController : MonoBehaviour
{

    [SerializeField]
    private SpriteShapeController spriteShapeController;
    [SerializeField]
    private GameObject wavePointPref;
    [SerializeField]
    private GameObject wavePoints;
    [SerializeField]
    public float waveSpeed;
    [SerializeField]
    private float waveHeight;
    [SerializeField]
    [Range(1, 1000)]
    private int WavesCount;

    private int CorsnersCount = 2;
    private List<WaterSpring> springs = new();
    // How stiff should our spring be constnat
    [SerializeField]
    private float springStiffness;
    // Slowing the movement over time
    [SerializeField]
    public float dampening;
    [SerializeField]
    // How much to spread to the other springs
    public float spread;

    void Start() { 
        //if not in editor
            // Clean waterpoints 
            StartCoroutine(CreateWaves());
    }
    IEnumerator CreateWaves() {
        foreach (Transform child in wavePoints.transform) {
            StartCoroutine(Destroy(child.gameObject));
        }
        yield return null;
        SetWaves();
        yield return null;
    }
    IEnumerator Destroy(GameObject go) {
        yield return null;
        DestroyImmediate(go);
    }
    private void SetWaves() { 
        Spline waterSpline = spriteShapeController.spline;
        int waterPointsCount = waterSpline.GetPointCount();


        for (int i = CorsnersCount; i < waterPointsCount - CorsnersCount; i++) {
            waterSpline.RemovePointAt(CorsnersCount);
        }

        Vector3 waterTopLeftCorner = waterSpline.GetPosition(1);
        Vector3 waterTopRightCorner = waterSpline.GetPosition(2);
        float waterWidth = waterTopRightCorner.x - waterTopLeftCorner.x;

        float spacingPerWave = waterWidth / (WavesCount+1);
        // Set new points for the waves
        for (int i = WavesCount; i > 0 ; i--) {
            int index = CorsnersCount;

            float xPosition = waterTopLeftCorner.x + (spacingPerWave*i);
            Vector3 wavePoint = new Vector3(xPosition, waterTopLeftCorner.y, waterTopLeftCorner.z);
            waterSpline.InsertPointAt(index, wavePoint);
            waterSpline.SetHeight(index, 0.1f);
            waterSpline.SetCorner(index, false);
            waterSpline.SetTangentMode(index, ShapeTangentMode.Continuous);

        }

        springs = new();
        for (int i = 0; i <= WavesCount+1; i++) {
            int index = i + 1; 
            
            Smoothen(waterSpline, index);

            GameObject wavePoint = Instantiate(wavePointPref, wavePoints.transform, false);
            wavePoint.transform.localPosition = waterSpline.GetPosition(index);

            WaterSpring waterSpring = wavePoint.GetComponent<WaterSpring>();
            waterSpring.Init(spriteShapeController);
            springs.Add(waterSpring);
        }

    }
    private void Smoothen(Spline waterSpline, int index)
    {
        Vector3 position = waterSpline.GetPosition(index);
        Vector3 positionPrev = position;
        Vector3 positionNext = position;
        if (index > 1) {
            positionPrev = waterSpline.GetPosition(index-1);
        }
        if (index - 1 <= WavesCount) {
            positionNext = waterSpline.GetPosition(index+1);
        }

        Vector3 forward = gameObject.transform.forward;

        float scale = Mathf.Min((positionNext - position).magnitude, (positionPrev - position).magnitude) * 0.33f;

        Vector3 leftTangent = (positionPrev - position).normalized * scale;
        Vector3 rightTangent = (positionNext - position).normalized * scale;

        SplineUtility.CalculateTangents(position, positionPrev, positionNext, forward, scale, out rightTangent, out leftTangent);
        
        waterSpline.SetLeftTangent(index, leftTangent);
        waterSpline.SetRightTangent(index, rightTangent);
    }
    void FixedUpdate()
    {

        foreach(WaterSpring waterSpringComponent in springs) {
            waterSpringComponent.WaveSpringUpdate(springStiffness, dampening);
            waterSpringComponent.WavePointUpdate();
        }

        UpdateSprings();

    }

    private void Update()
    {
        UpdateSpringVisibility();
    }

    private void UpdateSprings()
    {
        int count = springs.Count;
        float[] left_deltas = new float[count];
        float[] right_deltas = new float[count];

        float time = Time.time * waveSpeed;

        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                left_deltas[i] = spread * (springs[i].height - springs[i - 1].height);
                springs[i - 1].velocity += left_deltas[i];
            }
            if (i < springs.Count - 1)
            {
                right_deltas[i] = spread * (springs[i].height - springs[i + 1].height);
                springs[i + 1].velocity += right_deltas[i];
            }

            // Apply sine wave function to the springs' heights
            float sineWave = waveHeight * Mathf.Sin(i * 0.2f + time);
            springs[i].height += sineWave;
        }
    }

    private void UpdateSpringVisibility()
    {
        Camera mainCamera = Camera.main;
        float screenHeight = mainCamera.orthographicSize * 2;
        float screenWidth = screenHeight * mainCamera.aspect;
        Vector3 cameraLeftBottom = mainCamera.transform.position - new Vector3(screenWidth / 2, screenHeight / 2, 0);
        Vector3 cameraRightTop = mainCamera.transform.position + new Vector3(screenWidth / 2, screenHeight / 2, 0);

        foreach (WaterSpring spring in springs)
        {
            Vector3 position = spring.transform.position;

            // Check if the spring is within screen bounds
            if (position.x >= cameraLeftBottom.x && position.x <= cameraRightTop.x &&
                position.y >= cameraLeftBottom.y && position.y <= cameraRightTop.y)
            {
                if (!spring.gameObject.activeSelf)
                spring.gameObject.SetActive(true);
            }
            else
            {
                if (spring.gameObject.activeSelf)
                spring.gameObject.SetActive(false);
            }
        }
    }

    public void Splash(Vector3 position, float force)
    {
        float minDistance = Mathf.Infinity;
        WaterSpring nearestSpring = null;

        foreach (WaterSpring spring in springs)
        {
            float distance = Mathf.Abs(spring.transform.position.x - position.x);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestSpring = spring;
            }
        }

        if (nearestSpring != null)
        {
            nearestSpring.velocity += force / nearestSpring.resistance;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("FallingObject"))
        {
            if (collision.gameObject.GetComponent<Rigidbody2D>().velocity.y < 0)
            Splash(collision.transform.position, collision.GetComponent<Rigidbody2D>().velocity.y);
        }
    }

}

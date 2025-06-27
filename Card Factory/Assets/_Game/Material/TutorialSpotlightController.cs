using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialSpotlightController : MonoBehaviour
{
    [Header("UI References")]
    public Image overlayImage;          // UI Image s? d?ng shader
    public Canvas tutorialCanvas;       // Canvas ch?a overlay

    [Header("Spotlight Settings")]
    public float spotlightRadius = 0.15f;
    public float softEdge = 0.05f;
    public float animationDuration = 0.5f;
    public Color overlayColor = new Color(0, 0, 0, 0.8f);

    [Header("Tutorial Steps")]
    public TutorialStep[] tutorialSteps;

    private Material overlayMaterial;
    private Camera uiCamera;
    private int currentStep = 0;
    private bool isTutorialActive = false;

    [System.Serializable]
    public class TutorialStep
    {
        public Button targetButton;
        public string instructionText;
        public float customRadius = 0.15f;
    }

    void Start()
    {
        InitializeShader();
        uiCamera = tutorialCanvas.worldCamera ?? Camera.main;
    }

    void InitializeShader()
    {
        // T?o material instance t? shader
        if (overlayImage.material == null)
        {
            Debug.LogError("Overlay Image c?n có Material s? d?ng SpotlightOverlay shader!");
            return;
        }

        overlayMaterial = new Material(overlayImage.material);
        overlayImage.material = overlayMaterial;

        // Thi?t l?p giá tr? ban ??u
        overlayMaterial.SetColor("_Color", overlayColor);
        overlayMaterial.SetFloat("_SpotlightRadius", spotlightRadius);
        overlayMaterial.SetFloat("_SoftEdge", softEdge);
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Length == 0)
        {
            Debug.LogWarning("Không có tutorial steps nào ???c thi?t l?p!");
            return;
        }

        isTutorialActive = true;
        currentStep = 0;
        overlayImage.gameObject.SetActive(true);

        ShowStep(currentStep);
    }

    void ShowStep(int stepIndex)
    {
        if (stepIndex >= tutorialSteps.Length)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[stepIndex];

        // Tính toán v? trí spotlight trong UV coordinates
        Vector2 spotlightPos = WorldToUVPosition(step.targetButton.transform.position);

        // C?p nh?t shader properties
        StartCoroutine(AnimateSpotlight(spotlightPos, step.customRadius));

        // Thi?t l?p button click listener
        step.targetButton.onClick.RemoveAllListeners();
        step.targetButton.onClick.AddListener(() => OnStepCompleted());

        // Hi?n th? text h??ng d?n (n?u có UI Text component)
        // UpdateInstructionText(step.instructionText);
    }

    Vector2 WorldToUVPosition(Vector3 worldPos)
    {
        // Chuy?n ??i world position sang screen space
        Vector3 screenPos = uiCamera.WorldToScreenPoint(worldPos);

        // Chuy?n ??i screen space sang UV coordinates (0-1)
        RectTransform overlayRect = overlayImage.rectTransform;
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayRect, screenPos, uiCamera, out localPos);

        // Normalize v? UV coordinates
        Rect rect = overlayRect.rect;
        float uvX = (localPos.x - rect.x) / rect.width;
        float uvY = (localPos.y - rect.y) / rect.height;

        return new Vector2(uvX, uvY);
    }

    IEnumerator AnimateSpotlight(Vector2 targetPos, float targetRadius)
    {
        Vector4 currentCenter = overlayMaterial.GetVector("_SpotlightCenter");
        Vector2 startPos = new Vector2(currentCenter.x, currentCenter.y);
        float startRadius = overlayMaterial.GetFloat("_SpotlightRadius");

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth animation curve

            // Animate position
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
            overlayMaterial.SetVector("_SpotlightCenter", new Vector4(currentPos.x, currentPos.y, 0, 0));

            // Animate radius
            float currentRadius = Mathf.Lerp(startRadius, targetRadius, t);
            overlayMaterial.SetFloat("_SpotlightRadius", currentRadius);

            yield return null;
        }

        // ??m b?o giá tr? cu?i chính xác
        overlayMaterial.SetVector("_SpotlightCenter", new Vector4(targetPos.x, targetPos.y, 0, 0));
        overlayMaterial.SetFloat("_SpotlightRadius", targetRadius);
    }

    void OnStepCompleted()
    {
        currentStep++;
        ShowStep(currentStep);
    }

    void EndTutorial()
    {
        isTutorialActive = false;
        StartCoroutine(FadeOutTutorial());
    }

    IEnumerator FadeOutTutorial()
    {
        Color startColor = overlayMaterial.GetColor("_Color");
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            Color currentColor = Color.Lerp(startColor, endColor, t);
            overlayMaterial.SetColor("_Color", currentColor);

            yield return null;
        }


        overlayImage.gameObject.SetActive(false);
        overlayMaterial.SetColor("_Color", overlayColor); // Reset cho l?n sau
    }

    // Ph??ng th?c ?? g?i t? bên ngoài
    public void FocusOnButton(Button button, float radius = 0.15f)
    {
        if (!isTutorialActive)
        {
            overlayImage.gameObject.SetActive(true);
            isTutorialActive = true;
        }

        Vector2 spotlightPos = WorldToUVPosition(button.transform.position);
        StartCoroutine(AnimateSpotlight(spotlightPos, radius));
    }

    void OnDestroy()
    {
        // Cleanup material instance
        if (overlayMaterial != null)
        {
            DestroyImmediate(overlayMaterial);
        }
    }
}
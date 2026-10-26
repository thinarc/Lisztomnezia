using System;
using System.Collections.Generic;
using _Project.Scripts.AuthorSphere;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class SphereController : MonoBehaviour
{
    [Header("Role")]
    public bool isBigSphere = false;
    public Transform bigSphere;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float changeDirTime = 2f;

    [Header("Magnet")]
    public float magnetRadius = 5f;
    public float magnetStrength = 3f;

    [Header("Release")]
    public float releaseDistance = 1.2f;   // расстояние "захвата"
    public float releaseForce = 4f;        // сила отталкивания
    public float releaseTime = 1f;         // сколько отпускает

    [Header("Bounds")]
    public Vector2 boundsX = new Vector2(-10, 10);
    public Vector2 boundsY = new Vector2(-5, 5);

    private Rigidbody rb;
    private Vector3 direction;
    private float timer;

    private bool isReleasing = false;
    private float releaseTimer = 0f;

    private Camera cam;
    private bool manualMenuPhysics;

    public void UseMenuPhysics(Camera menuCamera)
    {
        manualMenuPhysics = true;
        cam = menuCamera;
    }

    public void StepMenuPhysics(float deltaTime)
    {
        if (rb != null) StepMovement(deltaTime);
    }
    
    private TextMeshPro bigTMP;

    private Color accumulatedColor = Color.white;
    private int absorbedCount = 0;

// Список текущих «внутри» сфер
    private List<Color> insideColors = new List<Color>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (!manualMenuPhysics) cam = Camera.main;

        if (!isBigSphere)
            PickNewDirection();

        if (isBigSphere)
        {
            scale = transform.localScale;
            scaleText = GetComponentInChildren<TextMeshPro>().transform.localScale;
            scaleText /= 1.12f;
            scaleText /= 1.12f;
            scaleText /= 1.12f;
            scaleText /= 1.12f;
            scaleText /= 1.12f;
            GetComponentInChildren<TextMeshPro>().transform.DOScale(Vector2.zero, 0f).SetEase(Ease.InExpo).SetUpdate(manualMenuPhysics);
            
            bigTMP = GetComponentInChildren<TextMeshPro>();
            accumulatedColor = bigTMP.color;
            absorbedCount = 0;
            insideColors.Clear();
        }
    }

    void FixedUpdate()
    {
        if (!manualMenuPhysics) StepMovement(Time.fixedDeltaTime);
    }

    private void StepMovement(float deltaTime)
    {
        if (isBigSphere)
        {
            BigSphereMovement();
        }
        else
        {
            SmallSphereMovement(deltaTime);
        }

        ClampPosition();
    }

    // --- BIG ---
    void BigSphereMovement()
    {
        if (cam == null) return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z - transform.position.z);

        Vector3 target = cam.ScreenToWorldPoint(mouse);

        Vector3 dir = (target - transform.position);
        dir.z = 0f;

        rb.linearVelocity = dir * moveSpeed;
    }

    // --- SMALL ---
    async void SmallSphereMovement(float deltaTime)
    {
        if (bigSphere == null) return;

        timer += deltaTime;

        if (timer >= changeDirTime)
        {
            PickNewDirection();
            timer = 0f;
        }

        Vector3 move = direction * moveSpeed;

        Vector3 toBigVec = bigSphere.position - transform.position;
        float distance = toBigVec.magnitude;
        Vector3 toBig = toBigVec.normalized;

        // --- режим отпускания ---
        if (isReleasing)
        {
            releaseTimer -= deltaTime;

            move = -toBig * releaseForce;

            if (releaseTimer <= 0f)
            {
                isReleasing = false;
            }

            rb.linearVelocity = move;
            return;
        }

        // --- магнит ---
        if (distance < magnetRadius)
        {
            float force = 1f - (distance / magnetRadius);

            Vector3 magnetMove = toBig * moveSpeed * magnetStrength;

            move = Vector3.Lerp(move, magnetMove, force * 0.4f);
        }

        // --- проверка "захвата" ---
        if (distance < releaseDistance)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(releaseTime * 2), ignoreTimeScale: manualMenuPhysics);
            if (distance > releaseDistance) return;
            
            isReleasing = true;
            releaseTimer = releaseTime;
        }

        rb.linearVelocity = move;
    }
    
    void AbsorbColor(Color newColor)
    {
        accumulatedColor = (accumulatedColor * absorbedCount + newColor) / (absorbedCount + 1);
        // accumulatedColor += newColor * 0.2f;
        // accumulatedColor = Color.Clamp01(accumulatedColor);
        absorbedCount++;

        // bigTMP.color = accumulatedColor;
        LerpColorTMP(bigTMP, accumulatedColor, 0.2f);
    }
    
    async UniTask LerpColorTMP(TextMeshPro tmp, Color target, float time)
    {
        Color start = tmp.color;
        float t = 0f;

        while (t < 1f)
        {
            t += (manualMenuPhysics ? Time.unscaledDeltaTime : Time.deltaTime) / time;
            tmp.color = Color.Lerp(start, target, t);
            await UniTask.Yield();
        }
        
        tmp.color = target;
    }
    
    private void AddColor(Color newColor)
    {
        // Усиливаем насыщенность
        newColor = ForceColorful(newColor);

        insideColors.Add(newColor);
        RecalculateAccumulatedColor();
    }
    
    private void RemoveColor(Color oldColor)
    {
        insideColors.Remove(oldColor);
        RecalculateAccumulatedColor();
    }
    
    private void RecalculateAccumulatedColor()
    {
        if (insideColors.Count == 0)
        {
            accumulatedColor = Color.white; // базовый цвет большой сферы
        }
        else
        {
            Color c = Color.black;
            foreach (var col in insideColors)
            {
                c += col;
            }
            accumulatedColor = c / insideColors.Count;
        }

        // плавно применяем
        _ = LerpColorTMP(bigTMP, accumulatedColor, 0.3f);
    }
    
    private Color GetPreferredColor()
    {
        // 1. Генерация базового цвета как раньше
        float h;

        float r = Random.value;
        if (r < 0.35f)
        {
            // красный
            h = Random.Range(0f, 0.05f);
        }
        else if (r < 0.7f)
        {
            // голубой
            h = Random.Range(0.5f, 0.65f);
        }
        else
        {
            // остальные оттенки, исключаем зелёный
            float temp = Random.Range(0f, 1f);
            if (temp > 0.25f && temp < 0.4f) temp = 0.4f;
            h = temp;
        }

        float s = Random.Range(0.4f, 0.7f);
        float v = Random.Range(0.4f, 0.7f);

        Color baseColor = Color.HSVToRGB(h, s, v);

        Color[] palette = new Color[]
        {
            new Color32(0x37, 0x12, 0x15, 255), // #371215
            new Color32(0x55, 0x5D, 0x69, 255), // #555D69
            new Color32(0x19, 0x19, 0x19, 255), // #191919
            new Color32(0x6A, 0x70, 0x71, 255), // #6A7071
            new Color32(0x66, 0x75, 0x96, 255)  // #667596
        };

        // Выбираем случайный цвет из палитры
        Color paletteColor = palette[Random.Range(0, palette.Length)];

        // 3. Смешиваем с базовым цветом, чтобы немного «вписался» в гамму
        float influence = 0.7f; // 20% от палитры, 80% от яркого цвета
        Color finalColor = Color.Lerp(baseColor, paletteColor, influence);

        return finalColor;
    }

    private Color GetOppositeColor(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        h = (h + 0.5f) % 1f; // противоположный тон
        return Color.HSVToRGB(h, s, v);
    }
    
    private Color IncreaseSaturation(Color color, float amount)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s + amount);
        return Color.HSVToRGB(h, s, v);
    }
    
    private Color ForceColorful(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = 1f;      // максимально насыщенный
        v = Mathf.Clamp01(v * 1.0f); // сохраняем яркость
        return Color.HSVToRGB(h, s, v);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isBigSphere) return;
        
        TextMeshPro otherTMP = other.GetComponentInChildren<TextMeshPro>();
        if (otherTMP != null)
        {
            AddColor(otherTMP.color);
        }
        
        other.transform.DOScale(8, 0.4f).SetEase(Ease.OutExpo).SetUpdate(manualMenuPhysics);
        scale *= 1.12f;
        scaleText *= 1.12f;
        transform.DOScale(scale, 0.4f).SetEase(Ease.OutExpo).SetUpdate(manualMenuPhysics);
        ppp[0].transform.DOScale(scaleText, 0.4f).SetEase(Ease.OutExpo).SetUpdate(manualMenuPhysics);
        ppp[1].transform.DOScale(scaleText, 0.4f).SetEase(Ease.OutExpo).SetUpdate(manualMenuPhysics);
    }

    public TextMeshPro[] ppp;
    
    private void OnTriggerExit(Collider other)
    {
        if (!isBigSphere)
        {
            if (other.CompareTag("BigSphere"))
            {
                TextMeshPro tmp = GetComponentInChildren<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.color = GetPreferredColor();
                    // if (Random.Range(0, 2) == 1) tmp.color = GetPreferredColor();
                    // else tmp.color = GetOppositeColor(tmp.color);
                }
            }
            return;
        }
        
        TextMeshPro otherTMP = other.GetComponentInChildren<TextMeshPro>();
        if (otherTMP != null)
        {
            RemoveColor(otherTMP.color);
        }
        
        other.transform.DOScale(12, 0.4f).SetEase(Ease.InExpo).SetUpdate(manualMenuPhysics);
        scale /= 1.1f;
        scaleText /= 1.12f;
        transform.DOScale(scale, 0.6f).SetEase(Ease.InExpo).SetUpdate(manualMenuPhysics);
        ppp[0].transform.DOScale(Vector2.zero, 0.4f).SetEase(Ease.InExpo).SetUpdate(manualMenuPhysics);
        ppp[1].transform.DOScale(Vector2.zero, 0.4f).SetEase(Ease.InExpo).SetUpdate(manualMenuPhysics);
    }

    private Vector3 scale;
    private Vector3 scaleText;

    void PickNewDirection()
    {
        direction = Random.insideUnitSphere;
        direction.z = 0f;
        direction.Normalize();
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, boundsX.x, boundsX.y);
        pos.y = Mathf.Clamp(pos.y, boundsY.x, boundsY.y);

        transform.position = pos;
    }

    void OnDrawGizmosSelected()
    {
        // Bounds
        Gizmos.color = Color.yellow;

        Vector3 center = new Vector3(
            (boundsX.x + boundsX.y) * 0.5f,
            (boundsY.x + boundsY.y) * 0.5f,
            0f
        );

        Vector3 size = new Vector3(
            boundsX.y - boundsX.x,
            boundsY.y - boundsY.y,
            0f
        );

        Gizmos.DrawWireCube(center, size);

        // Радиус магнита
        if (!isBigSphere)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, releaseDistance);
        }
    }
}

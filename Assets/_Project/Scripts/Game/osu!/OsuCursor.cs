using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.osu_
{
    public class OsuCursor : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        private RectTransform _rect;
        
        [SerializeField] private ParticleSystem click;
        
        private void Start() => _rect = GetComponent<RectTransform>();
        
        private void Update()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out var pos
            );

            _rect.localPosition = pos;
            
            if (Input.GetMouseButtonDown(0))
            {
                // click.transform.localPosition = pos;
                click.Play();
                
                transform
                    .DOScale(0.86f, 0.052f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                        transform.DOScale(1.032f, 0.07f)
                            .SetEase(Ease.OutBack)
                            .OnComplete(() =>
                                transform.DOScale(1f, 0.046f)
                                    .SetEase(Ease.OutQuad)
                            )
                    );
            }
        }
    }
}
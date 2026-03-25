using TMPro;
using UnityEngine;

namespace _Project.Scripts.AuthorSphere
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMP_Curve : MonoBehaviour
    {
        public float radius = 5f;        // радиус дуги
        public float arcAngle = 180f;    // угол (градусы)
        public bool invert = false;      // перевернуть (внутрь/наружу)

        private TMP_Text textComponent;

        void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
        }

        public void Update()
        {
            if (textComponent == null) return;

            textComponent.ForceMeshUpdate();
            var textInfo = textComponent.textInfo;

            int charCount = textInfo.characterCount;
            if (charCount == 0) return;

            float angleStep = arcAngle / Mathf.Max(1, charCount - 1);
            float startAngle = -arcAngle / 2f;

            for (int i = 0; i < charCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // центр символа
                Vector3 charMid = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;

                // смещение в локальное пространство
                for (int j = 0; j < 4; j++)
                    vertices[vertexIndex + j] -= charMid;

                float angle = startAngle + angleStep * i;
                float rad = angle * Mathf.Deg2Rad;

                float x = Mathf.Sin(rad) * radius;
                float y = Mathf.Cos(rad) * radius * (invert ? -1 : 1);

                Vector3 offset = new Vector3(x, y, 0);

                Quaternion rot = Quaternion.Euler(0, 0, -angle);

                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = rot * vertices[vertexIndex + j] + offset;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}
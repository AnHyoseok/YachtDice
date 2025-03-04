using UnityEngine;
using TMPro;

namespace IdleGame
{
    public class FontAssetChanger : MonoBehaviour
    {
        public TMP_FontAsset newFontAsset;

        void Start()
        {
            ChangeFontAssets();
        }

        // UI 활성화시 폰트 변경
        void OnEnable()
        {
            ChangeFontAssets();
        }

        void ChangeFontAssets()
        {
            // TextMeshProUGUI 폰트 찾기 (비활성화된 폰트 포함)
            TextMeshProUGUI[] textMeshProsUGUI = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (TextMeshProUGUI tmp in textMeshProsUGUI)
            {
                // 폰트 변경
                tmp.font = newFontAsset;
            }

            // TextMeshPro 폰트(TMP) 찾기 (비활성화된 폰트 포함)
            TextMeshPro[] textMeshProsTMP = FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (TextMeshPro tmp in textMeshProsTMP)
            {
                // 폰트 변경
                tmp.font = newFontAsset;
            }
        }

        public TextMeshProUGUI CreateNewTextMeshProUGUI(Vector3 position, string text)
        {
            GameObject newGameObject = new GameObject("NewTextMeshProUGUI");
            newGameObject.transform.position = position;

            TextMeshProUGUI newTMP = newGameObject.AddComponent<TextMeshProUGUI>();
            newTMP.font = newFontAsset; // 새로운 텍스트에 폰트 적용
            newTMP.text = text;

            return newTMP;
        }

        public TextMeshPro CreateNewTextMeshPro(Vector3 position, string text)
        {
            GameObject newGameObject = new GameObject("NewTextMeshPro");
            newGameObject.transform.position = position;

            TextMeshPro newTMP = newGameObject.AddComponent<TextMeshPro>();
            newTMP.font = newFontAsset; // 새로운 텍스트에 폰트 적용
            newTMP.text = text;

            return newTMP;
        }
    }
}

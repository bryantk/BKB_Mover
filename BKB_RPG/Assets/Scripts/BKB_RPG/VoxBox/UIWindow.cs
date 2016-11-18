using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using BKB_RPG;

namespace BKB_TEXT
{
    public class UIWindow : MonoBehaviour {

        public Image bgImage;
        public Text textElement;
        public RectTransform root;

        public TextAnchor Alignment {
            get { return textElement.alignment; }
            set { textElement.alignment = value; }
        }

        public bool Transparent
        {
            set { bgImage.gameObject.SetActive(value); }
            get { return bgImage.gameObject.activeSelf; }
        }

        public void SetText(string text) {
            textElement.text = text;
        }

        public void Align(HorizontalAlignment horizontal, VerticalAlignment vertical = VerticalAlignment.Top)
        {
            Align((int)horizontal, (int)vertical);
        }

        public void Align(int horizontal, int vertical = 0) {
            Alignment = (TextAnchor)(horizontal + vertical * 3);
        }

        public Vector2 Position
        {
            set { root.anchoredPosition = value; }
            get { return root.anchoredPosition; }
        }

        public bool Active
        {
            set { root.gameObject.SetActive(value); }
            get { return root.gameObject.activeSelf; }
        }

        public void Init()
        {
            root.localScale = Vector3.one;
        }
    }
}


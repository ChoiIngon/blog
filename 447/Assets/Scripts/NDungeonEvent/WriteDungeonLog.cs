using System.Collections;
using UnityEngine;

namespace NDungeonEvent
{
    public class WriteDungeonLog : DungeonEvent
    {
        private string text;
        private Color color;
        private int fontSize;

        public WriteDungeonLog(string text, Color color, int fontSize = 12)
        {
            this.text = text;
            this.color = color;
            this.fontSize = fontSize;
        }

        public IEnumerator OnEvent()
        {
            string hexColor = ColorToHex(color);
            DungeonLog.Write($"<size={fontSize}><color={hexColor}>{text}");
            yield break; ;
        }

        private string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            return $"#{r:X2}{g:X2}{b:X2}"; // 2자리 HEX 문자열 변환
        }
    }
}
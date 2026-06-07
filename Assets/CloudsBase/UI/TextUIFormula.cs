using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
public static class TextUIFormula  
{
    public static float GetCharacterWidth(string character, TMP_FontAsset font, float fontSize)
    {
        if (string.IsNullOrEmpty(character) || font == null)
            return 0;

        uint unicode = character[0]; // Lấy unicode của ký tự

        if (font.characterLookupTable.TryGetValue(unicode, out TMP_Character tmpCharacter))
        {
            // Glyph là dữ liệu chi tiết hơn về hình dạng ký tự
            var glyph = tmpCharacter.glyph;

            // Glyph metrics cho biết width, height, bearing...
            float glyphWidth = glyph.metrics.horizontalAdvance;

            // Scale kích thước theo font size
            float scale = fontSize / font.faceInfo.pointSize;

            return glyphWidth * scale;
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Character '{character}' not found in font asset.");
            return 0;
        }
    }
    public static float GetCharacterWidth(string character,float fontSize)
    {
        // Tạo TextGenerator để tính toán
        TextGenerator textGen = new TextGenerator();
        
        // Đặt các thông số cho TextGenerator
        TextGenerationSettings settings = new TextGenerationSettings();
        settings.fontSize = (int)fontSize;
        settings.lineSpacing = 1;  // Khoảng cách dòng, có thể tùy chỉnh
        settings.richText = false; // Nếu bạn không dùng rich text, để false
        settings.textAnchor = TextAnchor.UpperLeft;  // Dùng textAnchor để căn chỉnh chữ
        settings.scaleFactor = 1; // Tỷ lệ nếu cần
        settings.color = Color.white; // Màu sắc của văn bản
        settings.horizontalOverflow = HorizontalWrapMode.Overflow; // Chế độ tràn dòng
        settings.verticalOverflow = VerticalWrapMode.Overflow; // Chế độ tràn dòng dọc

        // Tính toán chiều rộng của ký tự
        textGen.Populate(character, settings);

        // Lấy chiều rộng của ký tự đầu tiên (vì chỉ có 1 ký tự trong string)
        return textGen.GetPreferredWidth(character, settings);
    }
    public static Vector2 GettextSize(TextMeshProUGUI textComponent) {
        return new Vector2( textComponent.preferredWidth,textComponent.preferredHeight);
    }
    public static string FormatNumberWithDots(int number,char symbor)
    {
        // Convert the number to a string
        string strNumber = number.ToString();
        
        // Use System.Globalization to format the number
        string formattedStr = "";
        for (int i = strNumber.Length - 1; i >= 0; i--)
        {
            formattedStr = strNumber[i] + formattedStr;
            if ((strNumber.Length - i) % 3 == 0 && i != 0)
            {
                formattedStr = symbor + formattedStr;
            }
        }
        
        return formattedStr;
    }
    public static string FormatNumber(float number)
    {
        if (number >= 1_000000000) // 1 tỷ
            return (number / 1000000000f).ToString("F0") + "g";
        if (number >= 1000000) // 1 triệu
            return (number / 1000000f).ToString("F0") + "m";
        if (number >= 1000f) // 1 nghìn
            return (number / 1000f).ToString("F0") + "k";

        return number.ToString("F0");
    }
}
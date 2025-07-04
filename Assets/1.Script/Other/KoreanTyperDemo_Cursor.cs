using System;
using System.Collections;
using UnityEngine;
using KoreanTyper;
using TMPro;
public class KoreanTyperDemo_Cursor : MonoBehaviour
{
    public static KoreanTyperDemo_Cursor instance;

    private string typingText;
    private char   cursor_char = '▐';

    private void Start()
    {
        instance = this;
    }
    
    public IEnumerator TypingCoroutine(string str,TextMeshProUGUI useText,bool isFrontCursorBlink,bool isTypingCursor,bool isEndCursorLoop,float typingSpeed) 
    {
        //=======================================================================================================
        // Blink cursor | 커서 깜빡임
        //=======================================================================================================
        if (isFrontCursorBlink)
        {
            typingText = "";
            for (int waitCount = 0; waitCount < 6; waitCount++) 
            {
                useText.text = typingText + cursor_char;
                yield return new WaitForSeconds(0.25f);
                useText.text = typingText;
                yield return new WaitForSeconds(0.25f);
            }
        }
        
        //=======================================================================================================
        // Typing effect | 타이핑 효과
        //=======================================================================================================
        
        for (int j = 0; j < str.Length + 1; j++)    // 글자 나오기.
        {
            typingText = str.Substring(0, j);
            if(!isTypingCursor)
                useText.text = typingText;
            else if(isTypingCursor)
                useText.text = typingText + cursor_char;
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        if(isTypingCursor && !isEndCursorLoop)   // isFrontCursor만 true인 경우, 마지막에 커서 없애기
            useText.text = typingText;
        //=======================================================================================================
        // Blink cursor | 커서 깜빡임
        //=======================================================================================================
        if (isEndCursorLoop)
        {
            while (true)
            {
                useText.text = typingText + cursor_char;
                yield return new WaitForSeconds(0.25f);
                useText.text = typingText;
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}

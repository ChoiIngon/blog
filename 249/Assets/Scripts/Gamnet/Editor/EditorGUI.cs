using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class EnumFlagsToggleAttribute : PropertyAttribute
{
    public Type enumType;
    public bool drawReserveETC;

    /// <summary> drawReserveETC : 향후 추가될지 모르는 enum타입에 대한 기본설정값을 세팅하게 해줍니다.  </summary>
    public EnumFlagsToggleAttribute(Type enumType, bool drawReserveETC = false)
    {
        this.enumType = enumType;
        this.drawReserveETC = drawReserveETC;
    }
}

[System.Diagnostics.Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class EditorGUIAttribute : Attribute
{
    public enum GUIType
    {
        /// <summary> 함수의 리턴타입이 Bool 이면 같은 이름의 EditorGUI 출력 여부를 정합니다. </summary>
        Condition,
        /// <summary> 함수의 리턴타입이 String 이면 empty가 아니면 다이얼로그 출력합니다. </summary>
        Button,
        /// <summary> 함수의 리턴타입이 String 이고 empty가 아니면 메세지박스를 출력합니다. </summary>
        HelpBox,

        TextField,

        Space,
    }

    public enum MessageType
    {
        //
        // 요약:
        //     Neutral message.
        None = 0,
        //
        // 요약:
        //     Info message.
        Info = 1,
        //
        // 요약:
        //     Warning message.
        Warning = 2,
        //
        // 요약:
        //     Error message.
        Error = 3
    }

    public string name;
    public object[] paramobjs;
    public GUIType guiType;

    public MessageType messageType = MessageType.None;

    public EditorGUIAttribute(GUIType guiType, string name, params object[] paramobjs)
    {
        this.name = name;
        this.paramobjs = paramobjs;
        this.guiType = guiType;
    }
    public EditorGUIAttribute(GUIType guiType, string name)
    {
        this.name = name;
        this.paramobjs = null;
        this.guiType = guiType;
    }

    public EditorGUIAttribute(GUIType guiType, MessageType messageType)
    {
        this.messageType = messageType;
        this.paramobjs = null;
        this.guiType = guiType;
    }
}

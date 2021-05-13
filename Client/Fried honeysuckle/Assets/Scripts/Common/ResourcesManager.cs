using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 资源管理
/// </summary>
public class ResourcesManager 
{
    /// <summary>
    /// 字典，存档已经获取到的图集
    /// </summary>
    private static Dictionary<string, Sprite> nameSpriteDic = new Dictionary<string, Sprite>();

    /// <summary>
    /// 获取图集
    /// </summary>
    /// <param name="iconName"></param>
    /// <returns></returns>
    public static Sprite GetSprite(string iconName)
    {
        if (nameSpriteDic.ContainsKey(iconName))
        {
            return nameSpriteDic[iconName];
        }
        else
        {

            Sprite[] sprites = Resources.LoadAll<Sprite>("headIcon");
            //切割字符，将头像名称分割开
            string[] nameArr = iconName.Split('_');
            //将加载出来的图片存档到字典里
            Sprite temp = sprites[int.Parse(nameArr[1])];
            nameSpriteDic.Add(iconName, temp);
            return temp;
        }

    }
    /// <summary>
    /// 加载牌的图集
    /// </summary>
    /// <param name="cardName">牌的名字</param>
    /// <returns></returns>
    public static Sprite LoadCardSprite(string cardName)
    {
        if (nameSpriteDic.ContainsKey(cardName))
        {
            return nameSpriteDic[cardName];
        }
        else
        {
            Sprite temp = Resources.Load<Sprite>("poke/" + cardName);
            nameSpriteDic.Add(cardName, temp);
            return temp;
        }
    }

}

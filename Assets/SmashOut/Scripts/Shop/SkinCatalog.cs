using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinCatalog", menuName = "JumpUp/Shop/Skin Catalog")]
public class SkinCatalog : ScriptableObject
{
    public List<SkinData> skins = new List<SkinData>();
}



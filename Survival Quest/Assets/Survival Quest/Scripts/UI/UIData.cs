using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame.UI
{
[CreateAssetMenu(fileName = "UIData", menuName = "CustomObjects/UIData", order = 1)]
public class UIData : ScriptableObject
{
    public GameObject[] m_UIPrefabs;
    public GameObject[] m_UIElementsPrefabs;
    public Dictionary<string, GameObject> m_UIPrefabList;


    [Space]
    public AnimationCurve m_AnimInCurve_1;
    public AnimationCurve m_AnimInCurve_2;

    public bool m_UseOverrideTab = false;
    public int m_OverrideTabNum = 0;
}
}
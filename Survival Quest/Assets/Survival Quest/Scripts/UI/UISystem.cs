using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalGame.UI
{
    public class UISystem : MonoBehaviour
    {
        public static UISystem m_Main;

        public UIData m_UIData;
        [HideInInspector]
        public List<GameObject> m_UIStack;
        [HideInInspector]
        public GameObject m_LastUI;
        [HideInInspector]
        public int m_LayerOrder = 0;
        public int m_MessageLayerOrder = 0;

        public string m_InitUI = "";

        public Vector2 m_GeneralCanvasSize = new Vector2(1600, 900);

        private void Awake()
        {
            m_Main = this;
            m_UIStack = new List<GameObject>();
            m_LayerOrder = 1;
            m_MessageLayerOrder = 100;
        }

        // Start is called before the first frame update
        void Start()
        {
            float screenW = Screen.width > 0 ? (float)Screen.width : 1920f;
            float screenH = Screen.height > 0 ? (float)Screen.height : 1080f;
            float ratio = screenW / screenH;
            if (float.IsNaN(ratio) || float.IsInfinity(ratio) || ratio <= 0f)
            {
                ratio = 16f / 9f;
            }
            m_GeneralCanvasSize = new Vector2(ratio * 900f, 900f);

            if (!string.IsNullOrEmpty(m_InitUI))
            {
                ShowUI(m_InitUI);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public static GameObject FindUIByName(string uiName)
        {
            if (m_Main == null || m_Main.m_UIData == null) return null;
            foreach (GameObject uiObj in m_Main.m_UIData.m_UIPrefabs)
            {
                if (uiObj != null && uiObj.name == uiName)
                {
                    return uiObj;
                }
            }
            return null;
        }

        public static GameObject FindOpenUIByName(string uiName)
        {
            if (m_Main == null || m_Main.m_UIStack == null) return null;
            for (int i = 0; i < m_Main.m_UIStack.Count; i++)
            {
                if (m_Main.m_UIStack[i] != null && m_Main.m_UIStack[i].name == uiName)
                {
                    return m_Main.m_UIStack[i];
                }
            }
            return null;
        }

        public static GameObject ShowUI(string uiName)
        {
            if (m_Main == null) return null;
            GameObject uiprefab = FindUIByName(uiName);
            if (uiprefab != null)
            {
                GameObject uiObj = Instantiate(uiprefab);
                uiObj.transform.SetParent(m_Main.transform);
                uiObj.transform.localPosition = Vector3.zero;
                uiObj.transform.localScale = Vector3.one;
                uiObj.transform.localRotation = Quaternion.identity;
                uiObj.name = uiName;
                m_Main.m_LastUI = uiObj;
                m_Main.m_UIStack.Add(uiObj);

                Camera uiCam = m_Main.GetComponentInChildren<Camera>();

                Canvas canvas = uiObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = m_Main.m_LayerOrder;
                    m_Main.m_LayerOrder++;

                    if (canvas.worldCamera == null && uiCam != null)
                    {
                        canvas.worldCamera = uiCam;
                    }
                }

                CanvasSizeFix sizeFix = uiObj.GetComponentInChildren<CanvasSizeFix>();
                if (sizeFix != null)
                {
                    sizeFix.UpdateCanvasSize();
                }

                return uiObj;
            }
            Debug.Log("UI not found : " + uiName);
            return null;
        }

        public static void RemoveUI(string uiName)
        {
            if (m_Main == null || m_Main.m_UIStack == null) return;
            for (int i = 0; i < m_Main.m_UIStack.Count; i++)
            {
                if (m_Main.m_UIStack[i] != null && m_Main.m_UIStack[i].name == uiName)
                {
                    if (m_Main.m_LastUI == m_Main.m_UIStack[i])
                        m_Main.m_LastUI = null;
                    Destroy(m_Main.m_UIStack[i]);
                    m_Main.m_UIStack.RemoveAt(i);
                    i--;
                }
            }
        }

        public static void RemoveUI(GameObject uiObj)
        {
            if (m_Main == null || m_Main.m_UIStack == null) return;
            for (int i = 0; i < m_Main.m_UIStack.Count; i++)
            {
                if (m_Main.m_UIStack[i] == uiObj)
                {
                    if (m_Main.m_LastUI == m_Main.m_UIStack[i])
                        m_Main.m_LastUI = null;
                    Destroy(m_Main.m_UIStack[i]);
                    m_Main.m_UIStack.RemoveAt(i);
                    i--;
                }
            }
        }

        public static Image FindImage(GameObject parent, string targetName)
        {
            if (parent == null) return null;
            Image[] all = parent.GetComponentsInChildren<Image>(true);
            foreach (Image img in all)
            {
                if (img != null && img.gameObject.name == targetName)
                {
                    return img;
                }
            }
            return null;
        }

        public static Text FindText(GameObject parent, string targetName)
        {
            if (parent == null) return null;
            Text[] all = parent.GetComponentsInChildren<Text>(true);
            foreach (Text text in all)
            {
                if (text != null && text.gameObject.name == targetName)
                {
                    return text;
                }
            }
            return null;
        }

        public static void ShowReward(string rewardType, int count1 = 0, int count2 = 0, string title = "", Sprite sprite = null)
        {
            if (m_Main != null)
                m_Main.StartCoroutine(Co_ShowReward(rewardType, title, count1, count2, sprite));
        }

        static IEnumerator Co_ShowReward(string rewardType, string title, int count1, int count2, Sprite sprite)
        {
            string prefab = "CoinRewardUI";
            GameObject uiObj = null;
            Text txt;
            Image image;
            switch (rewardType)
            {
                case "coin":
                    prefab = "CoinRewardUI";
                    uiObj = ShowUI(prefab);
                    txt = FindText(uiObj, "text-amount");
                    if (txt != null) txt.text = count1.ToString();
                    break;

                case "gem":
                    prefab = "GemRewardUI";
                    uiObj = ShowUI(prefab);
                    txt = FindText(uiObj, "text-amount");
                    if (txt != null) txt.text = count1.ToString();
                    break;

                case "upgrade":
                    prefab = "UpgradeRewardUI";
                    uiObj = ShowUI(prefab);
                    txt = FindText(uiObj, "text-title");
                    if (txt != null) txt.text = title;
                    image = FindImage(uiObj, "img-icon");
                    if (image != null) image.sprite = sprite;
                    break;

                case "turn":
                    prefab = "TurnRewardUI";
                    uiObj = ShowUI(prefab);
                    txt = FindText(uiObj, "text-amount");
                    if (txt != null) txt.text = count1.ToString();
                    break;

                case "wheel":
                    prefab = "WheelRewardMsg";
                    uiObj = ShowUI(prefab);
                    txt = FindText(uiObj, "text-amount");
                    if (txt != null) txt.text = count1.ToString();
                    break;
            }

            if (uiObj != null)
            {
                Canvas canvas = uiObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 150;
                    if (canvas.worldCamera == null && m_Main != null)
                    {
                        Camera uiCam = m_Main.GetComponentInChildren<Camera>();
                        if (uiCam != null) canvas.worldCamera = uiCam;
                    }
                }

                yield return new WaitForSeconds(3);
                RemoveUI(uiObj);
            }
        }

        public static void ShowCoinReward(int count)
        {
            if (m_Main != null)
                m_Main.StartCoroutine(Co_ShowCoinReward(count));
        }

        static IEnumerator Co_ShowCoinReward(int count)
        {
            GameObject uiObj = ShowUI("CoinRewardUI");
            if (uiObj != null)
            {
                Text txt = FindText(uiObj, "text-amount");
                if (txt != null) txt.text = count.ToString();

                Canvas canvas = uiObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 150;
                    if (canvas.worldCamera == null && m_Main != null)
                    {
                        Camera uiCam = m_Main.GetComponentInChildren<Camera>();
                        if (uiCam != null) canvas.worldCamera = uiCam;
                    }
                }

                yield return new WaitForSeconds(3);
                RemoveUI(uiObj);
            }
        }

        public static void ShowGemReward(int count)
        {
            if (m_Main != null)
                m_Main.StartCoroutine(Co_ShowGemReward(count));
        }

        static IEnumerator Co_ShowGemReward(int count)
        {
            GameObject uiObj = ShowUI("GemRewardUI");
            if (uiObj != null)
            {
                Text txt = FindText(uiObj, "text-amount");
                if (txt != null) txt.text = count.ToString();

                Canvas canvas = uiObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 150;
                    if (canvas.worldCamera == null && m_Main != null)
                    {
                        Camera uiCam = m_Main.GetComponentInChildren<Camera>();
                        if (uiCam != null) canvas.worldCamera = uiCam;
                    }
                }

                yield return new WaitForSeconds(3);
                RemoveUI(uiObj);
            }
        }

        public static void ShowCoinGemReward(int coincount, int gemcount)
        {
            if (m_Main != null)
                m_Main.StartCoroutine(Co_ShowCoinGemReward(coincount, gemcount));
        }

        static IEnumerator Co_ShowCoinGemReward(int coincount, int gemcount)
        {
            GameObject uiObj = ShowUI("CoinGemRewardUI");
            if (uiObj != null)
            {
                Text txtcoin = FindText(uiObj, "coin-text-amount");
                Text txtgem = FindText(uiObj, "gem-text-amount");
                if (txtcoin != null) txtcoin.text = coincount.ToString();
                if (txtgem != null) txtgem.text = gemcount.ToString();

                Canvas canvas = uiObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 150;
                    if (canvas.worldCamera == null && m_Main != null)
                    {
                        Camera uiCam = m_Main.GetComponentInChildren<Camera>();
                        if (uiCam != null) canvas.worldCamera = uiCam;
                    }
                }

                yield return new WaitForSeconds(3);
                RemoveUI(uiObj);
            }
        }

        public static void ShowUpgradeReward(Sprite img, string title)
        {
            if (m_Main != null)
                m_Main.StartCoroutine(Co_ShowUpgradeReward(img, title));
        }

        static IEnumerator Co_ShowUpgradeReward(Sprite img, string title)
        {
            GameObject uiObj = ShowUI("UpgradeRewardUI");
            if (uiObj != null)
            {
                Text txt = FindText(uiObj, "text-title");
                if (txt != null) txt.text = title;
                Image image = FindImage(uiObj, "img-icon");
                if (image != null) image.sprite = img;

                Canvas canvas = uiObj.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 150;
                    if (canvas.worldCamera == null && m_Main != null)
                    {
                        Camera uiCam = m_Main.GetComponentInChildren<Camera>();
                        if (uiCam != null) canvas.worldCamera = uiCam;
                    }
                }

                yield return new WaitForSeconds(3);
                RemoveUI(uiObj);
            }
        }
    }
}
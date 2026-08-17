using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarriorsVSOrcs
{
    public class FixFrameRate : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Resolution currentResolution = Screen.currentResolution;
#if UNITY_2022_2_OR_NEWER
            float refreshRate = (float)currentResolution.refreshRateRatio.value;
#else
            float refreshRate = currentResolution.refreshRate;
#endif
            if (refreshRate > 0 && !float.IsNaN(refreshRate) && !float.IsInfinity(refreshRate))
                Application.targetFrameRate = Mathf.RoundToInt(refreshRate);
            else
                Application.targetFrameRate = 60;
        }
    }
}

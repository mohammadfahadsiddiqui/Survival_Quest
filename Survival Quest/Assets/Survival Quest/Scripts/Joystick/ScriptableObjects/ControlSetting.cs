using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurvivalGame
{
    public enum PlatformControlType
    {
        PC = 0,
        Mobile = 1
    }

    public enum AimType
    {
        Movement = 0,
        AutoByAngle = 1,
        AutoByDistance = 2,
        Full360 = 3
    }

    [CreateAssetMenu(fileName = "ControlSetting", menuName = "CustomObjects/ControlSetting", order = 1)]
    public class ControlSetting : ScriptableObject
    {
        public PlatformControlType ControlType = PlatformControlType.PC;
        public AimType AimType = AimType.Movement;

        public bool AutoFire = false;

    }
}
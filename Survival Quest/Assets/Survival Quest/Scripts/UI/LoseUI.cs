using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace SurvivalGame.UI
{

    public class LoseUI : MonoBehaviour
    {


        void Start()
        {
          
        }

        void Update()
        {

        }


        public void BtnRestart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void BtnExit()
        {

           Application.Quit();
        }

    }

}
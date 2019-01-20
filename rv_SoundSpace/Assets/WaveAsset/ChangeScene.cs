using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public GameObject SplashVideo;
    public GameObject SplashVideo2;

    void Start()
    {
        StartCoroutine(Wait());
        SplashVideo.SetActive(true);
        SplashVideo2.SetActive(false);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3.5f);

        SplashVideo.SetActive(false);
        SplashVideo2.SetActive(true);

        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene("MR_MediaLab", LoadSceneMode.Single);
    }
}

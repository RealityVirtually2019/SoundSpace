using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public GameObject SplashVideo;

    void Start()
    {
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(10);
        SceneManager.LoadScene("MR_MediaLab", LoadSceneMode.Single);
        SplashVideo.SetActive(false);
    }
}

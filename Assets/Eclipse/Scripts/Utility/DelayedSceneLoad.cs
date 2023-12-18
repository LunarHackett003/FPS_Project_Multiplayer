using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DelayedSceneLoad : MonoBehaviour
{
    public SceneReference targetScene;
    public float delay;
    public void InvokeSceneLoad()
    {
        Invoke(nameof(LoadNewScene), delay);
    }
    void LoadNewScene()
    {
        SceneManager.LoadScene(targetScene.BuildIndex);
    }
}

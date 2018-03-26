using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;


namespace HoloPlaySDK
{
    public class HoloPlayLauncher : MonoBehaviour
    {
        static HoloPlayLauncher instance;
        public static HoloPlayLauncher Instance
        {
            get
            {
                //lazy singleton
                if (instance == null)
                {
                    instance = FindObjectOfType<HoloPlayLauncher>();
                    if (instance == null)
                    {
                        GameObject launcherGO;
                        if (HoloPlay.Main != null)
                            launcherGO = HoloPlay.Main.gameObject;
                        else
                            launcherGO = new GameObject("HoloPlay Launcher");
                        instance = launcherGO.AddComponent<HoloPlayLauncher>();
                    }
                }
                return instance;
            }
        }
        public bool currentlyQuittingToLauncher { get; private set; }
        float countdown;
        float countdownTime = 1f;
        public static readonly string quitToLauncherArg = "holoplayQuitToLauncher";

        void OnEnable()
        {
            if (instance != null)
            {
                Destroy(this);
                Debug.Log("can't have multiple Launcher, deleting one");
            }
        }

        public void StartLoadingApp(string appFileName)
        {
            StopAllCoroutines();
            StartCoroutine(StartCountdown(appFileName));
        }

        public void CancelLoadingApp()
        {
            currentlyQuittingToLauncher = false;
        }

        void LoadApp(string appFileName)
        {
            string extension = Application.platform == RuntimePlatform.WindowsPlayer ? ".exe" : ".app";
            // string fullPath = Path.Combine(Path.GetFullPath("."), appFileName + extension);
            string fullPath = Path.GetFullPath(".");
            string args = "";
            if (Application.platform != RuntimePlatform.WindowsPlayer)
            {
                args = " --args";
                // just hold off on this so far
            }

            if (appFileName != "launcher")
            {
                fullPath = Path.Combine(fullPath, "HoloPlayerApps");
            }
            else
            {
                args += " -shortIntro";
                if (Application.platform != RuntimePlatform.WindowsPlayer)
                {
                    var parentDir = Directory.GetParent(fullPath);
                    fullPath = parentDir.FullName;
                }
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    appFileName = "Launcher-Win";
                }
            }
            fullPath = Path.Combine(fullPath, appFileName + extension);
            Debug.Log("fullpath: " + fullPath);

            if (SceneManager.GetActiveScene().name == "HoloPlayLauncher")
            {
                args += " " + quitToLauncherArg;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                var p = new System.Diagnostics.ProcessStartInfo(fullPath, args);
                p.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                System.Diagnostics.Process.Start(p);
            }
            else
            {
                // var p = new System.Diagnostics.ProcessStartInfo("sh " + chaperonePath, '"' + fullPath + '"');
                string correctPath = "/Volumes/HOLOPLAYER";
                if (appFileName == "launcher")
                {
                    correctPath = Path.Combine(correctPath, "Launcher-Mac.app");
                    Debug.Log("launcher correct path: " + correctPath);
                }
                else
                {
                    correctPath = Path.Combine(correctPath, "HoloPlayerApps");
                    correctPath = Path.Combine(correctPath, appFileName + ".app");
                    Debug.Log("correct path: " + correctPath);
                }
                var p = new System.Diagnostics.ProcessStartInfo("open", "-a " + '"' + correctPath + '"' + args);
                System.Diagnostics.Process.Start(p);
            }
        }

        IEnumerator StartCountdown(string appFileName)
        {
            currentlyQuittingToLauncher = true;

            //disable depth plugin
            var depthPlugin = FindObjectOfType<depthPlugin>();

            //if depth plugin exists, save it's state incase the user cancels quitting
            bool previousDepthStatus = true;
            if (depthPlugin != null)
            {
                previousDepthStatus = depthPlugin.enabled;
                depthPlugin.enabled = false; //hopefully the app totally lets go of the plugin
            }

            countdown = countdownTime;
            while (currentlyQuittingToLauncher)
            {
                countdown -= Time.deltaTime;
                if (countdown <= 0)
                {
                    //quit
                    LoadApp(appFileName);
                    Application.Quit();
                }
                yield return null;
            }

            //shouldn't reach this point if the quitting completes
            //otherwise set the depthplugin back to where it was
            if (depthPlugin != null) depthPlugin.enabled = previousDepthStatus;
        }

        void OnGUI()
        {
            if (currentlyQuittingToLauncher)
            {
                GUI.skin.box.fontSize = HoloPlay.imguiFontSize;
                string str = "quitting";
                if (SceneManager.GetActiveScene().name == "HoloPlayLauncher")
                    str = "launching";

                int dotCount = Mathf.FloorToInt((countdownTime - countdown) * 3 / countdownTime);
                string dots = "";
                for (int i = 0; i < dotCount; i++)
                {
                    dots += ".";
                }

                GUI.Box(
                    HoloPlay.imguiRect,
                    str + dots
                );
            }
        }
    }
}
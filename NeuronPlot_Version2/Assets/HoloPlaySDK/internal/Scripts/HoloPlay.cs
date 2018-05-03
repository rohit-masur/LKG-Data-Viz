﻿//Copyright 2017 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace HoloPlaySDK
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class HoloPlay : MonoBehaviour
    {
        #region fields
        /// <summary>
        /// Release version of the SDK. 
        /// Stored as a float to allow comparisons (i.e. is this version > 0.1f ?)
        /// </summary>
        public static readonly float version = 0.42f;

        /// <summary>
        /// The HoloPlay holds a HoloPlayConfig from which it reads 
        /// all the values required for lenticular-izing the resulting image.
        /// </summary>
        private static HoloPlayConfig config;
        public static HoloPlayConfig Config
        {
            get
            {
                if (config == null)
                {
                    LoadConfig();
                }
                return config;
            }
            set { config = value; }
        }
        public readonly static string configFileName = "holoPlayConfig.json";
        public readonly static string configLenticularPrintName = "lenticularPrintConfig.json";
        public readonly static string configDirName = "holoPlaySDK_calibration";
        public static string relativePath
        {
            get { return Path.Combine(configDirName, configFileName); }
        }
        public static string lentilPrintPath
        {
            get { return Path.Combine(configDirName, configLenticularPrintName); }
        }

        /// <summary>
        /// The size of the HoloPlay Capture. 
        /// Use this, rather than the transform's scale, to resize the HoloPlay Capture.
        /// </summary>
        [Range(0.1f, 64)]
        public float size = 10;
        /// <summary>
        /// The field of view of the HoloPlay.
        /// Only available in Perspective mode.
        /// </summary>
        [Range(1, 75)]
        public float fov = 12.5f;
        /// <summary>
        /// The near clipping plane.
        /// Larger value = more distance *in front* of the focal plane is rendered.
        /// Use caution when using a high number for this,
        /// as objects too far in front or behind the focal plane will appear blurry and double-image.
        /// </summary>
        [Range(0f, 2f)]
        public float nearClip = 1f;
        /// <summary>
        /// The far clipping plane.
        /// Larger value = more distance *behind* the focal plane is rendered.
        /// Use caution when using a high number for this,
        /// as objects too far in front or behind the focal plane will appear blurry and double-image.
        /// </summary>
        [Range(0.01f, 3f)]
        public float farClip = 1.5f;
        /// <summary>
        /// Render in editor.
        /// When true, the lenticularization will happen even in edit-mode.
        /// For use in conjunction with the HoloPlayGameWindowMover.
        /// </summary>
        [SerializeField]
        public bool renderInEditor = true;

#if UNITY_EDITOR
        /// <summary>
        /// Gizmo Color.
        /// For use in editor only, sets the color of the gizmo in scene view.
        /// </summary>
        Color gizmoColor = Color.HSVToRGB(0.35f, 1, 1);
        Color gizmoColor0 = Color.HSVToRGB(0.5f, 1, 1);
        public bool gizmoShowAll;
        Vector2[] gizmoLogo = new[]
        {
            new Vector2(0, 1), new Vector2(2, 2), new Vector2(4, 1), new Vector2(2, 0),
            new Vector2(0, 2), new Vector2(2, 3), new Vector2(4, 2), new Vector2(2, 1),
        };
#endif

        /// <summary>
        /// Hidden away in debug mode, but this lets you turn off the HoloPlay
        /// even in play mode
        /// </summary>
        public bool fullyDisabled;

        //public fields that the HoloPlay must reference, 
        private RenderTexture rtMain;
        private RenderTexture rtFinal;
        private Material matFinal;
        private Camera cam;
        /// <summary>
        /// The Camera doing the rendering of the views.
        /// This camera moves around the focal pane, taking x number of renders 
        /// (where x is the number of views)
        /// </summary>
        /// <returns></returns>
        public Camera Cam
        {
            get { return cam; }
            private set { cam = value; }
        }
        private Camera camFinal;
        private int rtFinalSize = 2048;
        public static int tilesX { get; private set; }
        public static int tilesY { get; private set; }

        /// <summary>
        /// On View Render callback.
        /// This event fires once every time a view is rendered, just before the render happens.
        /// It passes an int which is the 0th indexed view being rendered (so from 0 to numViews-1).
        /// It fires one last time after the last render, passing the int numViews.
        /// </summary>
        public static Action<int> onViewRender;

        //the test textures
        [SerializeField]
        private Texture2D colorTestTex;
        [SerializeField]
        private Texture2D numTestTex;

        //to preface warnings and logs
        public static readonly string warningText = "[HoloPlay] ";

        //todo: perhaps remove
        public static Action onSaveConfig;
        public static Action onLoadConfig;

        public bool isReady { get; private set; }

        public bool configPrintout { get; private set; }
        float configPrintoutTimer;

        public bool autoHideCursor = true;

        //lenticular printing
        bool lentilPrintQueued = false;
        // float lentilPrintPitch;
        // float lentilPrintSeg;
        // int lentilPrintCount;

        //3d screenshot!
        bool screenshot3DQueued;

        //wiggle gif
        bool wiggleGifQueued;
        class GifSettings
        {
            public int cutoff = 8;
            public float sinStrength = 0.5f;
            public float fps = 45;
        }

        //quitting
        public static bool quitToLauncher { get; private set; }
        public readonly float quitTime = 0.75f;
        public float quitTimer { get; private set; }
        public bool quitting { get; private set; }
        public static readonly int imguiFontSize = 80;
        public static readonly Rect imguiRect = new Rect(80, 80, 800, 100);

        string[] comArgs;

        private static HoloPlay main;
        /// <summary>
        /// Static ref to the currently active HoloPlay.
        /// There may only be one active at a time, so this will always return the active one.
        /// </summary>
        public static HoloPlay Main
        {
            get
            {
                if (main != null) return main;
                main = FindObjectOfType<HoloPlay>();
                return main;
            }
            private set { main = value; }
        }
        #endregion

        #region start and onenable
        void OnEnable()
        {
            //setup the static ref
            Main = this;

            //tiles default
            tilesX = 4;
            tilesY = 8;

            //setup the main camera: this is the child that pivots around the focal pane
            Transform holoPlayCamChild = transform.Find("HoloPlay Camera");
            if (holoPlayCamChild == null)
            {
                holoPlayCamChild = new GameObject("HoloPlay Camera", typeof(Camera)).transform;
                holoPlayCamChild.parent = transform;
                var lc = holoPlayCamChild.GetComponent<Camera>();
                lc.clearFlags = CameraClearFlags.Color;
                lc.backgroundColor = Color.black;
            }
            Cam = holoPlayCamChild.GetComponent<Camera>();
            Cam.transform.hideFlags = HideFlags.NotEditable;

            //setup final camera. this is the camera which does the final render
            camFinal = GetComponent<Camera>();
            if (camFinal == null) gameObject.AddComponent<Camera>();
            camFinal.useOcclusionCulling = false;
            // camFinal.allowHDR = false;
            // camFinal.allowMSAA = false;
            camFinal.cullingMask = 0;
            camFinal.clearFlags = CameraClearFlags.Nothing;
            camFinal.backgroundColor = Color.black;
            camFinal.orthographic = true;
            camFinal.orthographicSize = 0.001f;
            camFinal.nearClipPlane = 0;
            camFinal.farClipPlane = 0.001f;
            camFinal.stereoTargetEye = StereoTargetEyeMask.None;

            //setup material to post-process the final cam
            matFinal = new Material(Shader.Find("Hidden/HoloPlay/HoloPlay Final"));
            matFinal.mainTexture = rtFinal;

            //setup textures used in the tests
            if (colorTestTex == null)
                colorTestTex = (Texture2D)Resources.Load("HoloPlay_colorTestTex__");
            if (numTestTex == null)
                numTestTex = (Texture2D)Resources.Load("HoloPlay_numTestTex__");

            //enforce the one-active-at-a-time rule
            var otherHoloPlay = FindObjectsOfType<HoloPlay>();
            if (otherHoloPlay.Length > 0)
            {
                bool displayMsg = false;
                for (int i = 0; i < otherHoloPlay.Length; i++)
                {
                    if (otherHoloPlay[i].gameObject != gameObject)
                    {
                        otherHoloPlay[i].gameObject.SetActive(false);
                        displayMsg = true;
                    }
                }
                if (displayMsg)
                {
                    Debug.LogWarning(warningText +
                        "Can only have one active HoloPlay at a time! disabling all others."
                    );
                }
            }

            //if the config isn't loaded yet, load it
            if (Config == null)
                LoadConfig();
            else
                ForceCalibrationRefresh();

            //read command line arguments to determine if we were launched by launcher
            comArgs = System.Environment.GetCommandLineArgs();
            foreach (var arg in comArgs)
            {
                if (!Application.isEditor)
                    Debug.Log("com arg: " + arg);
                if (arg == HoloPlayLauncher.quitToLauncherArg)
                {
                    quitToLauncher = true;
                }
            }
        }

        void Start()
        {
            //let whatever know its ready now
            isReady = true;

            StartCoroutine(ScanForHoloPlayJoystickEvery());

            if (Application.isPlaying && !Application.isEditor && autoHideCursor)
            {
                Cursor.visible = false;
            }
        }
        #endregion

        #region update and lateupdate
        void Update()
        {
            if (Application.isPlaying)
            {
                //the autohide cursor should still allow it to toggle on by hitting escape
                if (Input.GetKeyDown(KeyCode.Escape) && autoHideCursor && !Application.isEditor)
                {
                    Cursor.visible = !Cursor.visible;
                }

                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                    Input.GetKeyDown(KeyCode.F10))
                {
                    configPrintout = !configPrintout;
                }

                //exiting using the holoplay home button
                if (HoloPlayButton.GetButton(HP_Button.HOME))
                {
                    quitting = true;
                    quitTimer -= Time.deltaTime;
                    if (quitTimer <= 0)
                    {
                        //if from launcher
                        if (quitToLauncher)
                        {
                            if (!HoloPlayLauncher.Instance.currentlyQuittingToLauncher)
                                HoloPlayLauncher.Instance.StartLoadingApp("launcher");
                        }
                        //otherwise just quit the app
                        else
                        {
                            Application.Quit();
                        }
                    }
                }
                else
                {
                    quitting = false;
                    quitTimer = quitTime;
                }

                // if (Input.GetKeyDown(KeyCode.F9))
                // {
                //     StartCoroutine(LenticularPrint());
                // }

                if (Input.GetKeyDown(KeyCode.F6))
                {
                    screenshot3DQueued = true;
                }
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    wiggleGifQueued = true;
                }
            }
        }

        //the meat of the rendering is in here
        void LateUpdate()
        {
            float adjustedSize = GetAdjustedSize();
            //restore cameras to original state
            Cam.enabled = false;
            Cam.aspect = Config.screenW / Config.screenH;
            Cam.transform.position = transform.position + transform.forward * -adjustedSize;
            Cam.transform.localRotation = Quaternion.identity;
            Cam.nearClipPlane = adjustedSize - nearClip * size;
            Cam.farClipPlane = adjustedSize + farClip * size;
            Cam.fieldOfView = fov;
            Cam.orthographicSize = size * 0.5f;

            camFinal.enabled = true;
            camFinal.orthographicSize = 0.001f;

            //view cone for 3d screenshot
            float savedViewCone = Config.viewCone.Value;
            if (screenshot3DQueued)
            {
                Config.viewCone.Value = Config.viewCone.defaultValue;
            }

            //****************************
            //if not rendering views
            //****************************
            if (fullyDisabled || (!renderInEditor && !Application.isPlaying))
            {
                Cam.targetTexture = null;
                camFinal.enabled = false;
                Cam.enabled = true;
                HandleOffset(0, Config.verticalAngle);
                return;
            }

            //****************************
            //rendering views
            //****************************

            matFinal.mainTexture = rtFinal;
            camFinal.clearFlags = CameraClearFlags.SolidColor;
            Cam.clearFlags = CameraClearFlags.SolidColor;

            // if it's a test, cover the rtFinal in colortest first
            Graphics.SetRenderTarget(rtFinal);
            if ((int)Config.test == 1)
            {
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, rtFinal.width, rtFinal.height, 0);
                Graphics.DrawTexture(new Rect(0, 0, rtFinal.width, rtFinal.height), colorTestTex);
                GL.PopMatrix();
                Cam.backgroundColor = Color.clear;
            }
            else
            {
                GL.Clear(false, true, Color.black);
                Cam.backgroundColor = Color.black;
            }

            Graphics.SetRenderTarget(rtMain);

            //handle loop
            for (int i = 0; i < Config.numViews; i++)
            {
                //reset render texture
                Cam.targetTexture = rtMain;

                //offset or rotation
                HandleOffset(GetAngleAtView(i), Config.verticalAngle);

                //broadcast the onViewRender action
                if (onViewRender != null && Application.isPlaying)
                    onViewRender(i);

                //actually render~!
                Cam.Render();

                //copy to fullsize rt
                int ri = (tilesX * tilesY) - i - 1;
                int x = (i % tilesX) * rtMain.width;
                int y = (ri / tilesX) * rtMain.height;
                Rect rtRect = new Rect(x, y, rtMain.width, rtMain.height);
                if (rtMain.IsCreated() && rtFinal.IsCreated())
                {
                    Graphics.SetRenderTarget(rtFinal);
                    GL.PushMatrix();
                    GL.LoadPixelMatrix(0, rtFinal.width, rtFinal.height, 0);
                    Graphics.DrawTexture(rtRect, rtMain);
                    GL.PopMatrix();
                }
            }

            //sending variables to the shader

            //pitch
            float screenInches = (float)Config.screenW / Config.DPI;
            float newPitch = Config.pitch * screenInches;
            //account for tilt in measuring pitch horizontally
            newPitch *= Mathf.Cos(Mathf.Atan(1f / Config.slope));
            matFinal.SetFloat("pitch", newPitch);

            //tilt
            float newTilt = Config.screenH / (Config.screenW * Config.slope);
            matFinal.SetFloat("tilt", lentilPrintQueued ? 0 : newTilt);

            //center
            matFinal.SetFloat("center", Config.center);

            //numViews
            //during color test, force views down to 2
            float newNumViews = (int)Config.numViews;
            matFinal.SetFloat("numViews", newNumViews);

            //tiles
            matFinal.SetFloat("tilesX", tilesX);
            matFinal.SetFloat("tilesY", tilesY);

            //flip x
            matFinal.SetFloat("flipX", Config.flipImage);

            //flip y
            matFinal.SetFloat("flipY", Config.flipImageY);

            //flip subp
            matFinal.SetFloat("flipSubp", Config.flipSubp);

            //lentil disabling of subpixels
            matFinal.SetInt("subs", lentilPrintQueued ? 1 : 3);

            //broadcast the onViewRender action one last time, 
            //incase there is a need to change things before the final render
            if (onViewRender != null && Application.isPlaying)
                onViewRender((int)Config.numViews);

            //if a screenshot is queued, put it here
            if (screenshot3DQueued)
            {
                screenshot3DQueued = false;
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filename = Application.productName + "-3D-Scr-";
                int filenumber = 0;
                string finalPath = "";
                do
                {
                    finalPath = Path.Combine(path, filename + filenumber++ + ".png");
                }
                while (File.Exists(finalPath));

                //write a png
                Texture2D mainTexture = new Texture2D(rtFinal.width, rtFinal.height);
                RenderTexture.active = rtFinal;
                mainTexture.ReadPixels(new Rect(0, 0, rtFinal.width, rtFinal.height), 0, 0, false);
                var texBytes = mainTexture.EncodeToPNG();
                File.WriteAllBytes(finalPath, texBytes);
                print("saved " + Path.GetFileName(finalPath) + " to desktop");

                //set the view cone back
                Config.viewCone.Value = savedViewCone;
            }

            //wiggle gif
            if (wiggleGifQueued)
            {
                wiggleGifQueued = false;
                //if there is no streamingassetspath in the build, create one
                if (!Directory.Exists(Application.streamingAssetsPath))
                {
                    Directory.CreateDirectory(Application.streamingAssetsPath);
                }

                //read wigglegif settings if they're there
                string gifPath = Path.Combine(Application.streamingAssetsPath, "gifSettings.json");
                GifSettings gifSettings = new GifSettings();
                if (!File.Exists(gifPath))
                {
                    string gifSettingsJsonStr = JsonUtility.ToJson(gifSettings, true);
                    File.WriteAllText(gifPath, gifSettingsJsonStr);
                }
                else
                {
                    string gifSettingsJsonStr = File.ReadAllText(gifPath);
                    gifSettings = JsonUtility.FromJson<GifSettings>(gifSettingsJsonStr);
                }


                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filename = Application.productName + "-";
                int filenumber = 0;
                string finalPath = "";
                do
                {
                    finalPath = Path.Combine(path, filename + filenumber++ + ".gif");
                }
                while (File.Exists(finalPath));

                var gifEncoder = new uGIF.GIFEncoder();
                var ms = new MemoryStream();
                gifEncoder.useGlobalColorTable = true;
                gifEncoder.repeat = 0;
                gifEncoder.Start(ms);
                Texture2D gifTex = new Texture2D(rtMain.width, rtMain.height);

                int numViews = (int)HoloPlay.Config.numViews;
                int cutoff = 14;
                for (int i = 0; i < numViews * 2; i++)
                {
                    int r = i;
                    if (i >= numViews)
                    {
                        r = numViews * 2 - i - 1;
                    }
                    if (r < cutoff || numViews - r - 1 < cutoff)
                    {
                        continue;
                    }
                    float newR = Mathf.InverseLerp(cutoff, numViews - cutoff - 1, r);
                    float fpsMod = Mathf.Sin(newR * Mathf.PI) * 0.5f + 0.5f;
                    gifEncoder.FPS = 30f * fpsMod;
                    int x = (r % tilesX) * rtMain.width;
                    int y = (r / tilesX) * rtMain.height;
                    RenderTexture.active = rtFinal;
                    gifTex.ReadPixels(new Rect(x, y, rtMain.width, rtMain.height), 0, 0, false);
                    var gifImage = new uGIF.Image(gifTex);
                    gifImage.Flip();
                    gifImage.ResizeBilinear((int)(rtMain.height * Config.screenW / Config.screenH), rtMain.height);
                    gifEncoder.AddFrame(gifImage);
                }
                gifEncoder.Finish();

                var gifBytes = ms.GetBuffer();
                File.WriteAllBytes(finalPath, gifBytes);
                print("saved " + Path.GetFileName(finalPath) + " to desktop");
            }
        }
        #endregion

        #region holoplay hid handling
        IEnumerator ScanForHoloPlayJoystickEvery()
        {
            WaitForSeconds waitTime = new WaitForSeconds(5f);
            while (true)
            {
                HoloPlayButton.ScanForHoloPlayerJoystick();
                yield return waitTime;
            }
        }
        #endregion

        #region onrenderimage
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (renderInEditor || Application.isPlaying)
            {
                Graphics.Blit(matFinal.mainTexture, dest, matFinal);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }
        #endregion

        #region ondisable
        void OnDisable()
        {
            //destroy the rendertextues and materials we created
            if (!Application.isPlaying)
            {
                rtMain.Release();
                DestroyImmediate(rtMain);
            }
            rtFinal.Release();
            DestroyImmediate(rtFinal);
            DestroyImmediate(matFinal);
        }
        #endregion

        #region misc methods for holoplay positioning
        ///returns the cam size after adjustment for FOV. this is essentially the real cam distance from center.
        public float GetAdjustedSize()
        {
            if (Cam.orthographic)
                return 0;
            return size * 0.5f / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        }

        float GetAngleAtView(float viewNum)
        {
            return -Config.viewCone * 0.5f + (float)viewNum / (Config.numViews - 1) * Config.viewCone;
        }

        void HandleOffset(float horizontalOffset, float verticalOffset)
        {
            float adjustedSize = GetAdjustedSize();

            //start from scratch
            Cam.ResetProjectionMatrix();
            Cam.transform.localRotation = Quaternion.identity;

            //orthographic or regular perspective
            if (Cam.orthographic || wiggleGifQueued)
            {
                Cam.transform.position = transform.position;
                if (wiggleGifQueued)
                    Cam.transform.position = transform.forward * -adjustedSize;
                Cam.transform.RotateAround(transform.position, transform.up, -horizontalOffset);
                return;
            }

            //perspective correction
            //imagine triangle from pivot center, to camera, to camera's ideal new position. 
            //offAngle is angle at the pivot center. solve for offsetX
            //tan(offAngle) = offX / camDist
            //offX = camDist * tan(offAngle)
            float offsetX = adjustedSize * Mathf.Tan(horizontalOffset * Mathf.Deg2Rad);
            float offsetY = adjustedSize * Mathf.Tan(verticalOffset * Mathf.Deg2Rad);
            Vector3 camPos = transform.position;
            camPos += transform.right * offsetX;
            camPos += transform.up * offsetY;
            camPos += transform.forward * -adjustedSize;
            Cam.transform.position = camPos;

            //create var for new cam projection matrix
            Matrix4x4 matrix = Cam.projectionMatrix;

            //i.e. offset of 1 moves the camera 1/2 pane width to the right, so account for that
            matrix[0, 2] = -2 * offsetX / (size * Cam.aspect);
            matrix[1, 2] = -2 * offsetY / size;

            //tada
            Cam.projectionMatrix = matrix;
        }

        ///returns the corners of the camera frustum as vector3s at dist
        public Vector3[] GetFrustumCorners(Camera cam, float dist, bool relative = false)
        {
            //make sure the dist is actually within the camera's clipping area
            dist = Mathf.Clamp(dist, cam.nearClipPlane, cam.farClipPlane);

            //get corners
            Vector3[] frustumCorners = new[]
            {
                cam.ViewportToWorldPoint(new Vector3(0, 0, dist)),
                cam.ViewportToWorldPoint(new Vector3(0, 1, dist)),
                cam.ViewportToWorldPoint(new Vector3(1, 1, dist)),
                cam.ViewportToWorldPoint(new Vector3(1, 0, dist))
            };

            if (!relative)
                return frustumCorners;

            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = transform.InverseTransformPoint(frustumCorners[i]);
                frustumCorners[i] /= size;
            }
            return frustumCorners;
        }
        #endregion

        #region saving and loading config
        public static void SaveConfigToFile()
        {
            string filePath;
            if (!utils.getConfigPathToFile(relativePath, out filePath))
            {
                //todo: throw a big, in-game visible warning if this fails
                return;
            }
            //todo: just checking how this looks, REMOVE
            Debug.Log(filePath + " \n is the filepath");

            //never save test mode as being on.
            float testMode = Config.test;
            Config.test.Value = 0;
            string json = JsonUtility.ToJson(Config, true);
            Config.test.Value = testMode;

            File.WriteAllText(filePath, json);
            Debug.Log(warningText + "Config saved!");

            if (onSaveConfig != null)
                onSaveConfig();

#if UNITY_EDITOR
            if (UnityEditor.PlayerSettings.defaultScreenWidth != (int)Config.screenW ||
                UnityEditor.PlayerSettings.defaultScreenHeight != (int)Config.screenH)
            {
                UnityEditor.PlayerSettings.defaultScreenWidth = (int)Config.screenW;
                UnityEditor.PlayerSettings.defaultScreenHeight = (int)Config.screenH;
            }
#endif
        }

        public static void LoadConfig(bool lentilPrint = false)
        {
            string filePath;
            if (!utils.getConfigPathToFile(lentilPrint ? lentilPrintPath : relativePath, out filePath))
            {
                //todo: throw a huge warning
                Debug.Log(warningText + "Config file not found!");
                ConfigStartup(new HoloPlayConfig());
                return;
            }

            string configStr = File.ReadAllText(filePath);
            if (configStr.IndexOf('{') < 0 || configStr.IndexOf('}') < 0)
            {
                //if the file exists but is unpopulated by any info, don't try to parse it
                //this is a bug with jsonUtility that it doesn't know how to handle a fully empty text file >:(
                Debug.Log(warningText + "Config file not found!");
                ConfigStartup(new HoloPlayConfig());
                return;
            }

            //if it's made it this far, just load it
            Debug.Log(warningText + "Config loaded! loaded from " + filePath);
            ConfigStartup(JsonUtility.FromJson<HoloPlayConfig>(configStr));
            return;
        }

        /// <summary>
        /// Updates the config to the new one, and runs the necessary processes
        /// </summary>
        /// <param name="configIn"></param>
        static void ConfigStartup(HoloPlayConfig configIn)
        {
            Config = configIn;
            //make sure test value is always 0 unless specified by calibrator
            Config.test.Value = 0;
            if (onLoadConfig != null) onLoadConfig();
            if (HoloPlay.Main != null)
                HoloPlay.Main.ForceCalibrationRefresh();
        }

        void ForceCalibrationRefresh()
        {
            tilesX = 4;
            tilesY = 8;
            rtFinalSize = 2048;
            if ((int)Config.numViews > 32)
            {
                tilesX = 8;
                tilesY = 16;
                rtFinalSize = 4096;
            }

            //setup render texture
            rtMain = new RenderTexture(rtFinalSize / tilesX, rtFinalSize / tilesY, 24)
            {
                wrapMode = TextureWrapMode.Clamp,
                autoGenerateMips = false,
                useMipMap = false
            };
            rtMain.Create();
            rtFinal = new RenderTexture(rtFinalSize, rtFinalSize, 0)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                autoGenerateMips = false,
                useMipMap = false
            };
            rtFinal.Create();

            //if the config is already set, return out
            if (Screen.width == Config.screenW && Screen.height == Config.screenH)
                return;

            if (Application.isEditor)
            {
                // Debug.LogWarning(holoPlayWarning + "Set game window to 2048x1536 for proper aspect in editor");
                //todo: make the game window mover set the the resolution properly so we don't get this error
            }
            else
            {
                //trying to force display to be the holoplayer's
                int screenW = (int)Config.screenW;
                int screenH = (int)Config.screenH;
                int i = 0;
                List<int> matchingResolutionDisplays = new List<int>();

                foreach (var display in Display.displays)
                {
                    if (display.systemWidth == screenW &&
                        display.systemHeight == screenH)
                    {
                        matchingResolutionDisplays.Add(i);
                    }
                    i++;
                }

                //if one of the displays has the correct resolution
                //use that one. any more than 1 is useless because we can't predict
                //which one is the holoplayer
                if (matchingResolutionDisplays.Count == 1)
                {
                    i = matchingResolutionDisplays[0];
                    if (PlayerPrefs.GetInt("UnitySelectMonitor") == i)
                    {
                        //you're good
                        Screen.SetResolution((int)Config.screenW, (int)Config.screenH, true);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("UnitySelectMonitor", i);
                        StartCoroutine(QuickReset());
                    }


                    // var fakeW = 800;
                    // var fakeH = 600;
                    // for (int j = 0; j < Screen.resolutions.Length; j++)
                    // {
                    //     var res = Screen.resolutions[j];
                    //     if (res.width != screenW &&
                    //         res.height != screenH)
                    //     {
                    //         //we want to switch to a possible resolution right now
                    //         //that is not the correct one. this way
                    //         //we can switch back. i know this is hacky
                    //         fakeW = res.width;
                    //         fakeH = res.height;
                    //         break;
                    //     }
                    // }
                }
                else
                {
                    Screen.SetResolution((int)Config.screenW, (int)Config.screenH, true);
                }
            }

#if UNITY_EDITOR
            if (UnityEditor.PlayerSettings.defaultScreenWidth != (int)Config.screenW ||
                UnityEditor.PlayerSettings.defaultScreenHeight != (int)Config.screenH)
            {
                UnityEditor.PlayerSettings.defaultScreenWidth = (int)Config.screenW;
                UnityEditor.PlayerSettings.defaultScreenHeight = (int)Config.screenH;
            }
#endif
        }

        IEnumerator QuickReset()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer)
                yield break;

            var dps = FindObjectsOfType<depthPlugin>();
            foreach (var dp in dps)
            {
                dp.enabled = false;
            }

            //for good measure
            yield return null;

            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            print(p.MainModule.FileName);

            string args = "";
            foreach (var arg in comArgs)
            {
                args += arg + " ";
            }
            print(args);
            System.Diagnostics.Process.Start(p.MainModule.FileName, args);
            Application.Quit();
        }

        #endregion

        #region lenticular printing
        // IEnumerator LenticularPrint()
        // {
        //     LoadConfig(true);
        //     lentilPrintQueued = true;
        //     yield return null;

        //     //Change seg and count to do tests where multiple lenticular dpis are printed out.
        //     float curPitch = config.pitch;

        //     if (!Application.isEditor)
        //     {
        //         lentilPrintCount = 1;
        //         lentilPrintSeg = 0;
        //     }
        //     else
        //     {
        //         curPitch = lentilPrintPitch;
        //     }

        //     Texture2D[] tex = new Texture2D[4];
        //     for (int i = 0; i < lentilPrintCount; i++)
        //     {
        //         config.pitch.Value = curPitch + lentilPrintSeg * i;

        //         yield return null;
        //         yield return null;

        //         if (lentilPrintCount == 4)
        //             tex[i] = new Texture2D((int)config.screenW, (int)config.screenH);

        //         var p = config.pitch.Value.ToString("0.00");
        //         string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), p + ".png");
        //         //todo: when using lenticular print, gotta uncomment this part below
        //         // ScreenCapture.CaptureScreenshot(fileName);

        //         yield return null;

        //         if (lentilPrintCount == 4)
        //             tex[i].LoadImage(File.ReadAllBytes(fileName));

        //         print("wrote Screenie " + p + ".png");

        //         yield return null;
        //         yield return null;
        //     }

        //     if (lentilPrintCount == 4)
        //     {
        //         Texture2D finalTex = new Texture2D((int)config.screenW * 2, (int)config.screenH * 2);
        //         finalTex.SetPixels(0, 0, tex[0].width, tex[0].height, tex[0].GetPixels(), 0);
        //         finalTex.SetPixels(tex[1].width, 0, tex[1].width, tex[1].height, tex[1].GetPixels(), 0);
        //         finalTex.SetPixels(0, tex[2].height, tex[2].width, tex[2].height, tex[2].GetPixels(), 0);
        //         finalTex.SetPixels(tex[3].width, tex[3].height, tex[3].width, tex[3].height, tex[3].GetPixels(), 0);

        //         string fileNamer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "collection.png");
        //         File.WriteAllBytes(fileNamer, finalTex.EncodeToPNG());
        //     }

        //     yield return null;
        //     lentilPrintQueued = false;
        //     LoadConfig();
        //     yield return null;
        // }
        #endregion

        #region gui
        void OnGUI()
        {
            if (quitting)
            {
                if (!quitToLauncher || !HoloPlayLauncher.Instance.currentlyQuittingToLauncher)
                {
                    //will return this if necessary
                    // GUI.skin.box.fontSize = imguiFontSize;
                    // int dotCount = (int)((quitTime - quitTimer) * 4 / quitTime);
                    // string dots = "";
                    // for (int i = 0; i < dotCount; i++)
                    // {
                    //     dots += ".";
                    // }
                    // GUI.Box(imguiRect, "quitting" + dots);
                }
            }

            if (configPrintout)
                GUIConfigPrintout();
        }

        void GUIConfigPrintout()
        {
            var guiRect = new Rect(Screen.width * 0.3f, 0, Screen.width * 0.45f, Screen.height);
            var style = new GUIStyle();
            style.fontSize = 60;
            GUI.backgroundColor = Color.black;
            GUI.color = Color.white;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleLeft;
            style.richText = true;
            style.normal.background = Texture2D.whiteTexture;
            style.padding = new RectOffset(120, 20, 0, 0);
            style.stretchWidth = true;
            style.stretchHeight = true;
            style.wordWrap = true;

            var printStr = "<color=green><size=120>HoloPlay SDK</size></color>";
            printStr += "\n<size=100>" + version + "</size>\n\n";

            var configFields = typeof(HoloPlayConfig).GetFields();
            for (int i = 0; i < configFields.Length; i++)
            {
                if (configFields[i].FieldType == typeof(HoloPlayConfig.ConfigValue))
                {
                    var configValue = (HoloPlayConfig.ConfigValue)configFields[i].GetValue(HoloPlay.Config);
                    printStr += configValue.name + ": " + configValue.Value.ToString("0.########") + "\n";
                }
            }

            string filePathToPrint;
            if (utils.getConfigPathToFile(relativePath, out filePathToPrint))
                printStr += "\n<size=40>config path:\n" + filePathToPrint + "</size>";
            else
                printStr += "\n<size=40>no config path found!</size>";

            GUI.Box(guiRect, printStr, style);
        }
        #endregion

        #region editor stuff
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            HandleOffset(0, Config.verticalAngle);
            //get corners
            List<Vector3> fc = new List<Vector3>();
            fc.AddRange(GetFrustumCorners(cam, cam.nearClipPlane));
            fc.AddRange(GetFrustumCorners(cam, cam.farClipPlane));

            DrawVolume(fc);

            //focal point
            Gizmos.color = gizmoColor0;
            var foc = GetFrustumCorners(Cam, GetAdjustedSize());
            for (int i = 0; i < foc.Length; i++)
            {
                var i0 = i != 0 ? i - 1 : foc.Length - 1;
                var i1 = i != foc.Length - 1 ? i + 1 : 0;

                var f = foc[i];
                var f0 = Vector3.Lerp(foc[i], foc[i0], 0.1f);
                var f1 = Vector3.Lerp(foc[i], foc[i1], 0.1f);

                Gizmos.DrawLine(f, f0);
                Gizmos.DrawLine(f, f1);
            }

            //arrow
            if (UnityEditor.SceneView.lastActiveSceneView.camera != null)
            {
                var forward = transform.forward * size;
                var aRelPos = -forward * nearClip;
                var aPos = aRelPos + transform.position - forward * 0.1f;
                var editorCamPos = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
                var cross = Vector3.Cross(editorCamPos - transform.position, aRelPos - forward * 0.06f);
                cross = cross.normalized * size * 0.06f;

                Gizmos.DrawLine(aPos, aPos - forward * 0.24f);
                Gizmos.DrawLine(aPos, aPos - forward * 0.06f + cross);
                Gizmos.DrawLine(aPos, aPos - forward * 0.06f - cross);
            }

            //logo
            Gizmos.color = new Color(0.8f, 0.2f, 1, 0.8f);
            var gl = new List<Vector3>();
            var s = Vector3.Distance(fc[0], fc[1]);

            foreach (var g in gizmoLogo)
            {
                gl.Add(transform.rotation * ((g + new Vector2(1, 1)) * s * 0.02f) + fc[0]);
            }
            foreach (var g in gizmoLogo)
            {
                gl.Add(transform.rotation * ((g + new Vector2(1, 2)) * s * 0.02f) + fc[0]);
            }
            for (int i = 0; i < 4; i++)
            {
                var i0 = i != 3 ? i + 1 : 0;
                Gizmos.DrawLine(gl[i], gl[i0]);
                Gizmos.DrawLine(gl[i + 4], gl[i0 + 4]);
            }

            Gizmos.DrawLine(gl[0], gl[4]);
            Gizmos.DrawLine(gl[2], gl[6]);

            //range
            //this one will definitely be removed but for now in Debug you can activate gizmoShowAll to see the range of the cameras
            //it seems to take a slight toll on the editor processing so i turn it off by default
            if (Cam.orthographic || !gizmoShowAll)
                return;

            Gizmos.color = new Color(0.8f, 1, 0, 0.4f);
            var rn = new List<Vector3>();
            var rf = new List<Vector3>();

            HandleOffset(GetAngleAtView(0), Config.verticalAngle);
            rn.AddRange(new[]
            {
                cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane)),
                cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane)),
            });
            rf.AddRange(new[]
            {
                cam.ViewportToWorldPoint(new Vector3(1, 0, cam.farClipPlane)),
                cam.ViewportToWorldPoint(new Vector3(1, 1, cam.farClipPlane)),
            });

            HandleOffset(GetAngleAtView(config.numViews - 1), config.verticalAngle);
            rn.AddRange(new[]
            {
                cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane)),
                cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane)),
            });
            rf.AddRange(new[]
            {
                cam.ViewportToWorldPoint(new Vector3(0, 0, cam.farClipPlane)),
                cam.ViewportToWorldPoint(new Vector3(0, 1, cam.farClipPlane)),
            });

            Gizmos.DrawLine(rn[0], Vector3.Lerp(rn[0], rn[1], 0.2f));
            Gizmos.DrawLine(rn[0], Vector3.Lerp(rn[0], foc[0], 0.2f));
            Gizmos.DrawLine(rn[0], Vector3.Lerp(rn[0], fc[0], 0.2f));

            Gizmos.DrawLine(rn[1], Vector3.Lerp(rn[1], rn[0], 0.2f));
            Gizmos.DrawLine(rn[1], Vector3.Lerp(rn[1], foc[1], 0.2f));
            Gizmos.DrawLine(rn[1], Vector3.Lerp(rn[1], fc[1], 0.2f));

            Gizmos.DrawLine(rn[2], Vector3.Lerp(rn[2], rn[3], 0.2f));
            Gizmos.DrawLine(rn[2], Vector3.Lerp(rn[2], foc[3], 0.2f));
            Gizmos.DrawLine(rn[2], Vector3.Lerp(rn[2], fc[3], 0.2f));

            Gizmos.DrawLine(rn[3], Vector3.Lerp(rn[3], rn[2], 0.2f));
            Gizmos.DrawLine(rn[3], Vector3.Lerp(rn[3], foc[2], 0.2f));
            Gizmos.DrawLine(rn[3], Vector3.Lerp(rn[3], fc[2], 0.2f));

            Gizmos.DrawLine(rf[0], Vector3.Lerp(rf[0], rf[1], 0.2f));
            Gizmos.DrawLine(rf[0], Vector3.Lerp(rf[0], foc[3], 0.2f));
            Gizmos.DrawLine(rf[0], Vector3.Lerp(rf[0], fc[3 + 4], 0.2f));

            Gizmos.DrawLine(rf[1], Vector3.Lerp(rf[1], rf[0], 0.2f));
            Gizmos.DrawLine(rf[1], Vector3.Lerp(rf[1], foc[2], 0.2f));
            Gizmos.DrawLine(rf[1], Vector3.Lerp(rf[1], fc[2 + 4], 0.2f));

            Gizmos.DrawLine(rf[2], Vector3.Lerp(rf[2], rf[3], 0.2f));
            Gizmos.DrawLine(rf[2], Vector3.Lerp(rf[2], foc[0], 0.2f));
            Gizmos.DrawLine(rf[2], Vector3.Lerp(rf[2], fc[0 + 4], 0.2f));

            Gizmos.DrawLine(rf[3], Vector3.Lerp(rf[3], rf[2], 0.2f));
            Gizmos.DrawLine(rf[3], Vector3.Lerp(rf[3], foc[1], 0.2f));
            Gizmos.DrawLine(rf[3], Vector3.Lerp(rf[3], fc[1 + 4], 0.2f));

            HandleOffset(0, config.verticalAngle);
        }

        /// <summary>
        /// Draws a 6 sided gizmo shape, given the 8 corners (clockwise front to clockwise back)
        /// </summary>
        /// <param name="v"></param>
        void DrawVolume(List<Vector3> v)
        {
            if (v.Count != 8) return;

            //draw near square
            Gizmos.DrawLine(v[0], v[1]);
            Gizmos.DrawLine(v[1], v[2]);
            Gizmos.DrawLine(v[2], v[3]);
            Gizmos.DrawLine(v[3], v[0]);

            //draw far square
            Gizmos.DrawLine(v[0 + 4], v[1 + 4]);
            Gizmos.DrawLine(v[1 + 4], v[2 + 4]);
            Gizmos.DrawLine(v[2 + 4], v[3 + 4]);
            Gizmos.DrawLine(v[3 + 4], v[0 + 4]);

            //connect them
            Gizmos.DrawLine(v[0], v[0 + 4]);
            Gizmos.DrawLine(v[1], v[1 + 4]);
            Gizmos.DrawLine(v[2], v[2 + 4]);
            Gizmos.DrawLine(v[3], v[3 + 4]);
        }
#endif
        #endregion
    }
}
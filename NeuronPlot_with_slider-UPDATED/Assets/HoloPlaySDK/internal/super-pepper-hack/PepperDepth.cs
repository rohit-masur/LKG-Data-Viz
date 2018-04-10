using System;
using System.Collections.Generic;

using HoloPlaySDK;

using UnityEngine;

public class PepperDepth : MonoBehaviour
{
    depthPlugin d;
    Material mat;
    Texture2D tex;
    int tipPixelCount;
    bool foundTip;
    Texture2D dt;
    Color32[] depthColors;
    Color32[] finalColors;
    Color c_darkGreen = Color.green * 0.3f;
    Color c_red = Color.red;
    Color c_blue = Color.blue;
    Color col;
    // float z;
    int texW;
    Transform tipIndicator;
    Material tipMat;
    List<ushort> zBuffer = new List<ushort>();

    public List<Vector3> TipPositions { get; private set; }

    public Vector2 guiBoxSize = new Vector2(250, 40);
    Action GUIAction;

    public bool reverse;

    void Start()
    {
        d = FindObjectOfType<depthPlugin>();
        if (d == null)
        {
            d = gameObject.AddComponent<depthPlugin>();
        }
        dt = d.getDepthTexture();
        tex = new Texture2D(dt.width, dt.height, TextureFormat.RGBA32, false);
        depthColors = new Color32[dt.width * dt.height];
        finalColors = new Color32[dt.width * dt.height];
        for (int i = 0; i < finalColors.Length; i++)
        {
            finalColors[i] = Color.cyan * 0.1f;
        }
        texW = tex.width;
        TipPositions = new List<Vector3>();

        var mr = GetComponent<MeshRenderer>();
        if (mr)
        {
            mat = new Material(Shader.Find("Unlit/Texture"));
            mat.mainTexture = tex;
            mr.material = mat;

            tipIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            tipIndicator.parent = transform;
            tipIndicator.localScale = Vector3.one * 0.02f;
            var tipMR = tipIndicator.GetComponent<MeshRenderer>();
            tipMat = new Material(Shader.Find("Unlit/Color"));
            tipMR.material = tipMat;
        }
    }

    void OnDestroy()
    {
        if (mat)
            Destroy(mat);

        if (tipMat)
            Destroy(tipMat);
    }

    void Update()
    {
        GenerateTipTex();
    }

    void GenerateTipTex()
    {
        dt = d.getDepthTexture();
        depthColors = dt.GetPixels32();
        // float z = 0;
        col = c_darkGreen;
        tipPixelCount = 0;
        if (tipMat != null)
        {
            tipMat.color = c_red;
        }
        foundTip = false;
        TipPositions.Clear();
        int tipWidth = (int) HoloPlay.Config.pepperMinTipWidth.Value;

        GUIAction = null;

        var iStart = reverse ? HoloPlay.Config.yBounds[0] : HoloPlay.Config.yBounds[1];
        var iEnd = reverse ? HoloPlay.Config.yBounds[1] : HoloPlay.Config.yBounds[0];
        var iSign = reverse ? 1 : -1;
        for (int i = iStart;
            (i >= iEnd && !reverse) || (i <= iEnd && reverse); i += iSign)
        {
            for (int j = HoloPlay.Config.xBounds[0]; j < HoloPlay.Config.xBounds[1]; j++)
            {
                // z = dt.GetPixel(j, i).r;
                var z = utils.colorToDepth16(depthColors[xyTo1D(j, i)]);
                col = c_darkGreen;

                // if (i == dt.height / 2 && j == dt.width / 2)
                // {
                //     print(z);
                //     GUIAction += () => GUI.Label(new Rect(20, 20, guiBoxSize.x, guiBoxSize.y), z.ToString());
                // }

                if (z < HoloPlay.Config.pepperThreshold && z > 10)
                {
                    tipPixelCount++;
                    col = Color.Lerp(c_red, c_blue, z / HoloPlay.Config.pepperThreshold);
                }
                else
                {
                    tipPixelCount = 0;
                }

                if (tipPixelCount > tipWidth && !foundTip)
                {
                    //assume x is in the midpoint of the "blob" lol
                    int newX = j - tipWidth / 2;

                    //smoothing z
                    zBuffer.Add(z);
                    foreach (var zzz in zBuffer)
                    {
                        z += zzz;
                    }

                    z /= (ushort)(zBuffer.Count + 1);

                    Vector3 posXYZ = new Vector3(
                        (float) newX / texW - 0.5f,
                        (float) i / tex.height - 0.5f,
                        z
                    );

                    if (tipIndicator != null)
                    {
                        tipIndicator.localPosition = (Vector2) posXYZ;
                        tipMat.color = Color.green;
                    }

                    foundTip = true;
                    // TipPositions.Add(posXYZ);
                    TipPositions.Add(new Vector3(posXYZ.x, posXYZ.z, posXYZ.y));
                }

                if (tipMat != null)
                    finalColors[xyTo1D(j, i)] = col;
            }
        }

        if (tipMat != null)
        {
            tex.SetPixels32(finalColors);
            tex.Apply(false);
        }

        if ((!foundTip && zBuffer.Count > 0) ||
            (foundTip && zBuffer.Count > HoloPlay.Config.zSmoothCycles))
            zBuffer.RemoveAt(0);
    }

    void OnGUI()
    {
        if (GUIAction != null)
        {
            GUIAction();
        }
    }

    int xyTo1D(int x, int y)
    {
        return (x % texW) + (y * texW);
    }
}
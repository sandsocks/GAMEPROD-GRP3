using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class squigglytext: MonoBehaviour
{
    public enum Mode { Global, Selected }
    [Header("Mode Settings")]
    public Mode mode = Mode.Global;
    public List<TMP_Text> selectedTexts = new();  

    [Header("Animation Settings")]
    public int frames = 4;          
    public float frameRate = 8f;   
    public float jitter = 0.5f;     
    public bool autoRefresh = true; 

    private float timer;
    private int currentFrame;
    private Dictionary<TMP_Text, Vector3[][]> textFrames = new();

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        textFrames.Clear();

        if (mode == Mode.Global)
        {
            TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
            foreach (TMP_Text tmp in allTexts)
                RegisterText(tmp);
        }
        else
        {
            foreach (TMP_Text tmp in selectedTexts)
                if (tmp != null)
                    RegisterText(tmp);
        }
    }

    void RegisterText(TMP_Text tmp)
    {
        if (tmp == null || textFrames.ContainsKey(tmp)) return;
        textFrames[tmp] = GenerateFrames(tmp);
    }

    Vector3[][] GenerateFrames(TMP_Text tmp)
    {
        tmp.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmp.textInfo;
        if (textInfo.meshInfo.Length == 0)
            return new Vector3[0][];

        Vector3[] baseVerts = textInfo.meshInfo[0].vertices;
        if (baseVerts == null || baseVerts.Length == 0)
            return new Vector3[0][];

        Vector3[][] framesArray = new Vector3[frames][];

        for (int f = 0; f < frames; f++)
        {
            var vertsCopy = new Vector3[baseVerts.Length];

            for (int i = 0; i < baseVerts.Length; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-jitter, jitter),
                    Random.Range(-jitter, jitter),
                    0f
                );
                vertsCopy[i] = baseVerts[i] + offset;
            }

            framesArray[f] = vertsCopy;
        }

        return framesArray;
    }


    void Update()
    {
        if (autoRefresh && mode == Mode.Global && Time.frameCount % 120 == 0)
        {
            // Check for new TMP texts that were added at runtime
            TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
            foreach (TMP_Text tmp in allTexts)
                if (!textFrames.ContainsKey(tmp))
                    RegisterText(tmp);
        }

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % frames;
            AnimateAll();
        }
    }

    void AnimateAll()
    {
        foreach (var entry in textFrames)
        {
            TMP_Text tmp = entry.Key;
            if (tmp == null) continue;

            Vector3[][] framesArray = entry.Value;
            if (framesArray == null || framesArray.Length == 0) continue;

            tmp.ForceMeshUpdate();
            TMP_TextInfo textInfo = tmp.textInfo;

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                var verts = meshInfo.vertices;

                Vector3[] frameVerts = framesArray[currentFrame];
                if (frameVerts.Length == verts.Length)
                {
                    for (int v = 0; v < verts.Length; v++)
                        verts[v] = frameVerts[v];
                }

                meshInfo.mesh.vertices = verts;
                tmp.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }

    // Public API
    public void AddText(TMP_Text tmp)
    {
        if (!textFrames.ContainsKey(tmp))
            RegisterText(tmp);
    }

    public void RemoveText(TMP_Text tmp)
    {
        if (textFrames.ContainsKey(tmp))
            textFrames.Remove(tmp);
    }

    public void RefreshAll()
    {
        Initialize();
    }
}

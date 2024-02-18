using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtrudeIndexes : MonoBehaviour
{
    List<Vector3> mCurrentPositions;
    float[] mDeltaOffets;
    float[] mOriginalDeltaOffets;
    float[] mDeltaPosition;
    Mesh mMesh;

    float OffsetX;
    float OffsetY;

    float MaxX;
    float MaxY;
	
	int screenshotCount = 1;
    public bool GenerateFramesOutput = false;
    public bool GenerateWithTransparency = false;

    public float currentTime = 0.0f;
    public  int SmoothCount = 1;
	public float targetLength = 5.0f;
	public float fps = 30.0f;
    int frameskip = 5;

    void Start()
    {
        Random.InitState(0);
        Generate();
        DrawPlanes(SmoothCount);
    }

    private void Generate()
    {
        mMesh = GetComponent<MeshFilter>().mesh;
        CalulateVertRanges();

        mCurrentPositions = new List<Vector3>(mMesh.vertexCount);
        mOriginalDeltaOffets = new float[mMesh.vertexCount];
        mDeltaOffets = new float[mMesh.vertexCount];
        mDeltaPosition = new float[mMesh.vertexCount];


        for (int i = 0; i < mCurrentPositions.Capacity; i++)
        {
            mOriginalDeltaOffets[i] = Random.Range(0.0f, 90.0f);
        }
        mDeltaOffets = mOriginalDeltaOffets;
    }

    private void DrawPlanes(int smoothCount)
    {
        mMesh = GetComponent<MeshFilter>().mesh;

        float[] mPrevDeltaOffets = mOriginalDeltaOffets;


        for (int smooth = 0; smooth < smoothCount; smooth++)
        {
            int lastpos = mCurrentPositions.Capacity - 1;

            mDeltaOffets[0] = (mPrevDeltaOffets[0] + mPrevDeltaOffets[1]) / 2.0f;

            for (int i = 1; i < lastpos; i++)
            {
                mDeltaOffets[i] = (mPrevDeltaOffets[i - 1] + mPrevDeltaOffets[i] + mPrevDeltaOffets[i + 1]) / 3.0f;
            }

            mDeltaOffets[lastpos] = (mPrevDeltaOffets[lastpos] + mPrevDeltaOffets[lastpos-1]) / 2.0f;

            mPrevDeltaOffets = mDeltaOffets;
        }

        mDeltaOffets[0] = 0.0f;

        mCurrentPositions.Clear();
        for (int i = 0; i < mCurrentPositions.Capacity; i++)
        {
            Vector2 vertPos = GetVertRand(mMesh.vertices[i]);
            mDeltaPosition[i] = Mathf.PerlinNoise(vertPos.x, vertPos.y) * 0.1f;
            mCurrentPositions.Add(new Vector3(mDeltaPosition[i] + Mathf.Sin(mDeltaOffets[i]) * 0.001f, mMesh.vertices[i].y, mMesh.vertices[i].z));

        }
        mMesh.vertices = mCurrentPositions.ToArray();
    }

    private void CalulateVertRanges()
    {
        for (int i = 0; i< mMesh.vertexCount; i++)
        {
            if (OffsetX > mMesh.vertices[i].y)
                OffsetX = mMesh.vertices[i].y;

            if (OffsetY > mMesh.vertices[i].z)
                OffsetY = mMesh.vertices[i].z;

            if(MaxX < mMesh.vertices[i].y)
                MaxX = mMesh.vertices[i].y;

            if (MaxY < mMesh.vertices[i].z)
                MaxY = mMesh.vertices[i].z;
        }
    }

    private Vector2 GetVertRand(Vector3 vertPos)
    {
        return new Vector2((vertPos.y - OffsetX) / MaxX - OffsetX, (vertPos.z - OffsetY) / MaxY - OffsetY);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            DrawPlanes(++SmoothCount);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DrawPlanes(--SmoothCount);

        }

        if (frameskip-- > 0)
            return;

        for (int i = 0; i < mCurrentPositions.Capacity; i++)
        {
            mDeltaOffets[i] += ((Mathf.PI*2.0f) / fps / targetLength);

            float x = mDeltaPosition[i] + Mathf.Sin(mDeltaOffets[i]) * 0.01f;
            float y = mCurrentPositions[i].y;
            float z = mCurrentPositions[i].z;

            mCurrentPositions[i] = new Vector3(x, y, z);
        }
        mMesh.vertices = mCurrentPositions.ToArray();

        currentTime += ((Mathf.PI * 2.0f) / fps / targetLength);
        if (GenerateFramesOutput && currentTime < Mathf.PI * 2.0f)
        {
           string imagePath = "Screenshots" + string.Format("{0:0000}", screenshotCount++) + ".png";
           StartCoroutine(captureScreenshot(imagePath));
        }
    }

    IEnumerator captureScreenshot(string imagePath)
    {
        yield return new WaitForEndOfFrame();
        Texture2D screenImage = new Texture2D(Screen.width, Screen.height, GenerateWithTransparency?TextureFormat.ARGB32:TextureFormat.RGB24, false);

        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenImage.Apply();

        byte[] imageBytes = screenImage.EncodeToPNG();

        System.IO.File.WriteAllBytes(imagePath, imageBytes);
		Debug.Log("Exported " + imagePath);
    }
}

using UnityEngine;
using System.Collections;
using UnityEditor;

public class TreeToBillboard : MonoBehaviour
{
	/*
	Make a billboard out of an object in the scene
	The camera will auto-place to get the best view of the object so no need for camera adjustment

	To use - place an object in an empty scene with just camera and any lighting you want.
	Add this script to your scene camera and link to the object you want to render.
	Press play and you will get a snapshot of the object (looking down the +Z-axis at it) saved out to billboard.png in your project folder
	Any pixels colored the same as the camera background color will be transparent
	*/

	public GameObject objectToRender;
	public int imageWidth = 1024;
	public int imageHeight = 1024;
	public Material m_BlurMat;

	private Camera _captureCam;

	[ContextMenu("Capture")]
	void ConvertToImage()
	{
		if (objectToRender == null)
		{
			return;
		}

		var camObj = GameObject.Find("billboardCam");

		if (camObj)
		{
			_captureCam = camObj.GetComponent<Camera>();
		}

		if (_captureCam == null)
		{
			GameObject go = new GameObject("billboardCam");//create the cameraObject
			//go.hideFlags = HideFlags.HideAndDontSave;
			_captureCam = go.AddComponent<Camera>();
		}

		_captureCam.CopyFrom(Camera.main);

		RenderTexture[] rts = new RenderTexture[2]
		{
			new RenderTexture((int)imageWidth, (int)imageHeight, 0),
			new RenderTexture((int)imageWidth, (int)imageHeight, 0)
		};

		RenderBuffer[] rbs = new RenderBuffer[2];
		rbs[0] = rts[0].colorBuffer;
		rbs[1] = rts[1].colorBuffer;

		//RenderTexture rt = new RenderTexture((int)imageWidth, (int)imageHeight, 0);
		//_captureCam.targetTexture = rt;

		_captureCam.SetTargetBuffers(rbs, rts[0].depthBuffer);
		_captureCam.orthographic = true;
		_captureCam.clearFlags = CameraClearFlags.Nothing;
		//_captureCam.backgroundColor = new Color(0, 0, 0, 0);
		_captureCam.enabled = true;

		//grab size of object to render - place/size camera to fit
		Bounds bb = objectToRender.GetComponent<Renderer>().bounds;
		float maxSize = Mathf.Max(bb.max.z - bb.min.z, bb.max.x - bb.min.x);

		//place camera looking at centre of object - and backwards down the z-axis from it
		_captureCam.transform.position = bb.center;
		_captureCam.transform.position = new Vector3(_captureCam.transform.position.x + maxSize / 2, _captureCam.transform.position.y, _captureCam.transform.position.z);
		_captureCam.nearClipPlane = 0.0f;
		_captureCam.farClipPlane = maxSize;
		_captureCam.orthographicSize = Mathf.Max(Mathf.Max((bb.max.y - bb.min.y) / 2.0f, (bb.max.x - bb.min.x) / 2.0f), Mathf.Max((bb.max.z - bb.min.z) / 2.0f));
		//_captureCam.transform.LookAt(bb.center);
		_captureCam.transform.LookAt(new Vector3(objectToRender.transform.position.x, bb.center.y, objectToRender.transform.position.z));

		var baker = GetComponent<BillboardBaker>();
		baker.width = (bb.max.x - bb.min.x);

		float ratio = (bb.max.y - bb.min.y) / (bb.max.x - bb.min.x);

		if (ratio > 1)
		{
			baker.sizeU = 0.3333f / ratio;
			baker.sizeV = 0.3333f;
			baker.offsetU = (baker.sizeV - baker.sizeU) / 2;
			baker.offsetV = 0;
		}
		else
		{
			baker.sizeU = 0.3333f;
			baker.sizeV = 0.3333f * ratio;
			baker.offsetU = 0;
			baker.offsetV = (baker.sizeU - baker.sizeV) / 2;
		}

		RenderTexture.active = rts[0];
		GL.Clear(false, true, new Color(0.2745098f, 0.3019608f, 0.227451f, 0));

		RenderTexture.active = rts[1];
		GL.Clear(false,true, new Color(0.5f, 0.5f, 1, 0));

		// Render to Atlas textures
		renderToTextures();

		RenderTexture desRenderTexture;
		Blur(rts[0], out desRenderTexture);

		var tex = new Texture2D(imageWidth, imageHeight, TextureFormat.ARGB32, false);

		string path;
		path = AssetDatabase.GetAssetPath(m_BlurMat);
		path = path.Remove(path.Length - (m_BlurMat.name.Length + 4));

		// Read pixels
		RenderTexture.active = desRenderTexture;
		tex.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
		tex.Apply();

		// Encode texture into PNG
		byte[] bytes = tex.EncodeToPNG();
		System.IO.File.WriteAllBytes(path + "resource/Billboard_Albedo.png", bytes);

		// Read pixels
		Blur(rts[1], out desRenderTexture);
		RenderTexture.active = desRenderTexture;
		tex.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
		tex.Apply();

		// Encode texture into PNG
		bytes = tex.EncodeToPNG();
		System.IO.File.WriteAllBytes(path + "resource/Billboard_Normal.png", bytes);

		RenderTexture.active = null;

		_captureCam.enabled = false;

		//SafeDestroy(rt);
		SafeDestroy(tex);
		SafeDestoryArray(rts);

	}

	void SafeDestroy(Object obj)
	{
		if (Application.isEditor)
		{
			DestroyImmediate(obj);
		}
		else
		{
			Destroy(obj);
		}
	}

	void SafeDestoryArray(Object[] objs)
	{
		for (int i = 0; i < objs.Length; i++)
		{
			SafeDestroy(objs[i]);
		}
	}

	private void renderToTextures()
	{
		// bottom
		_captureCam.rect = new Rect(0.3333f, 0, 0.3333f, 0.3333f);
		_captureCam.Render();

		_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		_captureCam.rect = new Rect(0.6667f, 0, 0.3333f, 0.3333f);
		_captureCam.Render();

		// middle
		_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		_captureCam.rect = new Rect(0, 0.3333f, 0.3333f, 0.3333f);
		_captureCam.Render();

		_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		_captureCam.rect = new Rect(0.3333f, 0.3333f, 0.3333f, 0.3333f);
		_captureCam.Render();

		_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		_captureCam.rect = new Rect(0.6667f, 0.3333f, 0.3333f, 0.3333f);
		_captureCam.Render();

		//// top
		_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		_captureCam.rect = new Rect(0, 0.6667f, 0.3333f, 0.3333f);
		_captureCam.Render();

		_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		_captureCam.rect = new Rect(0.3333f, 0.6667f, 0.3333f, 0.3333f);
		_captureCam.Render();

		_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		_captureCam.rect = new Rect(0.6667f, 0.6667f, 0.3333f, 0.3333f);
		_captureCam.Render();

		//_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.up, -45);
		//_captureCam.transform.RotateAround(objectToRender.transform.position, Vector3.forward, 90);
		//_captureCam.transform.position = new Vector3(objectToRender.transform.position.x, _captureCam.transform.position.y, objectToRender.transform.position.z);
		//_captureCam.rect = new Rect(0, 0, 0.3333f, 0.3333f);
		//_captureCam.Render();
	}

	private void Blur(RenderTexture source, out RenderTexture destination)
	{
		destination = new RenderTexture((int)imageWidth, (int)imageHeight, 0);

		var temp = RenderTexture.GetTemporary(source.width, source.height);
		Graphics.Blit(source, temp, m_BlurMat, 0);
		Graphics.Blit(temp, destination, m_BlurMat, 1);
		RenderTexture.ReleaseTemporary(temp);
	}
}
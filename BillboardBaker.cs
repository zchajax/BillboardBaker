using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BillboardBaker : MonoBehaviour
{
#if UNITY_EDITOR
	public BillboardAsset m_outputFile;
	public Material m_material;

	private float ratio = 1.0f;
	public float sizeU = 0.3333f;
	public float sizeV = 0.3333f;
	public float offsetU = 0;
	public float offsetV = 0;

	public float width = 1;

	[ContextMenu("Bake Billboard")]
	void BakeBillboard()
	{
		BillboardAsset billboard = new BillboardAsset();

		billboard.material = m_material;
		Vector4[] texCoords = new Vector4[8];
		ushort[] indices = new ushort[6];
		Vector2[] vertices = new Vector2[4];

		//float height = 0.3333f * ratio;
		//float offsetY = 0.3333f * (1 - ratio) / 2;

		//height = 0.3333f;
		//offset = 0;

		texCoords[0].Set(0.3333f + offsetU, 0 + offsetV, sizeU, sizeV);
		texCoords[1].Set(0.6667f + offsetU, 0 + offsetV, sizeU, sizeV);
		texCoords[2].Set(0.0f + offsetU, 0.3333f + offsetV, sizeU, sizeV);
		texCoords[3].Set(0.3333f + offsetU, 0.3333f + offsetV, sizeU, sizeV);
		texCoords[4].Set(0.6667f + offsetU, 0.3333f + offsetV, sizeU, sizeV);
		texCoords[5].Set(0.0f + offsetU, 0.6667f + offsetV, sizeU, sizeV);
		texCoords[6].Set(0.3333f + offsetU, 0.6667f + offsetV, sizeU, sizeV);
		texCoords[7].Set(0.6667f + offsetU, 0.6667f + offsetV, sizeU, sizeV);

		ratio = sizeV / sizeU;

		//indices[0] = 0;
		//indices[1] = 3;
		//indices[2] = 1;
		//indices[3] = 3;
		//indices[4] = 4;
		//indices[5] = 1;
		//indices[6] = 1;
		//indices[7] = 4;
		//indices[8] = 5;
		//indices[9] = 1;
		//indices[10] = 5;
		//indices[11] = 2;

		indices[0] = 0;
		indices[1] = 3;
		indices[2] = 1;
		indices[3] = 2;
		indices[4] = 3;
		indices[5] = 0;

		//vertices[0].Set(0.47093f, 1);
		//vertices[1].Set(0.037790697f, 0.498547f);
		//vertices[2].Set(0.037790697f, 0.020348798f);
		//vertices[3].Set(0.58906996f, 1);
		//vertices[4].Set(0.95930207f, 0.498547f);
		//vertices[5].Set(0.95930207f, 0.020348798f);

		vertices[0].Set(0, 1);
		vertices[1].Set(0, 0);
		vertices[2].Set(1, 1);
		vertices[3].Set(1, 0);

		billboard.SetImageTexCoords(texCoords);
		billboard.SetIndices(indices);
		billboard.SetVertices(vertices);

		//billboard.width = 19.03616f;
		//billboard.height = 19.58068f;
		//billboard.bottom = -1.814002f;

		billboard.width = width;
		billboard.height = width * ratio;
		billboard.bottom = 0;

		if (m_outputFile != null)
		{
			EditorUtility.CopySerialized(billboard, m_outputFile);
		}
		else
		{
			string path;
			path = AssetDatabase.GetAssetPath(m_material);
			path = path.Remove(path.Length - (m_material.name.Length + 4));
			AssetDatabase.CreateAsset(billboard, path + "resource/BillboardAsset.asset");
		}
	}
#endif
}
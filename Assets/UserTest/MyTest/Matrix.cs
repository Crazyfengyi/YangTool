using UnityEngine;

public class Matrix : MonoBehaviour
{
    public bool isInMatrix;
    public Texture2D text;

    public Shader shader;

    private Material material;
    private Material Material
    {
        get
        {
            if (material == null)
            {
                material = new Material(shader);
            }
            return material;
        }
    }
    private Camera Camera;
    void Awake()
    {
        Camera = Camera.main;
    }
    void OnEnable()
    {
        Camera.depthTextureMode = DepthTextureMode.DepthNormals;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isInMatrix = !isInMatrix;
        }
    }

    private void OnPreRender()
    {
        if (Material != null)
        {
            Material.SetMatrix("_ViewToWorld", Camera.cameraToWorldMatrix);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Material != null && isInMatrix)
        {
            Material.SetTexture("_FontTex", text);
            Graphics.Blit(source, destination, Material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}

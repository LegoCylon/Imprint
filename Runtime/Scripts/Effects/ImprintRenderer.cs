using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Imprint.Runtime.Effects
{
    public class ImprintRenderer : PostProcessEffectRenderer<ImprintEffect>
    {
        #region Fields
        private static int sRenderTextureID;

        private Material _Material;
        private Mesh _Mesh;
        #endregion

        public override void Init()
        {
            base.Init();

            sRenderTextureID = Shader.PropertyToID(name: "_ImprintTex");

            _Material = new Material(shader: Shader.Find(name: "Imprint/Make"));
            var gameObject = GameObject.CreatePrimitive(type: PrimitiveType.Sphere);
            _Mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(obj: gameObject);
        }

        public override void Release()
        {
            base.Release();

            Object.DestroyImmediate(obj: _Material);
        }

        public override void Render(PostProcessRenderContext context)
        {
            if ((context.camera.depthTextureMode & DepthTextureMode.Depth) != DepthTextureMode.Depth)
            {
                context.camera.depthTextureMode |= DepthTextureMode.Depth;
            }

            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Imprint/Show"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);

            context.command.BeginSample(name: "Imprint");
            context.GetScreenSpaceTemporaryRT(cmd: context.command, nameID: sRenderTextureID);
            context.command.SetRenderTarget(rt: sRenderTextureID);
            context.command.ClearRenderTarget(clearDepth: false, clearColor: true, backgroundColor: Color.black);

            foreach (ImprintBehaviour imprintBehaviour in ImprintBehaviour.Instances)
            {
                switch (imprintBehaviour.Renderer)
                {
                    case null:
                        break;
                    default:
                        Bounds bounds = imprintBehaviour.Renderer.bounds;
                        float width = Mathf.Max(a: bounds.size.x, b: bounds.size.y);
                        width = Mathf.Max(a: width, b: bounds.size.z);
                        Vector3 scale = Vector3.one * width;
                        Matrix4x4 matrix = Matrix4x4.TRS(
                            pos: bounds.center,
                            q: imprintBehaviour.Renderer.transform.rotation,
                            s: scale);
                        context.command.DrawMesh(mesh: _Mesh, matrix: matrix, material: _Material);
                        break;
                }
            }

            #if true
            context.command.SetGlobalTexture(nameID: sRenderTextureID, value: sRenderTextureID);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);
            #else
            context.command.CopyTexture(src: sRenderTextureID, dst: context.destination);
            #endif

            context.command.ReleaseTemporaryRT(nameID: sRenderTextureID);
            context.command.EndSample(name: "Imprint");
        }
    }
}
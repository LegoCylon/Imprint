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
            // Ensure that the camera is setup to manage a depth texture so that it can be used by the Imprint/Make
            // shader.
            if ((context.camera.depthTextureMode & DepthTextureMode.Depth) != DepthTextureMode.Depth)
            {
                context.camera.depthTextureMode |= DepthTextureMode.Depth;
            }

            var sheet = context.propertySheets.Get(shader: Shader.Find(name: "Imprint/Show"));
            sheet.properties.SetFloat(name: "_Blend", value: settings.Blend);

            // Draw Imprint renderers into a temporary render texture (using the Imprint/Make shader) so that the Show
            // shader knows where they all are.
            context.command.BeginSample(name: "Imprint");
            context.GetScreenSpaceTemporaryRT(cmd: context.command, nameID: sRenderTextureID);
            context.command.SetRenderTarget(rt: sRenderTextureID);
            context.command.ClearRenderTarget(clearDepth: false, clearColor: true, backgroundColor: Color.black);

            // Note that for simplicity this doesn't do any culling or instancing.
            foreach (ImprintBehaviour imprintBehaviour in ImprintBehaviour.Instances)
            {
                if (imprintBehaviour.Renderer == null)
                {
                    continue;
                }

                // Create a uniformly scaled shape using the max bounds along any axis to ensure it keeps a consistent
                // size no matter how the character rotates.
                Bounds bounds = imprintBehaviour.Renderer.bounds;
                float width = Mathf.Max(a: bounds.size.x, b: bounds.size.y);
                width = Mathf.Max(a: width, b: bounds.size.z);
                Vector3 scale = Vector3.one * width;
                Matrix4x4 matrix = Matrix4x4.TRS(
                    pos: bounds.center,
                    q: imprintBehaviour.Renderer.transform.rotation,
                    s: scale);
                context.command.DrawMesh(mesh: _Mesh, matrix: matrix, material: _Material);
            }

            // Now that all the imprints have been made into the temporary render texture, perform postprocessing using
            // the Show shader.
            context.command.SetGlobalTexture(nameID: sRenderTextureID, value: sRenderTextureID);
            context.command.BlitFullscreenTriangle(source: context.source,
                destination: context.destination,
                propertySheet: sheet,
                pass: 0);

            context.command.ReleaseTemporaryRT(nameID: sRenderTextureID);
            context.command.EndSample(name: "Imprint");
        }
    }
}
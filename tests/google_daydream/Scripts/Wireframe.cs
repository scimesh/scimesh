using UnityEngine;

namespace Scimesh.Unity
{
    public class Wireframe : MonoBehaviour
    {
        void OnPreRender()
        {
            GL.wireframe = true;
        }
        void OnPostRender()
        {
            GL.wireframe = false;
        }
    }
}

using System;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Data.Item
{
    [Serializable]
    public struct ModelInfo
    {
        [SF] private Mesh renderMesh;
        [SF] private Mesh colliderMesh;
        [SF] private Vector3 scale;
     
        public bool IsValid => renderMesh != null && colliderMesh != null && scale != default;
        public Mesh RenderMesh => renderMesh;
        public Mesh ColliderMesh => colliderMesh;
        public Vector3 Scale => scale;
    }
}
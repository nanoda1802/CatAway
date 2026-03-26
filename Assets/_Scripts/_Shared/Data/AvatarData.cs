using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared.Data
{
    [CreateAssetMenu(menuName = "SO/Avatar", fileName = "AvatarData")]
    public class AvatarData : ScriptableObject
    {
        [SF] private Sprite[] coatColorSprites;
        [SF] private Texture[] coatColorTextures;
        [SF] private Material[] faceMaterials;

        private readonly int _mainTexId = Shader.PropertyToID("_MainTex");
        
        public Sprite GetCoatColorSprite(int idx)
        {
            if (idx < 0 || idx >= coatColorSprites.Length) return coatColorSprites[0];
            return coatColorSprites[idx];
        }

        public Material GetFaceMaterial(int idx)
        {
            if (idx <0 || idx >= faceMaterials.Length) return faceMaterials[0];
            return faceMaterials[idx];
        }

        public void ChangeAvatar(SkinnedMeshRenderer renderer, MaterialPropertyBlock block, int avatarIdx = 0)
        {
            if (avatarIdx < 0 || avatarIdx >= coatColorTextures.Length) return;
            
            renderer.GetPropertyBlock(block, 0);
            
            var curTex = block.GetTexture(_mainTexId);
            var targetTex = coatColorTextures[avatarIdx];
            
            if (curTex == targetTex) return;
            
            block.SetTexture(_mainTexId, targetTex);
            renderer.SetPropertyBlock(block, 0);
        }
    }
}
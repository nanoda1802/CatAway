using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Stage.Item.Ingredient
{
    public class IngredientPrefabHandler : NetworkPrefabInstanceHandlerWithData<IngredientTypePacket>
    {
        private readonly IngredientProvider _provider;

        public IngredientPrefabHandler(IngredientProvider provider)
        {
            _provider = provider;
        }

        public override NetworkObject Instantiate(
            ulong ownerClientId,
            Vector3 position,
            Quaternion rotation,
            IngredientTypePacket instantiationData)
        {
            var type = instantiationData.IngredientType;
            return _provider.GetIngredient(type, position).NetworkObject;
        }

        public override void Destroy(NetworkObject networkObject)
        {
            var ingredient = networkObject.GetComponentInChildren<Ingredient>();

            if (ingredient != null)
            {
                _provider.ReleaseIngredient(ingredient);
            }
            else
            {
                Object.Destroy(networkObject.gameObject);
            }
        }
    }
}
using System.Collections.Generic;
using _Scripts.Stage.Item.Ingredient;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.Table;
using Unity.Netcode;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Item.Plate
{
    public class Plate : NetworkBehaviour, IPrepable, IPlacable
    {
        [SF] private PlateData data;
        [SF] private IngredientType requiredIngredient = IngredientType.Bun;
        
        private readonly List<IngredientType> _platingList = new();
        private IngredientType _platingMask = 0;
        [SF] private MeshFilter platingMeshFilter;

        private Carriable _carriable;
        
        private bool IsFull => _platingList.Count >= data.MaxPlatingCount;
        
        private void Awake() // [임시]
        {
            InitComponents();
        }

        private void OnCollisionEnter(Collision other) // [임시]
        {
            if (!IsServer) return;
            
            if (other.collider.CompareTag("Item")) return;
            if (!other.collider.TryGetComponent(out Carriable carriable)) return;
            if (carriable.Type != CarriableType.Ingredient) return;

            if (TryPlace(carriable))
            {
                carriable.NetworkObject.Despawn();
            }
        }

        public void InitComponents()
        {
            _carriable = this.GetComponentInChildren<Carriable>();
            var rb = this.GetComponentInChildren<Rigidbody>();

            _carriable?.Construct(rb);

            if (platingMeshFilter == null)
            {
                platingMeshFilter = transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>();
            }

            platingMeshFilter.gameObject.SetActive(false);
            platingMeshFilter.transform.localPosition = data.PlatingLocalPos;
            platingMeshFilter.transform.localScale = data.PlatingLocalScale;
        }

        public float Prepare()
        {
            throw new System.NotImplementedException();
        }

        public bool TryPlace(Carriable carriable)
        {
            // Prep 됐는지도 확인해야해
            
            if (carriable.Type != CarriableType.Ingredient) return false;
            if (!carriable.NetworkObject.TryGetComponent(out Ingredient.Ingredient ingredient)) return false;
            if (IsFull || _platingMask.HasFlag(ingredient.Type))  return false;
            if (!HasRequiredIngredient(ingredient.Type)) return false;
            
            if (_platingList.Count <= 0)
            {
                platingMeshFilter.gameObject.SetActive(true);
            }

            _platingMask |= ingredient.Type;
            _platingList.Add(ingredient.Type);
            
            UpdatePlatingClientRpc(_platingMask);
            
            return true;
        }

        public bool TryDisplace(CarrierBehaviour carrier)
        {
            return false;
        }

        private bool HasRequiredIngredient(IngredientType type)
        {
            if (_platingMask.HasFlag(requiredIngredient)) return true;
            if (_platingList.Count < data.MaxPlatingCount - 1) return true;
            
            return type == requiredIngredient;
        }

        [ClientRpc]
        private void UpdatePlatingClientRpc(IngredientType key)
        {
            platingMeshFilter.sharedMesh = data.GetMesh(key);
        }

        public void ResetPlate()
        {
            _platingMask = 0;
            _platingList.Clear();
            platingMeshFilter.gameObject.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Gameplay.Player.SpawnPoint
{
    public class SpawnPoint : MonoBehaviour
    {
        [ReadOnly] [SerializeField] private Vector3 position;
        [ReadOnly] [SerializeField] private int personalId;
        [Space]
        [SerializeField] private SpawnPointMaterialsBank materialsBank;
        [Space]
        [SerializeField] private SpawnPointType spawnPointType;
        [SerializeField] private TextMeshPro textMeshPro;
        [SerializeField] private MeshRenderer meshRenderer;
        [Space] 
        [SerializeField] private List<SpawnPointConnection> connections;
        
        public int PersonalId => personalId;
        public SpawnPointType SpawnPointType => spawnPointType;
        public TextMeshPro Text => textMeshPro;
        public Vector3 Position => position;

        private void OnValidate()
        {
            UpdatePosition();
            UpdateEditorText();
            UpdateMaterial();
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void SetId(int id)
        {
            personalId = id;
        }
        
        public void UpdateEditorText()
        {
            if (textMeshPro == null)
            {
                return;
            }

            gameObject.name = $"{nameof(SpawnPoint)}_{SpawnPointType}_{personalId}";
            textMeshPro.text = $"<size=1.5f>id: {personalId}</size>\n{SpawnPointType.ToString()}\n<size=1.5f>Spawn point</size>";
        }

        public void UpdatePosition()
        {
            position = transform.position;
        }

        public void UpdateMaterial()
        {
            if (materialsBank == null || meshRenderer == null)
            {
                return;
            }

            var material = materialsBank.GetMaterial(spawnPointType);

            if (material == null)
            {
                return;
            }

            meshRenderer.material = material;
        }

        public SpawnPoint TryGetConnection(SpawnPointConnectionType type)
        {
            return connections.Count == 0 ? null : connections.FirstOrDefault(x => x.ConnectionType == type)?.SpawnPoint;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var previousColor = Gizmos.color;
            Gizmos.color = Color.white;

            foreach (var connection in connections)
            {
                if (connection.SpawnPoint == null)
                {
                    continue;
                }
                
                Gizmos.DrawLine(transform.position, connection.SpawnPoint.transform.position);
            }

            Gizmos.color = previousColor;
        }
#endif
    }
}
#if UNITY_EDITOR
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

namespace Utils
{
    public class CopyColliderHelper : MonoBehaviour
    {
        [SerializeField] private string prefabPath = "Assets/Prefabs/Props/Location1/";
        
        [Button("Copy")]
        public void Copy()
        {
            var children = transform.GetComponentsInChildren<Transform>();

            foreach (var child in children)
            {
                if (child == transform || child == null || child.GetComponent<MeshRenderer>() == null)
                {
                    continue;
                }

                var prefabFullPath = prefabPath + child.name + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFullPath);

                if (prefab != null)
                {
                    RemoveAllColliders(child.gameObject);

                    var colliders = prefab.GetComponentsInChildren<Collider>();

                    foreach (var collider in colliders)
                    {
                        var newCollider = AddCollider(child.gameObject, collider.GetType());
                        CopyColliderProperties(collider, newCollider);
                    }
                }
            }
        }

        private void RemoveAllColliders(GameObject obj)
        {
            obj.GetComponents<BoxCollider>().ToList().ForEach(DestroyImmediate);
            obj.GetComponents<MeshCollider>().ToList().ForEach(DestroyImmediate);
            obj.GetComponents<SphereCollider>().ToList().ForEach(DestroyImmediate);
            obj.GetComponents<CapsuleCollider>().ToList().ForEach(DestroyImmediate);
        }

        private Collider AddCollider(GameObject obj, System.Type colliderType)
        {
            return (Collider)obj.AddComponent(colliderType);
        }

        private void CopyColliderProperties(Collider source, Collider destination)
        {
            if (source is BoxCollider boxSource && destination is BoxCollider boxDest)
            {
                boxDest.center = boxSource.center;
                boxDest.size = boxSource.size;
                boxDest.isTrigger = boxSource.isTrigger;
                boxDest.sharedMaterial = boxSource.sharedMaterial;
            }
            else if (source is SphereCollider sphereSource && destination is SphereCollider sphereDest)
            {
                sphereDest.center = sphereSource.center;
                sphereDest.radius = sphereSource.radius;
                sphereDest.isTrigger = sphereSource.isTrigger;
                sphereDest.sharedMaterial = sphereSource.sharedMaterial;
            }
            else if (source is CapsuleCollider capsuleSource && destination is CapsuleCollider capsuleDest)
            {
                capsuleDest.center = capsuleSource.center;
                capsuleDest.radius = capsuleSource.radius;
                capsuleDest.height = capsuleSource.height;
                capsuleDest.direction = capsuleSource.direction;
                capsuleDest.isTrigger = capsuleSource.isTrigger;
                capsuleDest.sharedMaterial = capsuleSource.sharedMaterial;
            }
            else if (source is MeshCollider meshSource && destination is MeshCollider meshDest)
            {
                meshDest.sharedMesh = meshSource.sharedMesh;
                meshDest.convex = meshSource.convex;
                meshDest.isTrigger = meshSource.isTrigger;
                meshDest.sharedMaterial = meshSource.sharedMaterial;
                meshDest.inflateMesh = meshSource.inflateMesh;
                meshDest.cookingOptions = meshSource.cookingOptions;
            }
            // Добавьте обработку других типов коллайдеров по мере необходимости
        }
    }
}
#endif
using UnityEditor;
using UnityEngine;

namespace Gameplay.Player.Zones.Editor
{
    [CustomEditor(typeof(PrisonerHomeZone))]
    public class PrisonerHomeZoneEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var triggerZoneView = (PrisonerHomeZone)target;
            var colliders = triggerZoneView.GetComponents<BoxCollider>();

            foreach (var collider in colliders)
            {
                Handles.color = new Color(0, 1, 0, 0.25f);

                var center = collider.transform.TransformPoint(collider.center);
                var size = Vector3.Scale(collider.size, collider.transform.lossyScale);

                var vertices = new Vector3[4];
                vertices[0] = center + new Vector3(-size.x, 0, -size.z) * 0.5f;
                vertices[1] = center + new Vector3(size.x, 0, -size.z) * 0.5f;
                vertices[2] = center + new Vector3(size.x, 0, size.z) * 0.5f;
                vertices[3] = center + new Vector3(-size.x, 0, size.z) * 0.5f;

                Handles.DrawSolidRectangleWithOutline(vertices, new Color(0, 1, 0, 0.25f), Color.green);

                Handles.DrawLine(vertices[0], vertices[1]);
                Handles.DrawLine(vertices[1], vertices[2]);
                Handles.DrawLine(vertices[2], vertices[3]);
                Handles.DrawLine(vertices[3], vertices[0]);
            }
        }
    }
}
using PlayVibe.RolePopup;
using UnityEngine;

namespace Gameplay.Character
{
    public class CharacterViewGraphicContainer : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
        [SerializeField] private InteractiveOutline interactiveOutline;
        [Space]
        [SerializeField] private Material securityMaterial;
        [SerializeField] private Material prisonerMaterial;

        public void SetRole(RoleType roleType)
        {
            skinnedMeshRenderer.material = roleType == RoleType.Security ? securityMaterial : prisonerMaterial;
        }

        public void SetStateForInteractiveOutline(bool value)
        {
            interactiveOutline.enabled = value;
        }
    }
}
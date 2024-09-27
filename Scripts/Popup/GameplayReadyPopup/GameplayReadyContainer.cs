using TMPro;
using UnityEngine;

namespace PlayVibe
{
    public class GameplayReadyContainer : PoolView
    {
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private TextMeshProUGUI statusText;

        public int ActorNumber { get; private set; }
        
        public void SetActor(int actorNumber)
        {
            ActorNumber = actorNumber;
        }
        
        public void SetNickname(string value)
        {
            nicknameText.text = value;
        }
        
        public void SetStatus(GameplayReadyType value)
        {
            statusText.text = value.ToString();
        }
    }
}
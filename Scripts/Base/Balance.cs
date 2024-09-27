using System;
using System.Collections.Generic;
using DG.Tweening;
using Services.Gameplay.TimeDay;
using UnityEngine;

namespace PlayVibe
{
    [CreateAssetMenu(fileName = "Balance", menuName = "SO/Balance")]
    public class Balance : ScriptableObject
    {
        public MainBalance Main;
        public MovementBalance Movement;
        public InteractiveBalance Interactive;
        public InventoryBalance Inventory;
        public RoleRulesBalance RoleRules;
        public WalletBalance Wallet;
        public MarkersBalance Markers;
        public RequestTimeoutBalance RequestTimeout;
        public ChatSettingsBalance ChatSettings;
        public TimeDayBalance TimeDay;
        public DropBalance Drop;
        public SpellsBalance Spells;
        
        [Serializable]
        public class MainBalance
        {
            public byte MaxPlayersInRoom = 10;
            public int RoundMax = 5;
            public bool ShowNickname = true;
            public float LoadLevelMaxTime = 60f;
            public float VisibilityDistance = 20f;
        }

        [Serializable]
        public class MovementBalance
        {
            public float MoveSpeed = 7.5f;
            public float MaxSpeed = 15f;
            public float StaminaRegenRate = 0.1f;
            public float StaminaDrainRate = 0.1f;
            [Range(0f, 0.99f)] public float StaminaExhausted = 0.99f;
            public float SpeedAccelerationFactor = 2.5f;
            public float SpeedSlowingDownFactor = 2.5f;
            public float MaxStamina = 3f;
            public float RotationSpeed = 10f;
            [Space]
            public float NetworkLerpMoveSpeed = 10f;
            public float NetworkLerpRotationSpeed = 10f;
            [Space]
            public float FallSlowdownSpeed = 4f;
        }

        [Serializable]
        public class InteractiveBalance
        {
            public int TryArrestPrice = 25;
            public float ArrestDistance = 3f;
            public float ArrestDuration = 10;
            public LayerMask InteractiveLayer;
            public float IndicatorScreenOffsetFactor = 0.9f;
            public float MinTradeDistance = 1.5f;
            public float MaxTradeDistance = 2f;
        }
        
        [Serializable]
        public class InventoryBalance
        {
            [Range(1, 10)] public int SecurityEquipmentCapacity = 5;
            [Range(1, 10)] public int PrisonerEquipmentCapacity = 5;
            [Space]
            [Range(1, 8)] public int SecurityLootBoxCapacity = 4;
            [Range(1, 8)] public int PrisonerLootBoxCapacity = 4;
            [Space]
            [Range(1, 100)] public int SecuritySeizedInventoryCapacity = 10;
            [Space]
            [Range(1, 18)] public int RecyclerCapacity = 18;
            public int RecyclerRunDuration = 20;
            public float SelfCraftDuration = 5f;
        }
        
        [Serializable]
        public class WalletBalance
        {
            public int SecurityInitialAmountCurrency = 100;
            public int PrisonerInitialAmountCurrency = 100;
            [Space]
            public int SecurityUpgradeLootBoxPrice = 250;
            public int PrisonerUpgradeLootBoxPrice = 250;
        }

        [Serializable]
        public class RoleRulesBalance
        {
            [Tooltip("Time to select a role in seconds")]
            public float PrepareTime = 60f;
            public List<RoleRulesBalanceData> Data;

            [Serializable]
            public class RoleRulesBalanceData
            {
                public int NumberPlayers;
                public int SecurityLimit = 1;
            }
        }
        
        [Serializable]
        public class MarkersBalance
        {
            public float DetectionRadius = 10f;
            public float WantedMarkLifeTime = 120;
        }
        
        [Serializable]
        public class RequestTimeoutBalance
        {
            public float Trade = 10f;
        }
        
        [Serializable]
        public class ChatSettingsBalance
        {
            public int MessagesLimit = 50;
            public int MessageCharLimit = 100;
            public float HideDelay = 5f;
            public float ShowDuration = 0.5f;
            public float HideDuration = 0.5f;
            public Ease ShowEase = Ease.InOutSine;
            public Ease HideEase = Ease.InOutSine;
            [Range(0f, 1f)] public float MinAlpha = 0;
            [Range(0f, 1f)] public float MaxAlpha = 1;
        }

        [Serializable]
        public class TimeDayBalance
        {
            public int LengthDay = 180;
            public int LengthNight = 120;
            public Ease SwitchEase = Ease.Linear;
            public float SwitchDuration = 1f;
            
            public double GetTime(TimeDayState timeDayState)
            {
                return timeDayState == TimeDayState.Day ? LengthDay : LengthNight;
            }
        }

        [Serializable]
        public class DropBalance
        {
            public List<float> FailedChange = new();
        }
        
        [Serializable]
        public class SpellsBalance
        {
            [Range(1, 4)] public int SecuritySpellsLimit = 4;
            [Range(1, 4)] public int PrisonerSpellsLimit = 4;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virtuery.PlayFab
{
    [Serializable]
    public class PlayerProfile
    {
        public string userName;
        public string userGender;
        public string selectedCharacter;
        public string personalityTraits;
        public string subscriptionTier;
        public int credits;
        public int messageLimit;
        public DateTime lastLogin;
        public PlayerSettings settings;
        public DateTime createdAt;
        public DateTime updatedAt;

        public PlayerProfile()
        {
            userName = "";
            userGender = "";
            selectedCharacter = "";
            personalityTraits = "";
            subscriptionTier = "Free";
            credits = 0;
            messageLimit = 50;
            lastLogin = DateTime.UtcNow;
            settings = new PlayerSettings();
            createdAt = DateTime.UtcNow;
            updatedAt = DateTime.UtcNow;
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                { PlayerDataKeys.USER_NAME, userName },
                { PlayerDataKeys.USER_GENDER, userGender },
                { PlayerDataKeys.SELECTED_CHARACTER, selectedCharacter },
                { PlayerDataKeys.PERSONALITY_TRAITS, personalityTraits },
                { PlayerDataKeys.SUBSCRIPTION_TIER, subscriptionTier },
                { PlayerDataKeys.CREDITS, credits.ToString() },
                { PlayerDataKeys.MESSAGE_LIMIT, messageLimit.ToString() },
                { PlayerDataKeys.LAST_LOGIN, lastLogin.ToString("o") },
                { PlayerDataKeys.SETTINGS_JSON, JsonUtility.ToJson(settings) },
                { PlayerDataKeys.CREATED_AT, createdAt.ToString("o") },
                { PlayerDataKeys.UPDATED_AT, updatedAt.ToString("o") }
            };
        }

        public static PlayerProfile FromDictionary(Dictionary<string, string> data)
        {
            var profile = new PlayerProfile();

            if (data.TryGetValue(PlayerDataKeys.USER_NAME, out string userName))
                profile.userName = userName;

            if (data.TryGetValue(PlayerDataKeys.USER_GENDER, out string userGender))
                profile.userGender = userGender;

            if (data.TryGetValue(PlayerDataKeys.SELECTED_CHARACTER, out string selectedCharacter))
                profile.selectedCharacter = selectedCharacter;

            if (data.TryGetValue(PlayerDataKeys.PERSONALITY_TRAITS, out string personalityTraits))
                profile.personalityTraits = personalityTraits;

            if (data.TryGetValue(PlayerDataKeys.SUBSCRIPTION_TIER, out string subscriptionTier))
                profile.subscriptionTier = subscriptionTier;

            if (data.TryGetValue(PlayerDataKeys.CREDITS, out string creditsStr) && int.TryParse(creditsStr, out int credits))
                profile.credits = credits;

            if (data.TryGetValue(PlayerDataKeys.MESSAGE_LIMIT, out string messageLimitStr) && int.TryParse(messageLimitStr, out int messageLimit))
                profile.messageLimit = messageLimit;

            if (data.TryGetValue(PlayerDataKeys.LAST_LOGIN, out string lastLoginStr) && DateTime.TryParse(lastLoginStr, out DateTime lastLogin))
                profile.lastLogin = lastLogin;

            if (data.TryGetValue(PlayerDataKeys.SETTINGS_JSON, out string settingsJson))
            {
                try
                {
                    profile.settings = JsonUtility.FromJson<PlayerSettings>(settingsJson) ?? new PlayerSettings();
                }
                catch
                {
                    profile.settings = new PlayerSettings();
                }
            }

            if (data.TryGetValue(PlayerDataKeys.CREATED_AT, out string createdAtStr) && DateTime.TryParse(createdAtStr, out DateTime createdAt))
                profile.createdAt = createdAt;

            if (data.TryGetValue(PlayerDataKeys.UPDATED_AT, out string updatedAtStr) && DateTime.TryParse(updatedAtStr, out DateTime updatedAt))
                profile.updatedAt = updatedAt;

            return profile;
        }
    }

    [Serializable]
    public class PlayerSettings
    {
        public bool avatarInChat;
        public bool backgroundMusic;
        public bool notifications;
        public bool soundEffects;
        public float musicVolume;
        public float sfxVolume;
        public string language;
        public string theme;

        public PlayerSettings()
        {
            avatarInChat = true;
            backgroundMusic = true;
            notifications = true;
            soundEffects = true;
            musicVolume = 1.0f;
            sfxVolume = 1.0f;
            language = "en";
            theme = "default";
        }
    }

    public static class PlayerDataKeys
    {
        public const string USER_NAME = "UserName";
        public const string USER_GENDER = "UserGender";
        public const string SELECTED_CHARACTER = "SelectedCharacter";
        public const string PERSONALITY_TRAITS = "PersonalityTraits";
        public const string SUBSCRIPTION_TIER = "SubscriptionTier";
        public const string AVATAR_IN_CHAT = "AvatarInChat";
        public const string BACKGROUND_MUSIC = "BackgroundMusic";
        public const string NOTIFICATIONS = "Notifications";
        public const string CREDITS = "Credits";
        public const string MESSAGE_LIMIT = "MessageLimit";
        public const string LAST_LOGIN = "LastLogin";
        public const string SETTINGS_JSON = "SettingsJson";
        public const string CREATED_AT = "CreatedAt";
        public const string UPDATED_AT = "UpdatedAt";
        public const string CHAT_HISTORY_KEY = "ChatHistory";
        public const string OFFLINE_QUEUE = "OfflineQueue";
    }

    public static class TitleDataKeys
    {
        public const string CHARACTER_DEFINITIONS = "CharacterDefinitions";
        public const string PERSONALITY_TEMPLATES = "PersonalityTemplates";
        public const string FEED_POSTS = "FeedPosts";
        public const string SUBSCRIPTION_TIERS = "SubscriptionTiers";
        public const string APP_CONFIG = "AppConfig";
        public const string FEATURE_FLAGS = "FeatureFlags";
        public const string MAINTENANCE_MODE = "MaintenanceMode";
        public const string MIN_APP_VERSION = "MinAppVersion";
    }
}

namespace AskBofah.Helpers
{
    public static class AppConstants
    {
        // ==================== OPENROUTER ====================
        // ⚠️ REPLACE WITH YOUR NEW KEY AFTER YOU ROTATE IT
        public const string OpenRouterApiKey = "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";
        public const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1";
        public const string DefaultModel = "openai/gpt-4o-mini";
        public const string AppTitle = "Ask Bofah";
        public const string AppReferer = "https://askbofah.app";
        public const string AppName = "Ask Bofah";

        // ==================== STORAGE KEYS ====================
        public const string SecureKeyToken = "askbofah_token";
        public const string SecureKeyUserId = "askbofah_user_id";
        public const string SecureKeyUserEmail = "askbofah_user_email";
        public const string SecureKeyUserName = "askbofah_user_name";

        public const string PrefKeyIsDarkMode = "askbofah_dark_mode";
        public const string PrefKeyConversationHistory = "askbofah_chat_history";

        // ==================== COLORS (Billion Dollar Theme) ====================
        // Dark background
        public const string Background = "#0B1120";
        public const string Surface = "#111827";
        public const string SurfaceLight = "#1E293B";

        // Brand
        public const string PrimaryTeal = "#00A3A1";
        public const string PrimaryTealDark = "#008B89";
        public const string AccentPurple = "#9333EA";
        public const string AccentIndigo = "#4F46E5";

        // Text
        public const string TextPrimary = "#F8FAFC";
        public const string TextSecondary = "#94A3B8";
        public const string TextMuted = "#64748B";

        // Status
        public const string Success = "#10B981";
        public const string Warning = "#F59E0B";
        public const string Danger = "#EF4444";

        // Borders
        public const string Border = "#1E293B";
        public const string BorderLight = "#334155";
    }
}
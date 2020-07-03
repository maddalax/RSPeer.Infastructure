namespace RSPeer.Domain.Entities
{
    public class BotPreferences
    {
        public bool LocalScriptsOnly { get; set; }
        public bool EnableFileLogging { get; set; }
        public bool ExpandLogger { get; set; }
        public bool CloseOnBan { get; set; }
        public bool ShowIpOnMenuBar { get; set; } = true;
        public bool ShowAccountOnMenuBar { get; set; } = true;
        public bool ShowScriptOnMenuBar { get; set; } = true;
        public bool AllowScriptMessageOnMenuBar { get; set; } = true;
        public WebWalker WebWalker { get; set; } = WebWalker.Acuity;
        public string DaxWebKey { get; set; }
    }
}
namespace MetaUI.NavControlAddInGenerator.Consts
{
    public class SymbolsDownloadConsts
    {
        // This parameter will be passed from BC
        public const string ClientId = "02aab7a9-5e57-44e1-900e-546a34d4940e";
        // This parameter will be passed from BC
        public const string ClientSecret = "-qq_~Ciq31FoQXu475CrZ54-Us3GTfpA1-";
        public const string LoginUrl = @"https://login.microsoftonline.com";
        // This parameter will be passed from BC
        public const string tenantId = "d25c5a7b-54fb-4863-88b9-5ccf8190a323";
        public const string Scopes = @"https://api.businesscentral.dynamics.com/.default";
        // This parameter will be passed from BC
        public const string environment = "test";
        public const string BaseUrl = @"https://api.businesscentral.dynamics.com/v2.0/{0}/dev/packages?publisher={1}&appName={2}&versionText={3}";
        // This parameter will be passed from BC
        public const string publisher = "Microsoft";
        // This parameter will be passed from BC
        public static readonly string[] appNames = { "Application", "Base Application", "System Application", "System" };
        // This parameter will be passed from BC
        public const string versionText = "17.0.0.0";

        // OAuth settings
        public const string grant_type = "client_credentials";
        public const string TokenEndpoint = @"oauth2/v2.0/token";
    }
}

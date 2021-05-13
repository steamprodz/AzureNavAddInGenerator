// The controladdin "type declares the new add-in."
controladdin {{ControlAddInName}}
{
    // The Scripts property can reference both external and local scripts.
    Scripts =
        'scripts/grid/engine.js',
        'scripts/grid/meta-ui-grid.js',
        'scripts/grid/extension.js',
        {{ScriptsPlaceholder}};

    // The StartupScript is a special script that the webclient calls once the page is loaded.
    StartupScript = 'scripts/grid/start.js';

    // Images and StyleSheets can be referenced in a similar fashion.
    StyleSheets =
        'https://use.fontawesome.com/releases/v5.6.3/css/all.css',
        'https://cdn.jsdelivr.net/npm/primeicons@2.0.0/primeicons.css',
        'https://cdn.jsdelivr.net/npm/primeng@8.1.1/resources/primeng.min.css',
        './stylesheets/styles.css',
        {{StylesPlaceholder}};


    // The layout properties define how control add-in are displayed on the page.
    RequestedHeight = 764;
    RequestedWidth = 500;

    MinimumWidth = 100;
    MinimumHeight = 50;

    MaximumWidth = 1920;
    MaximumHeight = 1080;

    VerticalShrink = true;
    HorizontalShrink = true;
    VerticalStretch = true;
    HorizontalStretch = true;


    // The event declarations specify what callbacks could be raised from JavaScript by using the webclient API.
    event OnControlInit();
    event OnControlReady();
    event OnClearFilters();
    event OnFiltersEmit(Filters: JsonArray);
    event OnRowSelected(Row: JsonObject);


    // The procedure declarations specify what JavaScript methods could be called from AL.
    procedure Initialize(SettingsHostUrl: Text; ControlID: Text; UserName: Text; AccessKey: Text; UserSecurityId: Text; CompanyID: Text);
    procedure SetFilters(Filters: text; languageId: Integer);
}

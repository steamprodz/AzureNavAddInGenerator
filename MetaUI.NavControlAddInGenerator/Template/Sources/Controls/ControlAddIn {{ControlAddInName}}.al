controladdin {{ControlAddInName}}
{
    Scripts = 
        'https://unpkg.com/@webcomponents/webcomponentsjs@2.1.3/custom-elements-es5-adapter.js',
        'https://cdn.polyfill.io/v2/polyfill.js?features=Element.prototype.closest',
        'https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.22.2/moment.min.js',
        'https://cdnjs.cloudflare.com/ajax/libs/node-uuid/1.4.8/uuid.js',
        'scripts/engine.js',
        'scripts/extension.js',
        {{ScriptsPlaceholder}};
              
    StartupScript = 'scripts/start.js';
    StyleSheets = 
        {{StylesPlaceholder}},
        'https://use.fontawesome.com/releases/v5.6.3/css/all.css',
        'stylesheets/styles.css',
        'stylesheets/assets/dot.png';


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
    event OnDoubleClick(Row: JsonObject);

    // The procedure declarations specify what JavaScript methods could be called from AL.
    procedure Initialize(SettingsHostUrl: Text; ControlID: Text; UserName: Text; AccessKey: Text; UserSecurityId: Text; CompanyID: Text);
    procedure SetOrder(OrderBy: text);
    procedure SetFilters(Filters: text);
}

pageextension {{PageExtensionId}} "{{PageExtensionName}}" extends "{{PageToExtendName}}"
{
    layout
    {
        addfirst(Content)
        {
            usercontrol({{ControlAddInName}}; {{ControlAddInName}})
            {
                ApplicationArea = All;

                trigger OnControlInit()
                var
                    SystemCompany: Record Company;
                    MetaUIAdminURL: Text;
                    MetaUIControlID: Text;
                begin
                    BaseFilterStorage.GetTable(Rec);
                    ActiveFilterStorage := BaseFilterStorage.Duplicate();

                    MetaUIAdminURL := 'https://test-meta-ui-admin.azurewebsites.net';
                    MetaUIControlID := 'adf28d23-6761-4342-988e-654ab950db81';

                    SystemCompany.Get(CompanyName);
                    CurrPage.{{ControlAddInName}}.Initialize(MetaUIAdminURL, MetaUIControlID, UserId(),
                        MetaUIGridRoutines.GetAuthorizationAccessKey(), UserSecurityId(), SystemCompany.Id);
                end;

                trigger OnControlReady()
                begin
                    CurrPage.{{ControlAddInName}}.SetOrder(MetaUIGridRoutines.ConvertToODataSorting(ActiveFilterStorage));
                    ConvertRecToODataFilters();


                    IsGridControlReady := true;
                end;

                trigger OnClearFilters()
                begin
                    ActiveFilterStorage := BaseFilterStorage.Duplicate();
                    ActiveFilterStorage.SetTable(Rec);
                    ConvertRecToODataFilters();
                end;

                trigger OnFiltersEmit(Filters: JsonArray)
                begin
                    UpdateODataFilters(Filters);
                end;

                trigger OnRowSelected(Row: JsonObject)
                begin
                    SetActiveRecord(Row);
                end;

                trigger OnDoubleClick(Row: JsonObject)
                begin
                    OpenCard(Row);
                end;
            }
        }
    }

    actions
    {
        addfirst(Processing)
        {
            action(ShowCard)
            {
                ApplicationArea = All;
                Caption = 'Open Card';
                Image = Card;
                Visible = true;

                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;
                PromotedOnly = true;
                RunObject = page "Firm Planned Prod. Order";
                                RunPageLink = "No." = FIELD("No.");
                ShortcutKey = 'Alt+X';
            }
        }
        addlast(Processing)
        {
            action(ShowFiltersByGroup)
            {
                ApplicationArea = All;
                Caption = '[Test] Show Filters By Group';
                Image = DataEntry;
                Visible = true;

                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;
                PromotedOnly = true;

                // Temporary: For testing purpose only
                trigger OnAction()
                var
                    MessageText: Text;
                begin
                    MessageText += StrSubstNo('Rec G0: %1\', Rec.GetFilters());
                    Rec.FilterGroup := 2;
                    MessageText += StrSubstNo('Rec G2: %1\', Rec.GetFilters());
                    Rec.FilterGroup := 9;
                    MessageText += StrSubstNo('Rec G9: %1\', Rec.GetFilters());
                    Rec.FilterGroup := 0;

                    MessageText += StrSubstNo('AFS G0: %1\', ActiveFilterStorage.GetFilters());
                    ActiveFilterStorage.FilterGroup := 2;
                    MessageText += StrSubstNo('AFS G2: %1\', ActiveFilterStorage.GetFilters());
                    ActiveFilterStorage.FilterGroup := 9;
                    MessageText += StrSubstNo('AFS G9: %1\', ActiveFilterStorage.GetFilters());
                    ActiveFilterStorage.FilterGroup := 0;

                    Message(MessageText);
                end;
            }
        }
    }

    var
        MetaUIGridRoutines: Codeunit "Meta UI OData Filters Routines";
        BaseFilterStorage: RecordRef;
        ActiveFilterStorage: RecordRef;
        IsGridControlReady: Boolean;

    trigger OnAfterGetRecord()
    var
        EmptyRow: JsonObject;
    begin
        if IsGridControlReady then
            if ActiveFilterStorage.GetFilters() <> GetFilters() then begin
                ActiveFilterStorage.GetTable(Rec);
                MetaUIGridRoutines.ManageRecordSelection(EmptyRow, ActiveFilterStorage);
                ConvertRecToODataFilters();
            end;
    end;

    local procedure ConvertRecToODataFilters()
    var
        ODataFilters: Text;
    begin
        ODataFilters := MetaUIGridRoutines.ConvertToODataFilters(ActiveFilterStorage);
        CurrPage.{{ControlAddInName}}.SetFilters(ODataFilters);
    end;

    local procedure SetActiveRecord(ActiveRow: JsonObject)
    var
        RecReference: RecordRef;
    begin
        RecReference.GetTable(Rec);
        MetaUIGridRoutines.ManageRecordSelection(ActiveRow, RecReference);
        RecReference.SetTable(Rec);

        CurrPage.SetRecord(Rec);
        CurrPage.Update(false);
    end;

    local procedure OpenCard(ActiveRow: JsonObject)
    var
        RecReference: RecordRef;
    begin
        SetActiveRecord(ActiveRow);
        // RecReference.GetTable(Rec);
        // MetaUIGridRoutines.ManageRecordSelection(ActiveRow, RecReference);
        // RecReference.SetTable(Rec);
        Page.RunModal(page::"Firm Planned Prod. Order", Rec);
    end;

    local procedure UpdateODataFilters(Filters: JsonArray)
    begin
        MetaUIGridRoutines.ApplyNewFilter(Filters, ActiveFilterStorage);
        ActiveFilterStorage.SetTable(Rec);

        ConvertRecToODataFilters();
    end;
}
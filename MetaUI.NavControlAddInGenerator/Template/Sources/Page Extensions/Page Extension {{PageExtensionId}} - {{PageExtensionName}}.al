pageextension {{PageExtensionId}} "{{PageExtensionName}}" extends "{{PageToExtendName}}" // TODO figure out how to fill correct id!!!!!!
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

                    MetaUIAdminURL := MetaUIGridRoutines.GetMetaUIAdminURL();
                    MetaUIControlID := MetaUIGridRoutines.GetConfiguration(CurrPage.ObjectId(false));

                    SystemCompany.Get(CompanyName);
                    CurrPage.{{ControlAddInName}}.Initialize(MetaUIAdminURL, MetaUIControlID, UserId(),
                        MetaUIGridRoutines.GetAuthorizationAccessKey(), UserSecurityId(), SystemCompany.Id);
                end;

                trigger OnControlReady()
                begin
                    ConvertFilters();
                    IsGridControlReady := true;
                end;

                trigger OnClearFilters()
                begin
                    ActiveFilterStorage := BaseFilterStorage.Duplicate();
                    ActiveFilterStorage.SetTable(Rec);
                    ConvertFilters()
                end;

                trigger OnFiltersEmit(Filters: JsonArray)
                begin
                    UpdateODataFilters(Filters); //TODO json does not have "template" node
                end;

                trigger OnRowSelected(Row: JsonObject)
                begin
                    SetActiveRecord(Row);
                end;
            }
        }
        modify(Control1)
        // TODO add logic here to figure out the repeater name.
        // Table 2000000192 is not suitable for this. 
        {
            Visible = false;
        }

    }
    trigger OnAfterGetRecord()
    begin
        if IsGridControlReady then
            if ActiveFilterStorage.GetFilters() <> Rec.GetFilters() then begin
                ActiveFilterStorage.GetTable(Rec);
                ConvertFilters()
            end;
    end;

    local procedure ConvertFilters()
    begin
        CurrPage.{{ControlAddInName}}.SetFilters(MetaUIGridRoutines.EncodeToBase64URI(MetaUIGridRoutines.AssembleFiltersIntoString(ActiveFilterStorage)), GlobalLanguage());
    end;

    local procedure UpdateODataFilters(Filters: JsonArray)
    begin
        MetaUIGridRoutines.ApplyNewFilter(Filters, ActiveFilterStorage);
        ActiveFilterStorage.SetTable(Rec);
        ConvertFilters()
    end;

    local procedure SetActiveRecord(ActiveRow: JsonObject)
    var
        RecID: RecordId;
        JsToken: JsonToken;
    begin
        ActiveRow.Get('recID', JsToken);
        Evaluate(RecID, JsToken.AsValue().AsText());
        Rec.Get(RecID);
    end;

    var
        MetaUIGridRoutines: Codeunit "Meta UI Routines";
        BaseFilterStorage: RecordRef;
        ActiveFilterStorage: RecordRef;
        IsGridControlReady: Boolean;
}
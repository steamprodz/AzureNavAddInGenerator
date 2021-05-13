codeunit 70000 "Meta UI OData Filters Routines"
{
    procedure GetAuthorizationAccessKey() WebServiceAccessKey: Text
    begin
        // SEB: Temporarily.. shitty shit solution...

        // Meta UI Dev Environment
        case UserId() of
            'METAUI\AAG': // Artem Agamalov
                WebServiceAccessKey := 'VKoIa0eNlMq3+0agX/TDihnc5RZVcCeSOzl5cYRWKa8=';
            'METAUI\KOP': // Kostyantyn Pavlenko
                WebServiceAccessKey := 'ile7xP5oEnlnTvrVPN4V6lfDlosKSAongii/G5aezz0=';
            'METAUI\VIK': // Victoriia Ostrolutska
                WebServiceAccessKey := '';
            'METAUI\OLS': // Oleksii Safronov
                WebServiceAccessKey := 'EDOgICcH/Zs5j+lmwOSLVgDWK6g3yHM6lNQGHmaIL90=';
            'METAUI\AND': // Andrii Diachenko
                WebServiceAccessKey := 'F21j7OEYeYMklatywB0ppYFxSqAD+O/YxRJImiORtVY=';
            'METAUI\ADI': // Andrii Deineko
                WebServiceAccessKey := '1qnkYDhQiZDpNq8DoaAw4Stm0OD/8eojjTcJ4tPn/ig=';
            'METAUI\DAR': // Daria Tkachuk
                WebServiceAccessKey := 'Q8S+4Do/6d2yakEykEtIRcaJMA5/kkZUqVTntdBvKQQ=';
            'METAUI\EVB': // Evgeny Buriak
                WebServiceAccessKey := '1/rZwKpNA22Tx6eGNtSXvl2pQfnsoDWTHlUWLJ0F9hs=';
            'METAUI\MAK': // Maksym Kravchenko
                WebServiceAccessKey := 'Nrz7lRH01kj6m0XmR0jmnRMfouH+w5HVXLPPe4RXCmk=';
            'METAUI\MAP': // Maria Perekhodko
                WebServiceAccessKey := 'hV89XPQEWqPvX7x8wKHVO8QWY/28DfrCy5+Yx4b7EIU=';
            'METAUI\OCH': // Oksana Cherevko
                WebServiceAccessKey := 'G/+GopWypVcCTXSLFVRL7AI8FIYblzyqW++SQgMM4a8=';
            'METAUI\SEB': // Sergiy Basenko
                WebServiceAccessKey := 'ZaQK0qIyOp6effCuXB2tN6Bskbn4QdHWOenKgqm2zgc=';
            'METAUI\VOB': // Volodymyr Bohutskyi
                WebServiceAccessKey := '7oHnXPz4WeOzpdn58uJDEeIFak3VKV0emh8FOvOv0Mk=';
            'METAUI\NAS': // Nazar Sandyga
                WebServiceAccessKey := 'wEsEfyZHcKTh+0hExVfkjMxN4oWrWWopsWOPIhgY7Z0=';
        end;
    end;


    local procedure ConvertToODataName(SourceName: Text) ODataName: Text
    var
        ODataUtility: Codeunit ODataUtility;
    begin
        ODataName := ODataUtility.ConvertNavFieldNameToOdataName(SourceName);
        ODataName := DelChr(ODataName, '=', '_');
    end;

    procedure ConvertToODataSorting(Source: RecordRef) ODataSorting: Text
    var
        FieldReference: FieldRef;
        KeyReference: KeyRef;
        Index: Integer;
    begin
        // Getting active key and looping through all it's fields
        KeyReference := Source.KeyIndex(Source.CurrentKeyIndex);
        for Index := 1 to KeyReference.FieldCount() do begin
            FieldReference := KeyReference.FieldIndex(Index);

            ODataSorting += ConvertToODataName(FieldReference.Name);

            if Source.Ascending() then
                ODataSorting += ' asc,'
            else
                ODataSorting += ' desc,';
        end;
        ODataSorting := DelChr(ODataSorting, '>', ',');
    end;

    procedure ConvertToODataFilters(var Source: RecordRef) ODataFilters: Text
    var
        FilterParticle: Record "Record Link" temporary;
        FilterParticle2: Record "Record Link" temporary;
        SystemField: Record Field;

        Regex: Codeunit DotNet_Regex;
        Match: Codeunit DotNet_Match;
        Group: Codeunit DotNet_Group;
        Groups: Codeunit DotNet_GroupCollection;
        Matches: Codeunit DotNet_MatchCollection;

        SkipEqualization: Boolean;
        ODataFilter: Text;
        SourceView: Text;
        ODataName: Text;
        FieldNo: Integer;
        Index: Integer;

        MessageText: Text;
    begin
        Source.FilterGroup := 2;
        if Source.HasFilter then
            SourceView += TruncateTableView(Source.GetView(false)) + ',';

        Source.FilterGroup := 0;
        if Source.HasFilter then
            SourceView += TruncateTableView(Source.GetView(false)) + ',';

        Source.FilterGroup := 9;
        SourceView += TruncateTableView(Source.GetView(false));
        Source.FilterGroup := 0;

        Regex.Regex('Field(\d+)=(0|1)\(([^\)]+)\),?');
        Regex.Matches(SourceView, Matches);
        for Index := 0 to Matches.Count() - 1 do begin
            Matches.Item(Index, Match);
            Match.Groups(Groups);

            // Processing filter field name
            Groups.Item(1, Group);
            Evaluate(FieldNo, Group.Value());
            SystemField.Get(Source.Number(), FieldNo);
            ODataName := ConvertToODataName(SystemField.FieldName);

            FilterParticle.DeleteAll();
            FilterParticle2.DeleteAll();

            Groups.Item(3, Group);
            DisassembleFieldFilter(Group.Value(), FilterParticle);
            FilterParticle2.Copy(FilterParticle, true);

            // Testing: Temporary visualization
            MessageText += StrSubstNo('NAV Filter: %1 (%2)\\', ODataName, Group.Value());

            SkipEqualization := false;
            If FilterParticle.FindSet(true) then
                repeat
                    // Testing: Temporary visualization
                    MessageText += StrSubstNo('%1: %2\', FilterParticle.Type, FilterParticle.URL1);

                    case FilterParticle.Type of
                        FilterParticle.Type::Link: // Special Symbols
                            begin
                                SkipEqualization := FilterParticle.URL1 in ['<', '<=', '>', '>=', '<>'];

                                // case FilterParticle.URL1 of
                                //     '<':
                                //         FilterParticle.URL2 := ODataName + ' lt ';
                                //     '>':
                                //         FilterParticle.URL2 := ODataName + ' gt ';
                                //     '<=':
                                //         FilterParticle.URL2 := ODataName + ' le ';
                                //     '>=':
                                //         FilterParticle.URL2 := ODataName + ' ge ';
                                //     '<>':
                                //         FilterParticle.URL2 := ODataName + ' ne ';
                                //     '&':
                                //         FilterParticle.URL2 := ' and ';
                                //     '|':
                                //         FilterParticle.URL2 := ' or ';
                                //     '..':
                                //         begin
                                //             FilterParticle2.Get(FilterParticle."Link ID");
                                //             if FilterParticle2.Next(-1) <> 0 then
                                //                 if FilterParticle2.Type = FilterParticle2.Type::Note then begin
                                //                     FilterParticle2.URL2 :=
                                //                         ODataName + ' ge ' + ConvertToODataValue(SystemField, FilterParticle2.URL1);
                                //                     FilterParticle2.Modify();

                                //                     FilterParticle.URL2 += ' ';
                                //                 end;

                                //             FilterParticle2.Get(FilterParticle."Link ID");
                                //             if FilterParticle2.Next(1) <> 0 then
                                //                 if FilterParticle2.Type = FilterParticle2.Type::Note then begin
                                //                     FilterParticle2.URL2 :=
                                //                         ODataName + ' le ' + ConvertToODataValue(SystemField, FilterParticle2.URL1);
                                //                     FilterParticle2.Modify();

                                //                     if FilterParticle.URL2 <> '' then
                                //                         FilterParticle.URL2 += 'and '
                                //                     else
                                //                         FilterParticle.URL2 := ' ';
                                //                 end;
                                //         end;
                                // end;
                            end;

                    //FilterParticle.Type::Note: // Search Criteria
                    // if FilterParticle.URL2 = '' then begin
                    //     FilterParticle.URL2 := ConvertToODataValue(SystemField, FilterParticle.URL1);
                    //     if not SkipEqualization then
                    //         FilterParticle.URL2 := ODataName + ' eq ' + FilterParticle.URL2;
                    //end;
                    end;

                    FilterParticle.Modify();
                until (FilterParticle.Next() = 0);

            If FilterParticle.FindSet() then begin
                Clear(ODataFilter);
                // repeat
                //     ODataFilter += FilterParticle.URL2;
                // until (FilterParticle.Next() = 0);

                if ODataFilters <> '' then
                    ODataFilters += StrSubstNo(' and (%1)', ODataFilter)
                else
                    ODataFilters += StrSubstNo('(%1)', ODataFilter);
            end;
        end;
    end;

    local procedure TruncateTableView(SourceView: Text): Text
    begin
        if StrPos(SourceView, 'WHERE') = 0 then
            exit('');

        SourceView := CopyStr(SourceView, StrPos(SourceView, 'WHERE') + 6);
        SourceView := CopyStr(SourceView, 1, StrLen(SourceView) - 1);
        exit(SourceView);
    end;

    local procedure DisassembleFieldFilter(FilterValue: Text; var Particle: Record "Record Link")
    var
        SSParticle: Record "Record Link" temporary;
        SearchCriteria: Text;
        SpecialSymbols: Text;
        Index: Integer;
    begin
        if FilterValue = '' then
            exit;

        // Looking for "special symbols" particles only
        for Index := 1 to StrLen(FilterValue) do
            if FilterValue[Index] in ['<', '=', '>', '&', '|', '.'] then begin
                SpecialSymbols += FilterValue[Index];

                case StrLen(SpecialSymbols) of
                    1:
                        case true of
                            SpecialSymbols in ['&', '|']:
                                SpecialSymbols := CreateFilterParticle(Index, true, SpecialSymbols, SSParticle);

                            SpecialSymbols in ['<', '>']:
                                if not (FilterValue[Index + 1] in ['=', '>']) then
                                    SpecialSymbols := CreateFilterParticle(Index, true, SpecialSymbols, SSParticle);
                        end;
                    2:
                        if SpecialSymbols in ['<=', '>=', '<>', '..'] then
                            SpecialSymbols := CreateFilterParticle(Index - 1, true, SpecialSymbols, SSParticle)
                        else
                            Clear(SpecialSymbols);
                end;
            end else
                Clear(SpecialSymbols);

        // Looking for "search criterias" particles
        If SSParticle.FindSet() then begin
            for Index := 1 to (SSParticle.Count() + 1) do begin
                if Index < (SSParticle.Count() + 1) then begin
                    Particle := SSParticle;
                    Particle.Insert();

                    SearchCriteria := CopyStr(FilterValue, 1, StrPos(FilterValue, SSParticle.URL1) - 1);
                    CreateFilterParticle(SSParticle."Link ID" - 1, false, SearchCriteria, Particle);

                    FilterValue := DelStr(FilterValue, 1, StrPos(FilterValue, SSParticle.URL1) - 1);
                    FilterValue := DelStr(FilterValue, 1, StrLen(SSParticle.URL1));

                    SSParticle.Next();
                end else
                    CreateFilterParticle(SSParticle."Link ID" + 1, false, FilterValue, Particle);
            end;
        end else
            CreateFilterParticle(1, false, FilterValue, Particle);
    end;

    local procedure CreateFilterParticle(ID: Integer; SpecialSymbols: Boolean; Value: Text; var Particle: Record "Record Link"): Text
    begin
        if Value <> '' then begin
            Particle."Link ID" := ID;
            if SpecialSymbols then
                Particle.Type := Particle.Type::Link
            else
                Particle.Type := Particle.Type::Note;
            Particle.URL1 := Value;
            //Particle.URL2 := '';
            Particle.Insert();
        end;
    end;

    local procedure ConvertToODataValue(SystemField: Record Field; Value: Text): Text
    var
        String: Codeunit DotNet_String;
        VarDate: Date;
        VarTime: Time;
        VarBoolean: Boolean;
        VarDecimal: Decimal;
        VarDateTime: DateTime;
        VarDuration: Duration;
    begin
        String.Set(Value);
        Value := String.Replace('+', '%2B');

        case SystemField.Type of
            SystemField.Type::Code,
            SystemField.Type::Text,
            SystemField.Type::Option,
            SystemField.Type::Dateformula,
            SystemField.Type::RecordID:
                if StrPos(Value, '''') = 0 then
                    exit('''' + Value + '''')
                else
                    exit(Value);

            SystemField.Type::Boolean:
                begin
                    Evaluate(VarBoolean, Value);
                    exit(Format(VarBoolean, 0, 9));
                end;

            SystemField.Type::Decimal:
                begin
                    Evaluate(VarDecimal, Value);
                    exit(Format(VarDecimal, 0, 9));
                end;

            SystemField.Type::Time:
                begin
                    Evaluate(VarTime, Value);
                    exit('''' + Format(VarTime, 0, 9) + '''');
                end;

            SystemField.Type::Date:
                begin
                    Evaluate(VarDate, Value, 9);
                    exit(Format(VarDate, 0, 9));
                end;

            SystemField.Type::Datetime:
                begin
                    Evaluate(VarDateTime, Value, 9);
                    exit(Format(VarDateTime, 0, 9));
                end;

            SystemField.Type::Duration:
                begin
                    Evaluate(VarDuration, Value, 9);
                    exit('''' + Format(VarDuration, 0, 9) + '''');
                end;

            SystemField.Type::GUID:
                Exit(DelChr(Value, '=', '{}'));
            else
                exit(Value);
        end;
    end;

    procedure ApplyNewFilter(Filters: JsonArray; var Source: RecordRef)
    var
        SystemField: Record Field;
        FieldReference: FieldRef;
        FilterField: JsonToken;
        FilterValue: JsonToken;
        CurrDateValue: Date;
    begin
        if Filters.Count > 0 then
            if Filters.Get(0, FilterField) then begin
                // Searching for filter value
                FilterField.AsObject().Get('template', FilterValue);

                // Searching for filter field name
                FilterField.AsObject().Get('data', FilterField);
                FilterField.AsObject().Get('customSettings', FilterField);
                FilterField.AsObject().Get('navName', FilterField);

                // Looking for table field number by name
                SystemField.SetRange(TableNo, Source.Number());
                SystemField.SetRange(FieldName, FilterField.AsValue().AsText());
                SystemField.FindFirst();

                // Applying new filter for a selected field
                // Source.FilterGroup := 9;
                FieldReference := Source.FIELD(SystemField."No.");
                if not FilterValue.AsValue().IsNull() then
                    FieldReference.SetFilter(ConvertJsonFilterValueToBCValue(SystemField, FilterValue, FieldReference))
                else
                    FieldReference.SetRange();
                // Source.FilterGroup := 0;
            end;
    end;

    procedure ManageRecordSelection(ActiveRow: JsonObject; var Source: RecordRef)
    var
        SystemField: Record Field;
        FieldReference: FieldRef;
        KeyReference: KeyRef;
        FieldValue: JsonToken;
        Index: Integer;
    begin
        // Getting PK and looping through all it's fields
        Source.FilterGroup := 9;
        KeyReference := Source.KeyIndex(1);
        for Index := 1 to KeyReference.FieldCount() do begin
            FieldReference := KeyReference.FieldIndex(Index);

            // Searching for field value and apply it as a filter
            //if ActiveRow.Get(ConvertToODataName(FieldReference.Name), FieldValue) then
            //    FieldReference.SetFilter(FieldValue.AsValue().AsText())
            //else
            //    FieldReference.SetRange();
            // Looking for table field number by name
            SystemField.SetRange(TableNo, Source.Number());
            SystemField.SetRange(FieldName, FieldReference.Name);
            SystemField.FindFirst();

            if ActiveRow.Get(ConvertToODataName(FieldReference.Name), FieldValue) then
                FieldReference.SetFilter(ConvertJsonFilterValueToBCValue(SystemField, FieldValue, FieldReference))
            else
                FieldReference.SetRange();
        end;
        Source.FilterGroup := 0;
    end;

    procedure GetJsonToken(JsonObject: JsonObject; TokenKey: text) JsonToken: JsonToken
    begin
        if not JsonObject.Get(TokenKey, JsonToken) then
            Error('Could not find a token with key %1', TokenKey);
    end;

    procedure TextToDateTime(TextDatetIme: Text): DateTime
    var
        Day: Integer;
        Month: Integer;
        Year: Integer;
        Time: Time;
    begin
        Evaluate(Day, CopyStr(TextDatetIme, 9, 2));
        Evaluate(Month, CopyStr(TextDatetIme, 6, 2));
        Evaluate(Year, CopyStr(TextDatetIme, 1, 4));
        Evaluate(Time, CopyStr(TextDatetIme, 12, 2) + CopyStr(TextDatetIme, 15, 2));
        exit(CreateDateTime(DMY2Date(Day, Month, Year), Time));
    end;

    local procedure ConvertJsonFilterValueToBCValue(SystemField: Record Field; FilterValue: JsonToken; FieldReference: FieldRef): Text
    var
        OptionValues: List of [Text];

        VarDate: Date;
        VarTime: Time;
        VarBoolean: Boolean;
        VarDecimal: Decimal;
        VarDateTime: DateTime;
        VarDuration: Duration;
        VarOption: Option;
        VarInt: Integer;
        OptionStringText: Text;
    begin
        case SystemField.Type of
            SystemField.Type::Date:
                begin
                    Evaluate(VarDate, format(FilterValue.AsValue().AsDate()));
                    exit(Format(VarDate));
                end;

            SystemField.Type::Datetime:
                begin
                    Evaluate(VarDateTime, format(FilterValue.AsValue().AsDateTime()));
                    exit(Format(VarDateTime));
                end;

            SystemField.Type::Option:
                begin
                    OptionStringText := SystemField.OptionString;
                    OptionValues := OptionStringText.Split(',');
                    if format(FilterValue.AsValue().AsText()) = format(FieldReference.Value) then
                        VarInt := OptionValues.IndexOf(format(FieldReference.Value))
                    else
                        VarInt := OptionValues.IndexOf(format(FilterValue.AsValue().AsText()));
                    if VarInt = 0 then
                        exit(format(VarInt));
                    exit(format(VarInt - 1));
                end;
            else
                exit(Format(FilterValue.AsValue().AsText()));
        end;
    end;
}
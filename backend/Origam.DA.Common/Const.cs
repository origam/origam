#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

namespace Origam.DA;

/// <summary>
/// Summary description for Const.
/// </summary>
public class Const
{
    public static string DefaultLookupIdAttribute = "DefaultLookupId";
    public static string EntityAuditingAttribute = "IsAuditingEnabled";
    public static string IsDatabaseField = "IsDatabaseField";
    public static string AuditingSecondReferenceKeyColumnAttribute =
        "AuditingSecondReferenceKeyColumn";
    public static string TemporaryColumnAttribute = "TemporaryColumn";
    public static string TemporaryColumnInitializedAttribute = "TemporaryColumnInitialized";
    public static string IsAddittionalFieldAttribute = "IsAddittionalField";

    //		public static string TemporaryColumnLookupAttribute = "TemporaryColumnLookupId";
    public static string DataRefreshedAttribute = "DataRefrehsed";

    public static string ValuelistIdField = "Id";
    public static string ValuelistTextField = "Text";
    public static string OriginalLookupIdAttribute = "OriginalLookupId";
    public static string OriginalFieldId = "OriginalFieldId";

    public static string ArrayRelation = "ArrayRelation";
    public static string ArrayRelationField = "ArrayRelationField";
    public static string OrigamDataType = "OrigamDataType";
    public static string FieldId = "FieldId";
    public static string DescribingField = "DescribingField";
    public static string IsWriteOnlyAttribute = "IsWriteOnly";
    public static string HasAggregation = "HasAggregation";

    public Const() { }
}

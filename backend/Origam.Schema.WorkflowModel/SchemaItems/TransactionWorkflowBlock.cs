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

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;

namespace Origam.Schema.WorkflowModel;

public enum TransactionTypes
{
    None = 0,
    Atomic = 1,
}

/// <summary>
/// Summary description for TransactionWorkflowBlock.
/// </summary>
[SchemaItemDescription(
    name: "(Block) Transaction",
    folderName: "Tasks",
    iconName: "block-transaction.png"
)]
[HelpTopic(topic: "Transaction+Block")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class TransactionWorkflowBlock : AbstractWorkflowBlock, IWorkflowTransaction
{
    public TransactionWorkflowBlock()
        : base() { }

    public TransactionWorkflowBlock(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public TransactionWorkflowBlock(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    [Category(category: "Transaction"), DefaultValue(value: TransactionTypes.None)]
    [XmlAttribute(attributeName: "transactionType")]
    public TransactionTypes TransactionType { get; set; } = TransactionTypes.None;
    #endregion
}

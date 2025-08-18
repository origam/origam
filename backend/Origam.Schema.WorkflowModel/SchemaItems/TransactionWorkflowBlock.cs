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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;
public enum TransactionTypes
{
	None = 0,
	Atomic = 1
}
/// <summary>
/// Summary description for TransactionWorkflowBlock.
/// </summary>
[SchemaItemDescription("(Block) Transaction", "Tasks", "block-transaction.png")]
[HelpTopic("Transaction+Block")]
[ClassMetaVersion("6.0.0")]
public class TransactionWorkflowBlock : AbstractWorkflowBlock, IWorkflowTransaction
{
	public TransactionWorkflowBlock() : base() {}
	public TransactionWorkflowBlock(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public TransactionWorkflowBlock(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	[Category("Transaction"), DefaultValue(TransactionTypes.None)]
	[XmlAttribute ("transactionType")]
	public TransactionTypes TransactionType { get; set; } 
		= TransactionTypes.None;
	#endregion
}

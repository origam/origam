#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Collections;
using System.Xml;
using System.Data;

using Origam.Rule;

namespace Origam.Workflow.Star21Service
{
	/// <summary>
	/// Summary description for TransformationAgent.
	/// </summary>
	public class WarehouseServiceAgent : AbstractServiceAgent
	{
		public WarehouseServiceAgent()
		{

		}

		#region Private Methods
		private XmlDocument RecalculateWeightedAverage(WeightedAverageRecalculation data)
		{
			data.InventoryOperationDetail.Columns.Add("OperationDate", typeof(System.DateTime), "min(Child.Date)");

			foreach(WeightedAverageRecalculation.InventoryRow inventory in data.Inventory.Rows)
			{
				DataView operations = new DataView(data.InventoryOperationDetail, 
					"refInventoryId='" + inventory.Id.ToString() + "'",
					"OperationDate, RecordCreated",
					DataViewRowState.CurrentRows);

				if(inventory.IsWeightedAverageLastPriceNull()) inventory.WeightedAverageLastPrice = 0;
				if(inventory.IsWeightedAverageLastQuantityNull()) inventory.WeightedAverageLastQuantity = 0;

//				inventory.WeightedAverageLastPrice = 0;
//				inventory.WeightedAverageLastQuantity = 0;

				foreach(DataRowView operationRowView in operations)
				{
					WeightedAverageRecalculation.InventoryOperationDetailRow operation = (operationRowView.Row as WeightedAverageRecalculation.InventoryOperationDetailRow);

					if(operation.InventoryOperationRow.refInventoryOperationTypeId == new Guid((RuleEngine as RuleEngine).GetConstant("InventoryOperationType_WarehouseReceipt"))
						| operation.InventoryOperationRow.refInventoryOperationTypeId == new Guid((RuleEngine as RuleEngine).GetConstant("InventoryOperationType_WarehouseStockTakingExtra"))
						)
					{
						if(inventory.WeightedAverageLastQuantity + operation.Quantity == 0)
						{
							inventory.WeightedAverageLastPrice = 0;
							inventory.WeightedAverageLastQuantity = 0;
						}
						else
						{
							// Receipt - we have to recalculate the average
							decimal average = ((inventory.WeightedAverageLastPrice * (decimal)inventory.WeightedAverageLastQuantity) + operation.PriceOtherCosts + (operation.PriceLocal * (decimal)operation.Quantity))
								/ (decimal)(inventory.WeightedAverageLastQuantity + operation.Quantity);

							inventory.WeightedAverageLastPrice = average;
							inventory.WeightedAverageLastQuantity += operation.Quantity;
						}

						inventory.WeightedAverageLastDate = (DateTime)operation["OperationDate"];
					}
					else if(operation.InventoryOperationRow.refInventoryOperationTypeId == new Guid((RuleEngine as RuleEngine).GetConstant("InventoryOperationType_WarehouseTransfer")))
					{
						// Transfer - we just update the operation's price
						operation.PriceLocal = inventory.WeightedAverageLastPrice;
					}
					else if(operation.InventoryOperationRow.refInventoryOperationTypeId == new Guid((RuleEngine as RuleEngine).GetConstant("InventoryOperationType_WarehouseRequisition"))
						| operation.InventoryOperationRow.refInventoryOperationTypeId == new Guid((RuleEngine as RuleEngine).GetConstant("InventoryOperationType_WarehouseStockTakingMissing"))
						)
					{
						// Requisition - we set the price
						operation.PriceLocal = inventory.WeightedAverageLastPrice;
						inventory.WeightedAverageLastQuantity -= operation.Quantity;
					}
					else
					{
						throw new ArgumentOutOfRangeException("InventoryOperation.refInventoryOperationTypeId", operation.InventoryOperationRow.refInventoryOperationTypeId, ResourceUtils.GetString("ErrorUnknownInventoryOperation"));
					}

					operation.IsWeightedAverageProcessed = true;

					operation.EndEdit();
				}

				inventory.EndEdit();
			}

			return new XmlDataDocument(data);
		}
		#endregion

		#region IServiceAgent Members
		private object _result;
		public override object Result
		{
			get
			{
				return _result;
			}
		}

		public override void Run()
		{
			switch(this.MethodName)
			{
				case "RecalculateWeightedAverage":
					// Check input parameters
					if(! (this.Parameters["InventoryOperations"] is XmlDataDocument))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorOperationsNotXmlDataDocument"));
					
					WeightedAverageRecalculation sourceData = new WeightedAverageRecalculation();
					sourceData.Merge((this.Parameters["InventoryOperations"] as XmlDataDocument).DataSet);

					_result = this.RecalculateWeightedAverage(sourceData);

					break;
			}
		}

		#endregion
	}
}

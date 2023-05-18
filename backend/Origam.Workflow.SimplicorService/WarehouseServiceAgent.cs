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
using System.Collections;
using System.Data;

using Origam.DA;
using Origam.Rule;
using Origam.Schema;
using Origam.Service.Core;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workflow.SimplicorService
{
	/// <summary>
	/// Summary description for TransformationAgent.
	/// </summary>
	public class WarehouseServiceAgent : AbstractServiceAgent
	{
		private static Guid _weightedAveragePricingMethodId;
		readonly IParameterService parameterService = ServiceManager.Services.GetService<IParameterService>();

		public WarehouseServiceAgent()
		{
			_weightedAveragePricingMethodId = (Guid)parameterService.GetParameterValue("WarehousePricingMethod_WeightedAverage");
		}

		#region Private Methods

		private string GetConstant(string name)
		{
			return (string)parameterService.GetParameterValue(name,
				OrigamDataType.String);
		}
		private IXmlContainer RecalculateWeightedAverage(PriceRecalculationData data)
		{
			Hashtable inventory = new Hashtable();

			data.InventoryOperationDetail.Columns.Add("OperationDate", typeof(System.DateTime), "Parent.Date");
			data.InventoryOperationDetail.Columns.Add("OperationType", typeof(System.Guid), "Parent.refInventoryOperationTypeId");
			data.InventoryOperationDetail.Columns.Add("IsCancelled", typeof(bool), "Parent.IsCancelled");
			data.InventoryOperationDetail.Columns.Add("IsCancellation", typeof(bool), "Parent.IsCancellation");
			data.InventoryOperationDetail.Columns.Add("SortOrder", typeof(int), "Parent.SortOrder");

			DataView operations = new DataView(data.InventoryOperationDetail,
				"",
				"OperationDate, IsCancelled DESC, IsCancellation DESC, SortOrder, RecordCreated",
				DataViewRowState.CurrentRows);

//			if(inventory.IsWeightedAverageLastPriceNull()) inventory.WeightedAverageLastPrice = 0;
//			if(inventory.IsWeightedAverageLastQuantityNull()) inventory.WeightedAverageLastQuantity = 0;

			//				inventory.WeightedAverageLastPrice = 0;
			//				inventory.WeightedAverageLastQuantity = 0;

			foreach(DataRowView operationRowView in operations)
			{
				PriceRecalculationData.InventoryOperationDetailRow operation = 
					(operationRowView.Row as PriceRecalculationData.InventoryOperationDetailRow);

				bool isOldWarehouseAverage = false;
				bool isNewWarehouseAverage = false;

				if(!operation.InventoryOperationRow.IsrefOldWarehouseIdNull())
				{
					isOldWarehouseAverage = IsWarehouseWeightedAverage(data, operation.InventoryOperationRow.refOldWarehouseId);
				}
				if(!operation.InventoryOperationRow.IsrefNewWarehouseIdNull())
				{
					isNewWarehouseAverage = IsWarehouseWeightedAverage(data, operation.InventoryOperationRow.refNewWarehouseId);
				}

				bool isWarehouseAverage = (isOldWarehouseAverage || isNewWarehouseAverage);

				Guid oldInventoryId = Guid.Empty;
				Guid newInventoryId = Guid.Empty;

				if(! operation.InventoryOperationRow.IsrefOldWarehouseIdNull())
				{
					oldInventoryId = GetInventoryId(operation.InventoryOperationRow.refOldWarehouseId, operation.refProductId);
				}
				if(! operation.InventoryOperationRow.IsrefNewWarehouseIdNull())
				{
					newInventoryId = GetInventoryId(operation.InventoryOperationRow.refNewWarehouseId, operation.refProductId);
				}

				// set initial balance - load from the db
				if(!inventory.Contains(oldInventoryId))
				{
					inventory.Add(oldInventoryId, 
						InitialBalance(oldInventoryId, operation.InventoryOperationRow.Date));
				}
				if(!inventory.Contains(newInventoryId))
				{
					inventory.Add(newInventoryId, 
						InitialBalance(newInventoryId, operation.InventoryOperationRow.Date));
				}

				// basic inventory is the new inventory (there we have to update the average price because of receipts and receipt-transfers)
				Guid inventoryId = (newInventoryId.Equals(Guid.Empty) ? oldInventoryId : newInventoryId);

				object[] o = (object[])inventory[inventoryId];
				decimal lastQuantity = (decimal)o[0];
				decimal lastPrice = (decimal)o[1];
				decimal totalPrice = (decimal)o[2];
				decimal factor = 1;

				if((bool)operation["IsCancelled"] || (bool)operation["IsCancellation"])
				{
					// CANCELED OPERATIONS - WE IGNORE THEM
					operation.SetPriceAverageNull();
					operation.PriceAverageQuantity = 0;
				}
				else if(operation.InventoryOperationRow.refInventoryOperationTypeId == 
						new Guid(GetConstant("InventoryOperationType_WarehouseReceipt"))
					|| operation.InventoryOperationRow.refInventoryOperationTypeId == 
						new Guid(GetConstant("InventoryOperationType_WarehouseStocktakingExtra"))
					|| operation.InventoryOperationRow.refInventoryOperationTypeId 
						== new Guid(GetConstant("InventoryOperationType_WarehouseReturnInbound"))
					)
				{
					// RECEIPTS
                    // check receipts from manufacturing - we have to set the price
                    if (!operation.IsrefManufacturingReportIdNull())
                    {
                        PriceRecalculationData.InventoryOperationDetailRow[] mrpRequisitions =
                            (PriceRecalculationData.InventoryOperationDetailRow[])data.InventoryOperationDetail.Select(
                            "refManufacturingReportId='" + operation.refManufacturingReportId.ToString()
                            + "' and OperationType = '" + GetConstant("InventoryOperationType_WarehouseRequisition") + "'"
                            );

                        decimal totalMrpPrice = 0;

                        foreach (PriceRecalculationData.InventoryOperationDetailRow requisition in mrpRequisitions)
                        {
                            totalMrpPrice += requisition.PriceLocalTotal;
                        }

                        operation.PriceLocal = decimal.Round(totalMrpPrice / operation.Quantity, 4);
                        operation.PriceForeign = operation.PriceLocal;
                    }
                    
                    decimal newQuantity = lastQuantity + (operation.Quantity * factor);
					if(newQuantity == 0)
					{
						lastPrice = 0;
						lastQuantity = 0;
						totalPrice = 0;
					}
					else
					{
						// recalculate the average
						decimal average = decimal.Round(((lastPrice * lastQuantity) + (operation.PriceLocalTotal * factor))
							/ (lastQuantity + (operation.Quantity * factor)), 4);

						lastPrice = average;
						lastQuantity = newQuantity;
						totalPrice += (operation.PriceLocalTotal * factor);
					}
				}
				else if(operation.InventoryOperationRow.refInventoryOperationTypeId == 
					new Guid(GetConstant("InventoryOperationType_WarehouseTransfer")))
				{
					// TRANSFERS
					object[] old = (object[])inventory[oldInventoryId];
					decimal oldQuantity = (decimal)old[0];
					decimal oldLastPrice = (decimal)old[1];
					decimal oldTotalPrice = (decimal)old[2];
					decimal newQuantity = oldQuantity - (operation.Quantity * factor);

					// Transfer - we just update the operation's price
					if(isWarehouseAverage)
					{
						operation.PriceLocal = oldLastPrice;
						operation.PriceForeign = oldLastPrice;
						if(newQuantity == 0)
						{
							// stock out the rest of the price, not just quantity x average
							if(operation.PriceLocalTotal != oldTotalPrice)
							{
								operation.PriceLocalCorrection = -(oldTotalPrice - operation.PriceLocalTotal);
							}
							else
							{
								operation.PriceLocalCorrection = 0;
							}
							// set the average of the old inventory to 0
							oldLastPrice = 0;
						}
					}

					if(lastQuantity + operation.Quantity == 0)
					{
						lastPrice = 0;
						lastQuantity = 0;
						totalPrice = 0;
					}
					else
					{
						// recalculate the average and quantity for the new warehouse
						decimal average = decimal.Round(((lastPrice * lastQuantity) + operation.PriceLocalTotal)
							/ (lastQuantity + operation.Quantity), 4);

						lastPrice = average;
						lastQuantity = lastQuantity + operation.Quantity;
						totalPrice += operation.PriceLocalTotal;
					}

					// update the old quantity
					inventory[oldInventoryId] = new object[] {
						oldQuantity - operation.Quantity,
						oldLastPrice,
						oldTotalPrice - operation.PriceLocalTotal + operation.PriceLocalCorrection};
					UpdateBalances(
						data, 
						oldInventoryId, 
						operation.InventoryOperationRow.Date,
						oldLastPrice, 
						oldTotalPrice - operation.PriceLocalTotal + operation.PriceLocalCorrection);
				}
				else if(operation.InventoryOperationRow.refInventoryOperationTypeId ==
						new Guid(GetConstant("InventoryOperationType_WarehouseRequisition"))
					|| operation.InventoryOperationRow.refInventoryOperationTypeId == 
						new Guid(GetConstant("InventoryOperationType_WarehouseStocktakingMissing"))
					|| operation.InventoryOperationRow.refInventoryOperationTypeId 
						== new Guid(GetConstant("InventoryOperationType_WarehouseReturnOutbound"))
					)
				{
					decimal newQuantity = lastQuantity - (operation.Quantity * factor);
					// Requisition - we set the price
					if(isWarehouseAverage)
					{
						operation.PriceLocal = lastPrice;
						operation.PriceForeign = lastPrice;
						if(newQuantity == 0 && operation.PriceLocalTotal != totalPrice)
						{
							operation.PriceLocalCorrection = -(totalPrice - operation.PriceLocalTotal);
						}
						else
						{
							operation.PriceLocalCorrection = 0;
						}
					}

					if(newQuantity == 0)
					{
						lastPrice = 0;
						lastQuantity = 0;
						totalPrice = 0;
					}
					else
					{
						lastQuantity = newQuantity;
						totalPrice -= (operation.PriceLocalTotal * factor);
					}
				}
				else
				{
					throw new ArgumentOutOfRangeException("InventoryOperation.refInventoryOperationTypeId", 
						operation.InventoryOperationRow.refInventoryOperationTypeId, 
						ResourceUtils.GetString("ErrorUnknownInventoryOperation"));
				}

				operation.PriceAverageQuantity = lastQuantity;

				if(isWarehouseAverage)
				{
					operation.PriceAverage = lastPrice;
				}
				
				operation.EndEdit();

				UpdateBalances(data, inventoryId, operation.InventoryOperationRow.Date, lastPrice, totalPrice);

				inventory[inventoryId] = new object[] {lastQuantity, lastPrice, totalPrice};
			}

			return DataDocumentFactory.New(data);
		}

		/// <summary>
		/// Loads initial inventory balances from the database.
		/// </summary>
		/// <param name="inventoryId"></param>
		/// <param name="date"></param>
		/// <returns></returns>
		private static object[] InitialBalance(Guid inventoryId, DateTime date)
		{
			// we load the balance for a day-1 so we get the
			// final quantity/price for the beginning of the requested date
			object balanceId = GetInventoryBalanceId(inventoryId, date.AddDays(-1));

			if(balanceId == null)
			{
				return new object[] {(decimal)0, (decimal)0, (decimal)0};
			}
			else
			{
				DataSet balance = core.DataService.Instance.LoadData(new Guid("d8df51c1-5596-446e-b3e7-68b3f2f54a1f"),
					new Guid("a36bef1a-1a8f-45cd-9085-df9e7ac0b6b6"), Guid.Empty, Guid.Empty,
					null, "InventoryBalance_parId", balanceId);

				DataRow row = balance.Tables[0].Rows[0];

				return new object[] {row["QuantityBalance"], row["PriceLocalAverage"], row["PriceLocalTotal"]};
			}
		}

		private static void UpdateBalances(PriceRecalculationData data, Guid inventoryId, 
			DateTime date, decimal lastPrice, decimal totalPrice)
		{
			// update the balance for the operation's date
			PriceRecalculationData.InventoryBalanceRow[] balances = 
				(PriceRecalculationData.InventoryBalanceRow[])data.InventoryBalance.Select(
				"refInventoryId='" + inventoryId.ToString() + "' and Date = " + DatasetTools.DateExpression(date),
				"Date DESC"
				);

			if(balances.Length == 0) 
				throw new Exception(ResourceUtils.GetString("ErrorBalanceNotFound", inventoryId.ToString(), date.ToString()));
			if(balances.Length > 1) 
				throw new Exception(ResourceUtils.GetString("ErrorMultipleBalances", inventoryId.ToString(), date.ToString()));

			PriceRecalculationData.InventoryBalanceRow balance = balances[0];
			balance.PriceLocalAverage = lastPrice;
			balance.PriceLocalTotal = totalPrice;
		}

		private static Guid GetInventoryId(Guid warehouseId, Guid productId)
		{
			IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

			Hashtable pms = new Hashtable(2);

			pms.Add("Inventory_parWarehouseId", warehouseId);
			pms.Add("Inventory_parProductId", productId);
			
			return (Guid)ls.GetDisplayText(new Guid("b198e1b2-be96-42ed-8602-43e19d443685"), pms, false, false, null);
		}

		private static object GetInventoryBalanceId(Guid inventoryId, DateTime date)
		{
			IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

			Hashtable pms = new Hashtable(2);

			pms.Add("InventoryBalance_parDate", date);
			pms.Add("InventoryBalance_parInventoryId", inventoryId);
			
			return ls.GetDisplayText(new Guid("11ba7945-5e11-4bc5-8424-d1d7600439fb"), pms, false, false, null);
		}

		private static bool IsWarehouseWeightedAverage(PriceRecalculationData data, Guid warehouseId)
		{
			PriceRecalculationData.WarehouseRow warehouse = data.Warehouse.FindById(warehouseId);
			return warehouse.refWarehousePricingMethodId == _weightedAveragePricingMethodId;
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
				case "RecalculatePrices":
					// Check input parameters
					if(! (this.Parameters["InventoryOperations"] is IDataDocument))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlDataDocument"));
					
					PriceRecalculationData sourceData = new PriceRecalculationData();
					sourceData.Merge((this.Parameters["InventoryOperations"] as IDataDocument).DataSet);

					_result = this.RecalculateWeightedAverage(sourceData);

					break;
			}
		}

		#endregion
	}
}

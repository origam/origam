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
using Origam.Schema;
using Origam.Service.Core;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workflow.SimplicorService;

/// <summary>
/// Summary description for TransformationAgent.
/// </summary>
public class WarehouseServiceAgent : AbstractServiceAgent
{
    private static Guid _weightedAveragePricingMethodId;
    readonly IParameterService parameterService =
        ServiceManager.Services.GetService<IParameterService>();

    public WarehouseServiceAgent()
    {
        _weightedAveragePricingMethodId = (Guid)
            parameterService.GetParameterValue(
                parameterName: "WarehousePricingMethod_WeightedAverage"
            );
    }

    #region Private Methods
    private string GetConstant(string name)
    {
        return (string)
            parameterService.GetParameterValue(
                parameterName: name,
                targetType: OrigamDataType.String
            );
    }

    private IXmlContainer RecalculateWeightedAverage(PriceRecalculationData data)
    {
        Hashtable inventory = new Hashtable();
        data.InventoryOperationDetail.Columns.Add(
            columnName: "OperationDate",
            type: typeof(System.DateTime),
            expression: "Parent.Date"
        );
        data.InventoryOperationDetail.Columns.Add(
            columnName: "OperationType",
            type: typeof(System.Guid),
            expression: "Parent.refInventoryOperationTypeId"
        );
        data.InventoryOperationDetail.Columns.Add(
            columnName: "IsCancelled",
            type: typeof(bool),
            expression: "Parent.IsCancelled"
        );
        data.InventoryOperationDetail.Columns.Add(
            columnName: "IsCancellation",
            type: typeof(bool),
            expression: "Parent.IsCancellation"
        );
        data.InventoryOperationDetail.Columns.Add(
            columnName: "SortOrder",
            type: typeof(int),
            expression: "Parent.SortOrder"
        );
        DataView operations = new DataView(
            table: data.InventoryOperationDetail,
            RowFilter: "",
            Sort: "OperationDate, IsCancelled DESC, IsCancellation DESC, SortOrder, RecordCreated",
            RowState: DataViewRowState.CurrentRows
        );
        //			if(inventory.IsWeightedAverageLastPriceNull()) inventory.WeightedAverageLastPrice = 0;
        //			if(inventory.IsWeightedAverageLastQuantityNull()) inventory.WeightedAverageLastQuantity = 0;
        //				inventory.WeightedAverageLastPrice = 0;
        //				inventory.WeightedAverageLastQuantity = 0;
        foreach (DataRowView operationRowView in operations)
        {
            PriceRecalculationData.InventoryOperationDetailRow operation = (
                operationRowView.Row as PriceRecalculationData.InventoryOperationDetailRow
            );
            bool isOldWarehouseAverage = false;
            bool isNewWarehouseAverage = false;
            if (!operation.InventoryOperationRow.IsrefOldWarehouseIdNull())
            {
                isOldWarehouseAverage = IsWarehouseWeightedAverage(
                    data: data,
                    warehouseId: operation.InventoryOperationRow.refOldWarehouseId
                );
            }
            if (!operation.InventoryOperationRow.IsrefNewWarehouseIdNull())
            {
                isNewWarehouseAverage = IsWarehouseWeightedAverage(
                    data: data,
                    warehouseId: operation.InventoryOperationRow.refNewWarehouseId
                );
            }
            bool isWarehouseAverage = (isOldWarehouseAverage || isNewWarehouseAverage);
            Guid oldInventoryId = Guid.Empty;
            Guid newInventoryId = Guid.Empty;
            if (!operation.InventoryOperationRow.IsrefOldWarehouseIdNull())
            {
                oldInventoryId = GetInventoryId(
                    warehouseId: operation.InventoryOperationRow.refOldWarehouseId,
                    productId: operation.refProductId
                );
            }
            if (!operation.InventoryOperationRow.IsrefNewWarehouseIdNull())
            {
                newInventoryId = GetInventoryId(
                    warehouseId: operation.InventoryOperationRow.refNewWarehouseId,
                    productId: operation.refProductId
                );
            }
            // set initial balance - load from the db
            if (!inventory.Contains(key: oldInventoryId))
            {
                inventory.Add(
                    key: oldInventoryId,
                    value: InitialBalance(
                        inventoryId: oldInventoryId,
                        date: operation.InventoryOperationRow.Date
                    )
                );
            }
            if (!inventory.Contains(key: newInventoryId))
            {
                inventory.Add(
                    key: newInventoryId,
                    value: InitialBalance(
                        inventoryId: newInventoryId,
                        date: operation.InventoryOperationRow.Date
                    )
                );
            }
            // basic inventory is the new inventory (there we have to update the average price because of receipts and receipt-transfers)
            Guid inventoryId = (
                newInventoryId.Equals(g: Guid.Empty) ? oldInventoryId : newInventoryId
            );
            object[] o = (object[])inventory[key: inventoryId];
            decimal lastQuantity = (decimal)o[0];
            decimal lastPrice = (decimal)o[1];
            decimal totalPrice = (decimal)o[2];
            decimal factor = 1;
            if (
                (bool)operation[columnName: "IsCancelled"]
                || (bool)operation[columnName: "IsCancellation"]
            )
            {
                // CANCELED OPERATIONS - WE IGNORE THEM
                operation.SetPriceAverageNull();
                operation.PriceAverageQuantity = 0;
            }
            else if (
                operation.InventoryOperationRow.refInventoryOperationTypeId
                    == new Guid(g: GetConstant(name: "InventoryOperationType_WarehouseReceipt"))
                || operation.InventoryOperationRow.refInventoryOperationTypeId
                    == new Guid(
                        g: GetConstant(name: "InventoryOperationType_WarehouseStocktakingExtra")
                    )
                || operation.InventoryOperationRow.refInventoryOperationTypeId
                    == new Guid(
                        g: GetConstant(name: "InventoryOperationType_WarehouseReturnInbound")
                    )
            )
            {
                // RECEIPTS
                // check receipts from manufacturing - we have to set the price
                if (!operation.IsrefManufacturingReportIdNull())
                {
                    PriceRecalculationData.InventoryOperationDetailRow[] mrpRequisitions =
                        (PriceRecalculationData.InventoryOperationDetailRow[])
                            data.InventoryOperationDetail.Select(
                                filterExpression: "refManufacturingReportId='"
                                    + operation.refManufacturingReportId.ToString()
                                    + "' and OperationType = '"
                                    + GetConstant(
                                        name: "InventoryOperationType_WarehouseRequisition"
                                    )
                                    + "'"
                            );
                    decimal totalMrpPrice = 0;
                    foreach (
                        PriceRecalculationData.InventoryOperationDetailRow requisition in mrpRequisitions
                    )
                    {
                        totalMrpPrice += requisition.PriceLocalTotal;
                    }
                    operation.PriceLocal = decimal.Round(
                        d: totalMrpPrice / operation.Quantity,
                        decimals: 4
                    );
                    operation.PriceForeign = operation.PriceLocal;
                }

                decimal newQuantity = lastQuantity + (operation.Quantity * factor);
                if (newQuantity == 0)
                {
                    lastPrice = 0;
                    lastQuantity = 0;
                    totalPrice = 0;
                }
                else
                {
                    // recalculate the average
                    decimal average = decimal.Round(
                        d: ((lastPrice * lastQuantity) + (operation.PriceLocalTotal * factor))
                            / (lastQuantity + (operation.Quantity * factor)),
                        decimals: 4
                    );
                    lastPrice = average;
                    lastQuantity = newQuantity;
                    totalPrice += (operation.PriceLocalTotal * factor);
                }
            }
            else if (
                operation.InventoryOperationRow.refInventoryOperationTypeId
                == new Guid(g: GetConstant(name: "InventoryOperationType_WarehouseTransfer"))
            )
            {
                // TRANSFERS
                object[] old = (object[])inventory[key: oldInventoryId];
                decimal oldQuantity = (decimal)old[0];
                decimal oldLastPrice = (decimal)old[1];
                decimal oldTotalPrice = (decimal)old[2];
                decimal newQuantity = oldQuantity - (operation.Quantity * factor);
                // Transfer - we just update the operation's price
                if (isWarehouseAverage)
                {
                    operation.PriceLocal = oldLastPrice;
                    operation.PriceForeign = oldLastPrice;
                    if (newQuantity == 0)
                    {
                        // stock out the rest of the price, not just quantity x average
                        if (operation.PriceLocalTotal != oldTotalPrice)
                        {
                            operation.PriceLocalCorrection = -(
                                oldTotalPrice - operation.PriceLocalTotal
                            );
                        }
                        else
                        {
                            operation.PriceLocalCorrection = 0;
                        }
                        // set the average of the old inventory to 0
                        oldLastPrice = 0;
                    }
                }
                if (lastQuantity + operation.Quantity == 0)
                {
                    lastPrice = 0;
                    lastQuantity = 0;
                    totalPrice = 0;
                }
                else
                {
                    // recalculate the average and quantity for the new warehouse
                    decimal average = decimal.Round(
                        d: ((lastPrice * lastQuantity) + operation.PriceLocalTotal)
                            / (lastQuantity + operation.Quantity),
                        decimals: 4
                    );
                    lastPrice = average;
                    lastQuantity = lastQuantity + operation.Quantity;
                    totalPrice += operation.PriceLocalTotal;
                }
                // update the old quantity
                inventory[key: oldInventoryId] = new object[]
                {
                    oldQuantity - operation.Quantity,
                    oldLastPrice,
                    oldTotalPrice - operation.PriceLocalTotal + operation.PriceLocalCorrection,
                };
                UpdateBalances(
                    data: data,
                    inventoryId: oldInventoryId,
                    date: operation.InventoryOperationRow.Date,
                    lastPrice: oldLastPrice,
                    totalPrice: oldTotalPrice
                        - operation.PriceLocalTotal
                        + operation.PriceLocalCorrection
                );
            }
            else if (
                operation.InventoryOperationRow.refInventoryOperationTypeId
                    == new Guid(g: GetConstant(name: "InventoryOperationType_WarehouseRequisition"))
                || operation.InventoryOperationRow.refInventoryOperationTypeId
                    == new Guid(
                        g: GetConstant(name: "InventoryOperationType_WarehouseStocktakingMissing")
                    )
                || operation.InventoryOperationRow.refInventoryOperationTypeId
                    == new Guid(
                        g: GetConstant(name: "InventoryOperationType_WarehouseReturnOutbound")
                    )
            )
            {
                decimal newQuantity = lastQuantity - (operation.Quantity * factor);
                // Requisition - we set the price
                if (isWarehouseAverage)
                {
                    operation.PriceLocal = lastPrice;
                    operation.PriceForeign = lastPrice;
                    if (newQuantity == 0 && operation.PriceLocalTotal != totalPrice)
                    {
                        operation.PriceLocalCorrection = -(totalPrice - operation.PriceLocalTotal);
                    }
                    else
                    {
                        operation.PriceLocalCorrection = 0;
                    }
                }
                if (newQuantity == 0)
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
                throw new ArgumentOutOfRangeException(
                    paramName: "InventoryOperation.refInventoryOperationTypeId",
                    actualValue: operation.InventoryOperationRow.refInventoryOperationTypeId,
                    message: ResourceUtils.GetString(key: "ErrorUnknownInventoryOperation")
                );
            }
            operation.PriceAverageQuantity = lastQuantity;
            if (isWarehouseAverage)
            {
                operation.PriceAverage = lastPrice;
            }

            operation.EndEdit();
            UpdateBalances(
                data: data,
                inventoryId: inventoryId,
                date: operation.InventoryOperationRow.Date,
                lastPrice: lastPrice,
                totalPrice: totalPrice
            );
            inventory[key: inventoryId] = new object[] { lastQuantity, lastPrice, totalPrice };
        }
        return DataDocumentFactory.New(dataSet: data);
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
        object balanceId = GetInventoryBalanceId(
            inventoryId: inventoryId,
            date: date.AddDays(value: -1)
        );
        if (balanceId == null)
        {
            return new object[] { (decimal)0, (decimal)0, (decimal)0 };
        }
        DataSet balance = core.DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "d8df51c1-5596-446e-b3e7-68b3f2f54a1f"),
            methodId: new Guid(g: "a36bef1a-1a8f-45cd-9085-df9e7ac0b6b6"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "InventoryBalance_parId",
            paramValue1: balanceId
        );
        DataRow row = balance.Tables[index: 0].Rows[index: 0];

        return new object[]
        {
            row[columnName: "QuantityBalance"],
            row[columnName: "PriceLocalAverage"],
            row[columnName: "PriceLocalTotal"],
        };
    }

    private static void UpdateBalances(
        PriceRecalculationData data,
        Guid inventoryId,
        DateTime date,
        decimal lastPrice,
        decimal totalPrice
    )
    {
        // update the balance for the operation's date
        PriceRecalculationData.InventoryBalanceRow[] balances =
            (PriceRecalculationData.InventoryBalanceRow[])
                data.InventoryBalance.Select(
                    filterExpression: "refInventoryId='"
                        + inventoryId.ToString()
                        + "' and Date = "
                        + DatasetTools.DateExpression(dateValue: date),
                    sort: "Date DESC"
                );
        if (balances.Length == 0)
        {
            throw new Exception(
                message: ResourceUtils.GetString(
                    key: "ErrorBalanceNotFound",
                    args: new object[] { inventoryId.ToString(), date.ToString() }
                )
            );
        }

        if (balances.Length > 1)
        {
            throw new Exception(
                message: ResourceUtils.GetString(
                    key: "ErrorMultipleBalances",
                    args: new object[] { inventoryId.ToString(), date.ToString() }
                )
            );
        }

        PriceRecalculationData.InventoryBalanceRow balance = balances[0];
        balance.PriceLocalAverage = lastPrice;
        balance.PriceLocalTotal = totalPrice;
    }

    private static Guid GetInventoryId(Guid warehouseId, Guid productId)
    {
        IDataLookupService ls =
            ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
            as IDataLookupService;
        Hashtable pms = new Hashtable(capacity: 2);
        pms.Add(key: "Inventory_parWarehouseId", value: warehouseId);
        pms.Add(key: "Inventory_parProductId", value: productId);

        return (Guid)
            ls.GetDisplayText(
                lookupId: new Guid(g: "b198e1b2-be96-42ed-8602-43e19d443685"),
                lookupValue: pms,
                useCache: false,
                returnMessageIfNull: false,
                transactionId: null
            );
    }

    private static object GetInventoryBalanceId(Guid inventoryId, DateTime date)
    {
        IDataLookupService ls =
            ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
            as IDataLookupService;
        Hashtable pms = new Hashtable(capacity: 2);
        pms.Add(key: "InventoryBalance_parDate", value: date);
        pms.Add(key: "InventoryBalance_parInventoryId", value: inventoryId);

        return ls.GetDisplayText(
            lookupId: new Guid(g: "11ba7945-5e11-4bc5-8424-d1d7600439fb"),
            lookupValue: pms,
            useCache: false,
            returnMessageIfNull: false,
            transactionId: null
        );
    }

    private static bool IsWarehouseWeightedAverage(PriceRecalculationData data, Guid warehouseId)
    {
        PriceRecalculationData.WarehouseRow warehouse = data.Warehouse.FindById(Id: warehouseId);
        return warehouse.refWarehousePricingMethodId == _weightedAveragePricingMethodId;
    }
    #endregion
    #region IServiceAgent Members
    private object _result;
    public override object Result
    {
        get { return _result; }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "RecalculatePrices":
            {
                // Check input parameters
                if (!(this.Parameters[key: "InventoryOperations"] is IDataDocument))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotXmlDataDocument")
                    );
                }

                PriceRecalculationData sourceData = new PriceRecalculationData();
                sourceData.Merge(
                    dataSet: (this.Parameters[key: "InventoryOperations"] as IDataDocument).DataSet
                );
                _result = this.RecalculateWeightedAverage(data: sourceData);
                break;
            }
        }
    }
    #endregion
}

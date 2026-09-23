using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Enums
{
    public enum EDictionaryType
    {
        [Description("Waluty")]
        Currencies = 1,
        [Description("Jednostki miary")]
        UnitsOfMeasure = 2,
        [Description("Jednostki rozliczeniowe")]
        BillingUnits = 3,
        [Description("Kategorie produktów")]
        ProductCategories = 4,
        [Description("Typy produktów")]
        ProductTypes = 5,
        [Description("Stawki VAT")]
        VatRates = 6,
        [Description("Statusy płatności kontrahentów")]
        CustomerPaymentStates = 7,
        [Description("Statusy zadań CRM")]
        CrmTaskStates = 8,
        [Description("Priorytety zadań CRM")]
        CrmTasksPriorities = 9,
        [Description("Źródło zadania w CRM")]
        TaskSource = 10,
        [Description("Własne")]
        Custom = 99
    }
}

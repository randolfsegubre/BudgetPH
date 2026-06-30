namespace FinanceManager.Domain.Enums;

public enum AccountType
{
    Checking = 1,
    Savings = 2,
    CreditCard = 3,
    Investment = 4,
    Loan = 5,
    Cash = 6,
    Other = 7
}

public enum LoanType
{
    Personal = 1,
    Auto = 2,
    Mortgage = 3,
    Student = 4,
    Business = 5,
    Medical = 6,
    HomeEquity = 7,
    HELOC = 8,
    PayDay = 9,
    Other = 10
}

public enum InvestmentType
{
    Stock = 1,
    Bond = 2,
    ETF = 3,
    MutualFund = 4,
    Cryptocurrency = 5,
    RealEstate = 6,
    CD = 7,
    Treasury = 8,
    Option = 9,
    Other = 10
}

public enum InvestmentAccountType
{
    Brokerage = 1,
    IRA = 2,
    RothIRA = 3,
    Plan401k = 4,
    Plan403b = 5,
    Plan529 = 6,
    HSA = 7,
    Other = 8
}

public enum TransactionType
{
    Income = 1,
    Expense = 2,
    Transfer = 3,
    LoanPayment = 4,
    InvestmentBuy = 5,
    InvestmentSell = 6,
    CreditCardPayment = 7,
    SavingsDeposit = 8,
    Refund = 9,
    Fee = 10,
    Interest = 11,
    Dividend = 12
}

public enum TransactionStatus
{
    Pending = 1,
    Cleared = 2,
    Reconciled = 3,
    Cancelled = 4
}

public enum RecurrenceFrequency
{
    Daily = 1,
    Weekly = 2,
    BiWeekly = 3,
    SemiMonthly = 4,
    Monthly = 5,
    Quarterly = 6,
    SemiAnnual = 7,
    Annual = 8
}

public enum IncomeType
{
    Salary = 1,
    HourlyWage = 2,
    Freelance = 3,
    Business = 4,
    Rental = 5,
    Dividend = 6,
    Interest = 7,
    CapitalGains = 8,
    Pension = 9,
    SocialSecurity = 10,
    Bonus = 11,
    Commission = 12,
    Alimony = 13,
    ChildSupport = 14,
    GovernmentBenefit = 15,
    Other = 16
}

public enum CategoryType
{
    Income = 1,
    Expense = 2,
    Transfer = 3
}

public enum DocumentType
{
    BankStatement = 1,
    CreditCardStatement = 2,
    LoanStatement = 3,
    InvestmentStatement = 4,
    TaxDocument = 5,
    Receipt = 6,
    Invoice = 7,
    Contract = 8,
    Other = 9
}

public enum ReportType
{
    MonthlyBudget = 1,
    AnnualSummary = 2,
    NetWorthStatement = 3,
    CashFlowStatement = 4,
    DebtSummary = 5,
    InvestmentPerformance = 6,
    TaxSummary = 7,
    SpendingAnalysis = 8,
    IncomeAnalysis = 9,
    CustomDateRange = 10
}

public enum Currency
{
    PHP = 1,  // Philippine Peso — primary market
    USD = 2,
    EUR = 3,
    GBP = 4,
    JPY = 5,  // Japanese Yen — OFW destination
    CAD = 6,
    AUD = 7,
    CHF = 8,
    CNY = 9,
    SGD = 10, // Singapore Dollar — large PH expat/OFW destination
    HKD = 11, // Hong Kong Dollar — large PH expat/OFW destination
    MXN = 12,
    BRL = 13,
    INR = 14,
    NZD = 15,
    SAR = 16, // Saudi Riyal — largest OFW destination
    AED = 17, // UAE Dirham — large OFW destination
    KWD = 18, // Kuwaiti Dinar — OFW destination
    QAR = 19, // Qatari Riyal — OFW destination
    BHD = 20, // Bahraini Dinar — OFW destination
    OMR = 21, // Omani Rial — OFW destination
    KRW = 22  // South Korean Won — OFW destination
}

public enum Gender
{
    Male = 1,
    Female = 2,
    NonBinary = 3,
    PreferNotToSay = 4
}

public enum NotificationType
{
    BudgetExceeded = 1,
    BillDue = 2,
    LoanPaymentDue = 3,
    GoalAchieved = 4,
    LargeTransaction = 5,
    LowBalance = 6,
    HighSpending = 7,
    DocumentProcessed = 8
}

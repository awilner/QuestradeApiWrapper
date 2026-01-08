using System.Text.Json.Serialization;

namespace QuestradeApiWrapper.Model;

// https://www.questrade.com/api/documentation/rest-operations/enumerations/enumerations

public enum Currency
{
    USD,
    CAD
}

public enum ListingExchange
{
    TSX,
    TSXV,
    CNSX,
    MX,
    NASDAQ,
    NYSE,
    NYSEAM,
    ARCA,
    OPRA,
    PinkSheets,
    OTCBB
}

public enum AccountType
{
    Cash,
    Margin,
    TFSA,
    RRSP,
    FHSA,
    SRRSP,
    LRRSP,
    LIRA,
    LIF,
    RIF,
    SRIF,
    LRIF,
    RRIF,
    PRIF,
    RESP,
    FRESP
}

public enum ClientAccountType
{
    [JsonPropertyName("Individual")]
    Individual,
    [JsonPropertyName("Joint")]
    Joint,
    [JsonPropertyName("Informal Trust")]
    InformalTrust,
    [JsonPropertyName("Corporation")]
    Corporation,
    [JsonPropertyName("Formal Trust")]
    FormalTrust,
    [JsonPropertyName("Partnership")]
    Partnership,
    [JsonPropertyName("Sole Proprietorship")]
    SoleProprietorship,
    [JsonPropertyName("Family")]
    Family,
    [JsonPropertyName("Joint and Informal Trust")]
    JointAndInformalTrust,
    [JsonPropertyName("Institution")]
    Institution
}

public enum AccountStatus
{
    [JsonPropertyName("Active")]
    Active,
    [JsonPropertyName("Suspended (Closed)")]
    SuspendedClosed,
    [JsonPropertyName("Suspended (View Only)")]
    SuspendedViewOnly,
    [JsonPropertyName("Liquidate Only")]
    LiquidateOnly,
    [JsonPropertyName("Closed")]
    Closed
}

public enum OrderSide
{
    Buy,
    Sell,
    Short,
    Cov,
    BTO,
    STC,
    STO,
    BTC
}

public enum OrderType
{
    Market,
    Limit,
    Stop,
    StopLimit,
    TrailStopInPercentage,
    TrailStopInDollar,
    TrailStopLimitInPercentage,
    TrailStopLimitInDollar,
    LimitOnOpen,
    LimitOnClose
}

public enum OrderTimeInForce
{
    Day,
    GoodTillCanceled,
    GoodTillExtendedDay,
    GoodTillDate,
    ImmediateOrCancel,
    FillOrKill
}

public enum OrderState
{
    Failed,
    Pending,
    Accepted,
    Rejected,
    CancelPending,
    Canceled,
    PartialCanceled,
    Partial,
    Executed,
    ReplacePending,
    Replaced,
    Stopped,
    Suspended,
    Expired,
    Queued,
    Triggered,
    Activated,
    PendingRiskReview,
    ContingentOrder
}

public enum OrderClass
{
    Primary,
    Limit,
    StopLoss
}

public enum StrategyType
{
    SingleLeg,
    CoveredCall,
    MarriedPuts,
    VerticalCallSpread,
    VerticalPutSpread,
    CalendarCallSpread,
    CalendarPutSpread,
    DiagonalCallSpread,
    DiagonalPutSpread,
    Collar,
    Straddle,
    Strangle,
    ButterflyCall,
    ButterflyPut,
    IronButterfly,
    CondorCall,
    Custom
}

public enum OrderStateFilterType
{
    All,
    Open,
    Closed
}
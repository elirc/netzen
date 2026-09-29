namespace SupportDesk.Api.Domain;

public enum TicketStatus
{
    New,
    Open,
    Pending,
    OnHold,
    Solved,
    Closed
}

public enum TicketPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum CommentVisibility
{
    Public,
    Internal
}

public enum SupportRole
{
    Customer,
    Agent,
    Manager,
    Admin
}

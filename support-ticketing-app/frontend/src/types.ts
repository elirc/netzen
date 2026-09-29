export type TicketStatus = "New" | "Open" | "Pending" | "OnHold" | "Solved" | "Closed";
export type TicketPriority = "Low" | "Normal" | "High" | "Urgent";
export type CommentVisibility = "Public" | "Internal";

export type TicketSummary = {
  id: string;
  number: string;
  subject: string;
  customerName: string;
  customerEmail: string;
  status: TicketStatus;
  priority: TicketPriority;
  assigneeId: string | null;
  teamId: string | null;
  tags: string[];
  createdAt: string;
  updatedAt: string;
  firstResponseDueAt: string;
  resolutionDueAt: string;
  firstRespondedAt: string | null;
  closedAt: string | null;
  firstResponseBreached: boolean;
  resolutionBreached: boolean;
  commentCount: number;
  version: number;
};

export type TicketComment = {
  id: string;
  authorId: string;
  authorName: string;
  body: string;
  visibility: CommentVisibility;
  createdAt: string;
};

export type TicketDetail = TicketSummary & {
  description: string;
  comments: TicketComment[];
};

export type CannedReply = {
  id: string;
  title: string;
  body: string;
  tags: string[];
  suggestedPriority: TicketPriority | null;
};

export type PagedResponse<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};

export type TicketFilters = {
  query: string;
  status: "" | TicketStatus;
  priority: "" | TicketPriority;
  tag: string;
  breached: boolean;
};

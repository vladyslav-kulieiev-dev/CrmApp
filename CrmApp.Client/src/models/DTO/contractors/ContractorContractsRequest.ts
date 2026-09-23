export interface ContractorContractsRequest {
  contractorId?: number;
  engagementTypes?: number[];
  activeOnly?: boolean;
  hasHoursDebt?: boolean;
  isOverLimitCurrentMonth?: boolean;
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDescending: boolean;
}
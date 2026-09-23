export interface UserWorkloadDTO {
    userId: number;
    fullName: string;
    initials: string;
    totalTasks: number;
    overdueTasks: number;
    workloadPercent: number;
}
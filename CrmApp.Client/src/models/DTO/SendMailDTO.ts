export interface SendMailDTO {
    to: string;
    toName: string;
    subject: string;
    body: string;
    taskId?: number;
    projectId?: number;
}
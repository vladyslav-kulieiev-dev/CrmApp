export class ResultDTO<T> {
    succeeded: boolean = false;
    errors?: string[];
    messages?: string[];
    data?: T;
    objectId?: string;
}

export class ErrorResultDTO<T> extends ResultDTO<T> {
    
    constructor(errors: string[]) {
        super();
        this.succeeded = false;
        this.errors = errors;
        this.messages = [];
    }
}

export class SuccessResultDTO<T> extends ResultDTO<T> {
    constructor(objectId?: string, messages?: string[]) {
        super();
        this.succeeded = true;
        this.objectId = objectId;
        this.messages = messages || [];
    }
}
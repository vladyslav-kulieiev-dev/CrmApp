import { UsedResourceDTO } from "./ChatDTO";

export class MessageDTO {

    constructor(_type: 'ai' | 'user', _msg: string, _succeeded: boolean = true, _usedResources?: UsedResourceDTO[]) {
        this.type = _type;
        this.message = _msg;
        this.documents = _usedResources ?? [];
    }

    type: 'ai' | 'user';
    message: string;
    documents: UsedResourceDTO[] = [];
    succeeded: boolean = true;
}
import { AdditionalFieldDTO } from "./DTO/AdditionalFieldDTO";
import { AdditionalFieldValueDTO } from "./DTO/AdditionalFieldValueDTO";

export class ExtendableClass {
    additionalFields: AdditionalFieldDTO[] = [];
    additionalFieldValues: AdditionalFieldValueDTO[] = [];
}